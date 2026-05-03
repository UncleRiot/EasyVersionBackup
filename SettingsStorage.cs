using System;
using System.IO;
using System.Text.Json;

namespace EasyVersionBackup
{
    public static class SettingsStorage
    {
        private static readonly string SettingsFilePath =
            Path.Combine(AppContext.BaseDirectory, "EasyVersionBackup.settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                {
                    return CreateDefaultSettings();
                }

                string json = File.ReadAllText(SettingsFilePath);
                AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(json);

                if (settings == null)
                {
                    return CreateDefaultSettings();
                }

                EnsureSettingsInitialized(settings);
                return settings;
            }
            catch
            {
                return CreateDefaultSettings();
            }
        }
        private static void EnsureSettingsInitialized(AppSettings settings)
        {
            if (settings.BackupPathPairs == null)
            {
                settings.BackupPathPairs = new System.Collections.Generic.List<BackupPathPair>();
            }

            foreach (BackupPathPair pair in settings.BackupPathPairs)
            {
                if (pair.ExcludedPaths == null)
                {
                    pair.ExcludedPaths = new System.Collections.Generic.List<string>();
                }
            }

            if (settings.LastUsedVersionsByPair == null)
            {
                settings.LastUsedVersionsByPair = new System.Collections.Generic.Dictionary<string, string>();
            }

            if (settings.BackupStatusesByPair == null)
            {
                settings.BackupStatusesByPair = new System.Collections.Generic.Dictionary<string, BackupPathStatus>();
            }
        }
        public static bool TryImportFromFile(string filePath, out AppSettings importedSettings, out string errorMessage)
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

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(filePath, json);
        }
        public static void Save(AppSettings settings)
        {
            EnsureSettingsInitialized(settings);

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(SettingsFilePath, json);
        }

        public static string CreatePairKey(string sourceDirectory, string targetDirectory)
        {
            return $"{sourceDirectory}|{targetDirectory}";
        }

        private static AppSettings CreateDefaultSettings()
        {
            return new AppSettings
            {
                ZipDestinationFiles = true,
                DefaultVersioning = "0.0.1",
                AutoIncrementVersion = true,
                MinimizeToSystray = false
            };
        }
    }
}