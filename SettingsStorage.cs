using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace EasyVersionBackup
{
    public static class SettingsStorage
    {
        private const int CurrentSettingsSchemaVersion = 3;

        private static readonly string SettingsDirectoryPath =
            GetSettingsDirectoryPath();

        private static readonly string SettingsFilePath =
            Path.Combine(SettingsDirectoryPath, "EasyVersionBackup.settings.json");

        private static readonly string PreviousUserProfileSettingsFilePath =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "EasyVersionBackup",
                "EasyVersionBackup.settings.json");

        public static string LastLoadErrorMessage { get; private set; } = string.Empty;

        public static AppSettings Load()
        {
            LastLoadErrorMessage = string.Empty;

            string sourceSettingsFilePath =
                File.Exists(SettingsFilePath)
                    ? SettingsFilePath
                    : File.Exists(PreviousUserProfileSettingsFilePath)
                        ? PreviousUserProfileSettingsFilePath
                        : SettingsFilePath;

            try
            {
                if (!File.Exists(sourceSettingsFilePath))
                {
                    return CreateDefaultSettings();
                }

                string json = File.ReadAllText(sourceSettingsFilePath);
                AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(json);

                if (settings == null)
                {
                    throw new JsonException("Settings file is empty or invalid.");
                }

                ApplyLegacyMigrations(json, settings);
                EnsureSettingsInitialized(settings);

                if (string.Equals(
                        sourceSettingsFilePath,
                        PreviousUserProfileSettingsFilePath,
                        StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        Save(settings);
                    }
                    catch (Exception migrationException)
                    {
                        LastLoadErrorMessage =
                            $"Settings were loaded from the previous user-profile location, but could not be migrated to the application Settings directory.{Environment.NewLine}{migrationException.Message}";
                    }
                }

                return settings;
            }
            catch (Exception exception)
            {
                string backupPath = TryBackupInvalidSettingsFile(
                    sourceSettingsFilePath);

                LastLoadErrorMessage = string.IsNullOrWhiteSpace(backupPath)
                    ? $"Settings could not be loaded. Default settings are being used.{Environment.NewLine}{exception.Message}"
                    : $"Settings could not be loaded. Default settings are being used.{Environment.NewLine}{exception.Message}{Environment.NewLine}{Environment.NewLine}The invalid file was copied to:{Environment.NewLine}{backupPath}";

                return CreateDefaultSettings();
            }
        }

        public static bool TryImportFromFile(
            string filePath,
            out AppSettings importedSettings,
            out string errorMessage)
        {
            importedSettings = CreateDefaultSettings();
            errorMessage = string.Empty;

            try
            {
                if (!File.Exists(filePath))
                {
                    errorMessage = "Settings file does not exist.";
                    return false;
                }

                string json = File.ReadAllText(filePath);
                AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(json);

                if (settings == null)
                {
                    errorMessage = "Settings file is empty or invalid.";
                    return false;
                }

                ApplyLegacyMigrations(json, settings);
                EnsureSettingsInitialized(settings);
                importedSettings = settings;
                return true;
            }
            catch (Exception exception)
            {
                errorMessage = exception.Message;
                return false;
            }
        }

        public static void ExportToFile(AppSettings settings, string filePath)
        {
            EnsureSettingsInitialized(settings);
            string json = Serialize(settings);
            WriteFileAtomically(filePath, json);
        }

        public static void Save(AppSettings settings)
        {
            EnsureSettingsInitialized(settings);
            Directory.CreateDirectory(SettingsDirectoryPath);

            string json = Serialize(settings);
            WriteFileAtomically(SettingsFilePath, json);
        }

        public static string CreatePairKey(
            string sourceDirectory,
            string targetDirectory)
        {
            return NormalizePathForKey(sourceDirectory) +
                "|" +
                NormalizePathForKey(targetDirectory);
        }

        private static void EnsureSettingsInitialized(AppSettings settings)
        {
            settings.SettingsSchemaVersion = CurrentSettingsSchemaVersion;

            settings.BackupPathPairs ??= new List<BackupPathPair>();
            settings.BackupPathPairs = settings.BackupPathPairs
                .Where(pair => pair != null)
                .ToList();

            foreach (BackupPathPair pair in settings.BackupPathPairs)
            {
                pair.SourceDirectory ??= string.Empty;
                pair.TargetDirectory ??= string.Empty;
                pair.Versioning ??= string.Empty;
                pair.RetentionMode = BackupHelper.NormalizeRetentionMode(pair.RetentionMode);
                pair.ExcludedPaths ??= new List<string>();
                pair.RetentionExcludedTags ??= new List<string>();
                pair.SourceCleanupRelativeDirectory ??= string.Empty;
                pair.SourceCleanupFileExtensions ??= new List<string>();
                pair.SourceCleanupMode = SourceCleanupService.NormalizeMode(pair.SourceCleanupMode);
                pair.SourceCleanupKeepAfterDate ??= string.Empty;

                if (pair.SourceCleanupKeepLastCount < 1)
                {
                    pair.SourceCleanupKeepLastCount = 10;
                }

                if (pair.AutoBackupIntervalSeconds < 0)
                {
                    pair.AutoBackupIntervalSeconds = 0;
                }

                if (pair.RetentionKeepLastCount < 1)
                {
                    pair.RetentionKeepLastCount = 10;
                }

                if (pair.RetentionKeepDaysCount < 1)
                {
                    pair.RetentionKeepDaysCount = 14;
                }
            }

            settings.LastUsedVersionsByPair ??= new Dictionary<string, string>();
            settings.BackupStatusesByPair ??= new Dictionary<string, BackupPathStatus>();
            settings.Tags ??= new List<string>();

            settings.Tags = settings.Tags
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(tag => tag, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (settings.AutoBackupIntervalSeconds < 1)
            {
                settings.AutoBackupIntervalSeconds = 900;
            }

            if (settings.BackupVersionDialogWidth <= 0)
            {
                settings.BackupVersionDialogWidth = 560;
            }

            if (settings.BackupVersionDialogHeight <= 0)
            {
                settings.BackupVersionDialogHeight = 330;
            }

            if (settings.BackupInfoDialogWidth <= 0)
            {
                settings.BackupInfoDialogWidth = 760;
            }

            if (settings.BackupInfoDialogHeight <= 0)
            {
                settings.BackupInfoDialogHeight = 430;
            }

            settings.LogLevel = BackupLogger.NormalizeLogLevel(settings.LogLevel);
            settings.BackupDestinationConflictHandling =
                BackupHelper.NormalizeDestinationConflictHandling(
                    settings.BackupDestinationConflictHandling);

            MigratePairStateKeys(settings);
        }

        private static void MigratePairStateKeys(AppSettings settings)
        {
            Dictionary<string, string> migratedVersions =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            Dictionary<string, BackupPathStatus> migratedStatuses =
                new Dictionary<string, BackupPathStatus>(StringComparer.OrdinalIgnoreCase);

            foreach (BackupPathPair pair in settings.BackupPathPairs)
            {
                string normalizedKey = CreatePairKey(
                    pair.SourceDirectory,
                    pair.TargetDirectory);

                if (TryGetPairValue(
                    settings.LastUsedVersionsByPair,
                    pair,
                    out string? version))
                {
                    migratedVersions[normalizedKey] = version ?? string.Empty;
                }

                if (TryGetPairValue(
                    settings.BackupStatusesByPair,
                    pair,
                    out BackupPathStatus? status) &&
                    status != null)
                {
                    migratedStatuses[normalizedKey] = status;
                }
            }

            settings.LastUsedVersionsByPair = migratedVersions;
            settings.BackupStatusesByPair = migratedStatuses;
        }

        private static bool TryGetPairValue<T>(
            Dictionary<string, T> values,
            BackupPathPair pair,
            out T? value)
        {
            string normalizedKey = CreatePairKey(
                pair.SourceDirectory,
                pair.TargetDirectory);

            if (values.TryGetValue(normalizedKey, out value))
            {
                return true;
            }

            string legacyKey = pair.SourceDirectory + "|" + pair.TargetDirectory;

            if (values.TryGetValue(legacyKey, out value))
            {
                return true;
            }

            foreach (KeyValuePair<string, T> entry in values)
            {
                int separatorIndex = entry.Key.IndexOf('|');

                if (separatorIndex < 0)
                {
                    continue;
                }

                string sourceDirectory = entry.Key.Substring(0, separatorIndex);
                string targetDirectory = entry.Key.Substring(separatorIndex + 1);

                if (string.Equals(
                    CreatePairKey(sourceDirectory, targetDirectory),
                    normalizedKey,
                    StringComparison.OrdinalIgnoreCase))
                {
                    value = entry.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private static void ApplyLegacyMigrations(
            string json,
            AppSettings settings)
        {
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;

            if (!root.TryGetProperty(
                    nameof(AppSettings.AutoBackupIntervalSeconds),
                    out _) &&
                root.TryGetProperty(
                    "AutoBackupIntervalMinutes",
                    out JsonElement minutesElement) &&
                minutesElement.TryGetInt32(out int minutes) &&
                minutes > 0)
            {
                long seconds = (long)minutes * 60L;
                settings.AutoBackupIntervalSeconds =
                    seconds > int.MaxValue
                        ? int.MaxValue
                        : (int)seconds;
            }
        }

        private static string NormalizePathForKey(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            try
            {
                return BackupHelper.NormalizeDirectoryPath(path)
                    .ToUpperInvariant();
            }
            catch
            {
                return path.Trim()
                    .Replace(
                        Path.AltDirectorySeparatorChar,
                        Path.DirectorySeparatorChar)
                    .TrimEnd(
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar)
                    .ToUpperInvariant();
            }
        }

        private static string Serialize(AppSettings settings)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            return JsonSerializer.Serialize(settings, options);
        }

        private static void WriteFileAtomically(
            string filePath,
            string content)
        {
            string fullFilePath = Path.GetFullPath(filePath);
            string? directoryPath = Path.GetDirectoryName(fullFilePath);

            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new IOException(
                    $"Target directory could not be determined: {filePath}");
            }

            Directory.CreateDirectory(directoryPath);

            string temporaryPath = fullFilePath +
                ".tmp-" +
                Guid.NewGuid().ToString("N");

            string rollbackPath = fullFilePath +
                ".bak-" +
                Guid.NewGuid().ToString("N");

            File.WriteAllText(temporaryPath, content);

            try
            {
                if (!File.Exists(fullFilePath))
                {
                    File.Move(temporaryPath, fullFilePath);
                    return;
                }

                File.Replace(
                    temporaryPath,
                    fullFilePath,
                    rollbackPath,
                    true);

                TryDeleteTemporaryFile(rollbackPath);
            }
            finally
            {
                TryDeleteTemporaryFile(temporaryPath);
            }
        }

        private static void TryDeleteTemporaryFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Temporary settings file could not be removed: " +
                    exception.Message);
            }
        }

        private static string TryBackupInvalidSettingsFile(
            string sourceSettingsFilePath)
        {
            try
            {
                if (!File.Exists(sourceSettingsFilePath))
                {
                    return string.Empty;
                }

                Directory.CreateDirectory(SettingsDirectoryPath);

                string backupPath = Path.Combine(
                    SettingsDirectoryPath,
                    "EasyVersionBackup.settings.corrupt-" +
                    DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") +
                    ".json");

                File.Copy(
                    sourceSettingsFilePath,
                    backupPath,
                    false);

                return backupPath;
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Invalid settings file could not be backed up: " +
                    exception.Message);
                return string.Empty;
            }
        }

        private static string GetSettingsDirectoryPath()
        {
            return Path.Combine(
                AppContext.BaseDirectory,
                "Settings");
        }

        private static AppSettings CreateDefaultSettings()
        {
            AppSettings settings = new AppSettings
            {
                SettingsSchemaVersion = CurrentSettingsSchemaVersion,
                ZipDestinationFiles = true,
                DefaultVersioning = "0.0.1",
                AutoIncrementVersion = true,
                MinimizeToSystray = false,
                CloseToSystray = false,
                ShowRetentionWarningDialogue = true,
                Tags = new List<string>()
            };

            EnsureSettingsInitialized(settings);
            return settings;
        }
    }
}
