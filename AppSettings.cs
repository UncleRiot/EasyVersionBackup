using System.Collections.Generic;

namespace EasyVersionBackup
{
    public class AppSettings
    {
        public bool ZipDestinationFiles { get; set; } = true;
        public string DefaultVersioning { get; set; } = "0.0.1";
        public bool AutoIncrementVersion { get; set; } = true;
        public bool MinimizeToSystray { get; set; } = false;
        public bool IgnoreCopyErrors { get; set; } = false;
        public bool AutoBackupEnabled { get; set; } = false;
        public int AutoBackupIntervalMinutes { get; set; } = 15;
        public int AutoBackupIntervalSeconds { get; set; } = 900;

        public List<BackupPathPair> BackupPathPairs { get; set; } = new List<BackupPathPair>();
        public Dictionary<string, string> LastUsedVersionsByPair { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, BackupPathStatus> BackupStatusesByPair { get; set; } = new Dictionary<string, BackupPathStatus>();
        public int MainWindowWidth { get; set; } = 800;
        public int MainWindowHeight { get; set; } = 395;
        public int MainWindowLeft { get; set; } = -1;
        public int MainWindowTop { get; set; } = -1;
    }

    public class BackupPathStatus
    {
        public string LastBackupDateTime { get; set; } = string.Empty;
        public string LastBackupStatus { get; set; } = string.Empty;
        public string LastBackupErrorMessage { get; set; } = string.Empty;
    }
    public class BackupPathPair
    {
        public bool IsEnabled { get; set; } = true;
        public string SourceDirectory { get; set; } = string.Empty;
        public string TargetDirectory { get; set; } = string.Empty;
    }
}