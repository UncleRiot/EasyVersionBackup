using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EasyVersionBackup
{
    public static class VersionHelperGit
    {
        private const string LatestReleaseApiUrl =
            "https://api.github.com/repos/UncleRiot/EasyVersionBackup/releases/latest";

        private static readonly HttpClient HttpClient =
            CreateHttpClient();

        public static async Task<VersionHelperGitResult> CheckForUpdateAsync(
            string currentVersion,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using HttpResponseMessage response =
                    await HttpClient.GetAsync(
                        LatestReleaseApiUrl,
                        cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return VersionHelperGitResult.CreateConnectionFailed(
                        $"GitHub returned HTTP {(int)response.StatusCode}.");
                }

                string json =
                    await response.Content.ReadAsStringAsync(
                        cancellationToken);

                using JsonDocument document =
                    JsonDocument.Parse(json);

                JsonElement root = document.RootElement;

                string latestVersionText =
                    root.TryGetProperty(
                        "tag_name",
                        out JsonElement tagNameElement)
                        ? tagNameElement.GetString() ??
                          string.Empty
                        : string.Empty;

                string downloadUrl =
                    root.TryGetProperty(
                        "html_url",
                        out JsonElement htmlUrlElement)
                        ? htmlUrlElement.GetString() ??
                          string.Empty
                        : string.Empty;

                if (!TryParseVersion(
                        currentVersion,
                        out Version currentParsedVersion))
                {
                    return VersionHelperGitResult.CreateConnectionFailed(
                        "The installed version could not be parsed.");
                }

                if (!TryParseVersion(
                        latestVersionText,
                        out Version latestParsedVersion))
                {
                    return VersionHelperGitResult.CreateConnectionFailed(
                        "The GitHub release version could not be parsed.");
                }

                if (latestParsedVersion <=
                    currentParsedVersion)
                {
                    return VersionHelperGitResult.CreateNoUpdate();
                }

                return VersionHelperGitResult.CreateUpdateAvailable(
                    latestVersionText,
                    downloadUrl);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                return VersionHelperGitResult.CreateConnectionFailed(
                    "The GitHub update request timed out.");
            }
            catch (HttpRequestException exception)
            {
                return VersionHelperGitResult.CreateConnectionFailed(
                    exception.Message);
            }
            catch (JsonException exception)
            {
                return VersionHelperGitResult.CreateConnectionFailed(
                    "GitHub returned invalid release data: " +
                    exception.Message);
            }
            catch (Exception exception)
            {
                return VersionHelperGitResult.CreateConnectionFailed(
                    exception.Message);
            }
        }

        private static HttpClient CreateHttpClient()
        {
            HttpClient httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };

            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "EasyVersionBackup");

            return httpClient;
        }

        private static bool TryParseVersion(
            string versionText,
            out Version version)
        {
            version = new Version(0, 0, 0);

            if (string.IsNullOrWhiteSpace(versionText))
            {
                return false;
            }

            string normalizedVersionText =
                versionText.Trim();

            if (normalizedVersionText.StartsWith(
                "v",
                StringComparison.OrdinalIgnoreCase))
            {
                normalizedVersionText =
                    normalizedVersionText.Substring(1);
            }

            if (!Version.TryParse(
                    normalizedVersionText,
                    out Version? parsedVersion) ||
                parsedVersion == null)
            {
                return false;
            }

            version = parsedVersion;
            return true;
        }
    }

    public sealed class VersionHelperGitResult
    {
        private VersionHelperGitResult(
            bool canConnectToGitHub,
            bool updateAvailable,
            string latestVersion,
            string downloadUrl,
            string errorMessage)
        {
            CanConnectToGitHub = canConnectToGitHub;
            UpdateAvailable = updateAvailable;
            LatestVersion = latestVersion;
            DownloadUrl = downloadUrl;
            ErrorMessage = errorMessage;
        }

        public bool CanConnectToGitHub { get; }
        public bool UpdateAvailable { get; }
        public string LatestVersion { get; }
        public string DownloadUrl { get; }
        public string ErrorMessage { get; }

        public static VersionHelperGitResult CreateConnectionFailed(
            string errorMessage)
        {
            return new VersionHelperGitResult(
                false,
                false,
                string.Empty,
                string.Empty,
                errorMessage);
        }

        public static VersionHelperGitResult CreateNoUpdate()
        {
            return new VersionHelperGitResult(
                true,
                false,
                string.Empty,
                string.Empty,
                string.Empty);
        }

        public static VersionHelperGitResult CreateUpdateAvailable(
            string latestVersion,
            string downloadUrl)
        {
            return new VersionHelperGitResult(
                true,
                true,
                latestVersion,
                downloadUrl,
                string.Empty);
        }
    }
}
