using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace EasyVersionBackup
{
    public static class VersionHelperGit
    {
        private const string LatestReleaseApiUrl = "https://api.github.com/repos/UncleRiot/EasyVersionBackup/releases/latest";

        public static async Task<VersionHelperGitResult> CheckForUpdateAsync(string currentVersion)
        {
            try
            {
                using HttpClient httpClient = new HttpClient();

                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("EasyVersionBackup");

                using HttpResponseMessage response = await httpClient.GetAsync(LatestReleaseApiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    return VersionHelperGitResult.CreateConnectionFailed();
                }

                string json = await response.Content.ReadAsStringAsync();

                using JsonDocument document = JsonDocument.Parse(json);
                JsonElement root = document.RootElement;

                string latestVersionText = root.TryGetProperty("tag_name", out JsonElement tagNameElement)
                    ? tagNameElement.GetString() ?? string.Empty
                    : string.Empty;

                string downloadUrl = root.TryGetProperty("html_url", out JsonElement htmlUrlElement)
                    ? htmlUrlElement.GetString() ?? string.Empty
                    : string.Empty;

                if (!TryParseVersion(currentVersion, out Version currentParsedVersion) ||
                    !TryParseVersion(latestVersionText, out Version latestParsedVersion))
                {
                    return VersionHelperGitResult.CreateConnectionFailed();
                }

                if (latestParsedVersion <= currentParsedVersion)
                {
                    return VersionHelperGitResult.CreateNoUpdate();
                }

                return VersionHelperGitResult.CreateUpdateAvailable(latestVersionText, downloadUrl);
            }
            catch
            {
                return VersionHelperGitResult.CreateConnectionFailed();
            }
        }

        private static bool TryParseVersion(string versionText, out Version version)
        {
            version = new Version(0, 0, 0);

            string normalizedVersionText = versionText.Trim();

            if (normalizedVersionText.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                normalizedVersionText = normalizedVersionText.Substring(1);
            }

            return Version.TryParse(normalizedVersionText, out version!);
        }
    }

    public sealed class VersionHelperGitResult
    {
        private VersionHelperGitResult(bool canConnectToGitHub, bool updateAvailable, string latestVersion, string downloadUrl)
        {
            CanConnectToGitHub = canConnectToGitHub;
            UpdateAvailable = updateAvailable;
            LatestVersion = latestVersion;
            DownloadUrl = downloadUrl;
        }

        public bool CanConnectToGitHub { get; }
        public bool UpdateAvailable { get; }
        public string LatestVersion { get; }
        public string DownloadUrl { get; }

        public static VersionHelperGitResult CreateConnectionFailed()
        {
            return new VersionHelperGitResult(false, false, string.Empty, string.Empty);
        }

        public static VersionHelperGitResult CreateNoUpdate()
        {
            return new VersionHelperGitResult(true, false, string.Empty, string.Empty);
        }

        public static VersionHelperGitResult CreateUpdateAvailable(string latestVersion, string downloadUrl)
        {
            return new VersionHelperGitResult(true, true, latestVersion, downloadUrl);
        }
    }
}