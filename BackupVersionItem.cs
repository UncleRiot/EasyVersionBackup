namespace EasyVersionBackup
{
    public class BackupVersionItem
    {
        public string SourceDirectory { get; set; } = string.Empty;
        public string TargetDirectory { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
    }
}