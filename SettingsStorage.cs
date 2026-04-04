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

                if (settings.BackupPathPairs == null)
                {
                    settings.BackupPathPairs = new System.Collections.Generic.List<BackupPathPair>();
                }

                if (settings.LastUsedVersionsByPair == null)
                {
                    settings.LastUsedVersionsByPair = new System.Collections.Generic.Dictionary<string, string>();
                }

                return settings;
            }
            catch
            {
                return CreateDefaultSettings();
            }
        }

        public static void Save(AppSettings settings)
        {
            if (settings.BackupPathPairs == null)
            {
                settings.BackupPathPairs = new System.Collections.Generic.List<BackupPathPair>();
            }

            if (settings.LastUsedVersionsByPair == null)
            {
                settings.LastUsedVersionsByPair = new System.Collections.Generic.Dictionary<string, string>();
            }

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