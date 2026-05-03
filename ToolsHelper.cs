using System.IO;

namespace EasyVersionBackup
{
    public static class ToolsHelper
    {
        public const string SettingsFileName = "EasyVersionBackup.settings.json";

        public static string BuildSettingsFilePath(string directoryPath)
        {
            return Path.Combine(directoryPath, SettingsFileName);
        }
    }
}