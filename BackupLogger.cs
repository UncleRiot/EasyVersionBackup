// Design-Rule / UI consistency:
// Keep layout, spacing, colors, sizes, and fonts aligned with ModernTheme.
// Add new shared visual values to ModernTheme instead of hardcoding local exceptions here.
// 03.05.2026 /dc

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EasyVersionBackup
{
    public sealed class BackupLogEntry
    {
        public string Severity { get; set; } = BackupLogger.LogSeverityInfo;
        public DateTime Timestamp { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    public static class BackupLogger
    {
        public const string LogLevelMinimal = "Minimal";
        public const string LogLevelNormal = "Normal";
        public const string LogLevelVerbose = "Verbose";

        public const string LogSeverityInfo = "Info";
        public const string LogSeverityWarning = "Warnung";
        public const string LogSeverityError = "Fehler";

        private static readonly object SyncRoot = new object();

        public static string LogLevel { get; private set; } = LogLevelNormal;

        public static void SetLogLevel(string? logLevel)
        {
            LogLevel = NormalizeLogLevel(logLevel);
        }

        public static string NormalizeLogLevel(string? logLevel)
        {
            if (string.Equals(logLevel, LogLevelMinimal, StringComparison.OrdinalIgnoreCase))
            {
                return LogLevelMinimal;
            }

            if (string.Equals(logLevel, LogLevelVerbose, StringComparison.OrdinalIgnoreCase))
            {
                return LogLevelVerbose;
            }

            return LogLevelNormal;
        }

        public static bool IsVerboseEnabled()
        {
            return string.Equals(LogLevel, LogLevelVerbose, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsNormalOrVerboseEnabled()
        {
            return !string.Equals(LogLevel, LogLevelMinimal, StringComparison.OrdinalIgnoreCase);
        }

        public static void WriteLine(string message)
        {
            WriteRawLine(message);
        }

        public static void WriteNormalLine(string message)
        {
            if (IsNormalOrVerboseEnabled())
            {
                WriteRawLine(message);
            }
        }

        public static void WriteVerboseLine(string message)
        {
            if (IsVerboseEnabled())
            {
                WriteRawLine(message);
            }
        }

        public static List<BackupLogEntry> ReadBackupPairEntries(string sourceDirectory, string targetDirectory, string lastBackupFileName, int maxEntries)
        {
            List<BackupLogEntry> entries = new List<BackupLogEntry>();

            try
            {
                string logDirectory = GetLogDirectory();

                if (!Directory.Exists(logDirectory))
                {
                    return entries;
                }

                string normalizedSourceDirectory = sourceDirectory ?? string.Empty;
                string normalizedTargetDirectory = targetDirectory ?? string.Empty;
                string normalizedLastBackupFileName = lastBackupFileName ?? string.Empty;

                List<string> logFilePaths = Directory
                    .GetFiles(logDirectory, "EasyVersionBackup_*.log")
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                bool isInsideMatchingBackupBlock = false;
                HashSet<string> addedLines = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (string logFilePath in logFilePaths)
                {
                    foreach (string line in File.ReadLines(logFilePath))
                    {
                        if (!TryParseLogLine(line, out BackupLogEntry entry))
                        {
                            continue;
                        }

                        bool isBackupPairLine = entry.Text.StartsWith("BACKUP PAIR |", StringComparison.OrdinalIgnoreCase);
                        bool isRetentionSettingsLine = entry.Text.StartsWith("RETENTION SETTINGS |", StringComparison.OrdinalIgnoreCase);
                        bool isBackupEndLine = entry.Text.StartsWith("MANUAL BACKUP END", StringComparison.OrdinalIgnoreCase) ||
                                               entry.Text.StartsWith("AUTOMATIC BACKUP END", StringComparison.OrdinalIgnoreCase);

                        if (isBackupPairLine || isRetentionSettingsLine)
                        {
                            isInsideMatchingBackupBlock = ContainsText(entry.Text, normalizedSourceDirectory) &&
                                                          ContainsText(entry.Text, normalizedTargetDirectory);
                        }

                        bool matchesDirectly = ContainsText(entry.Text, normalizedSourceDirectory) ||
                                               ContainsText(entry.Text, normalizedTargetDirectory) ||
                                               ContainsText(entry.Text, normalizedLastBackupFileName);

                        if (isInsideMatchingBackupBlock || matchesDirectly)
                        {
                            string uniqueKey = entry.Timestamp.ToString("O") + "|" + entry.Text;

                            if (addedLines.Add(uniqueKey))
                            {
                                entries.Add(entry);
                            }
                        }

                        if (isBackupEndLine)
                        {
                            isInsideMatchingBackupBlock = false;
                        }
                    }
                }

                return entries
                    .OrderByDescending(entry => entry.Timestamp)
                    .Take(Math.Max(1, maxEntries))
                    .OrderBy(entry => entry.Timestamp)
                    .ToList();
            }
            catch
            {
                return entries;
            }
        }

        private static bool TryParseLogLine(string line, out BackupLogEntry entry)
        {
            entry = new BackupLogEntry();

            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            string[] parts = line.Split(new[] { " | " }, 2, StringSplitOptions.None);

            if (parts.Length != 2 || !DateTime.TryParse(parts[0], out DateTime timestamp))
            {
                return false;
            }

            entry.Timestamp = timestamp;
            entry.Text = parts[1];
            entry.Severity = GetSeverity(parts[1]);

            return true;
        }

        private static string GetSeverity(string message)
        {
            string normalizedMessage = message.Trim();
            string logCategory = normalizedMessage.Split('|')[0].Trim();

            if (logCategory.Contains("ERROR", StringComparison.OrdinalIgnoreCase) ||
                logCategory.Contains("EXCEPTION", StringComparison.OrdinalIgnoreCase) ||
                logCategory.Contains("FAILED", StringComparison.OrdinalIgnoreCase) ||
                normalizedMessage.Contains(" | exception=", StringComparison.OrdinalIgnoreCase) ||
                normalizedMessage.Contains(" | error=", StringComparison.OrdinalIgnoreCase) ||
                normalizedMessage.Contains("result=FAILED", StringComparison.OrdinalIgnoreCase))
            {
                return LogSeverityError;
            }

            if (logCategory.Contains("WARNING", StringComparison.OrdinalIgnoreCase) ||
                logCategory.Contains("SKIPPED", StringComparison.OrdinalIgnoreCase) ||
                logCategory.Equals("RETENTION DELETE", StringComparison.OrdinalIgnoreCase) ||
                logCategory.EndsWith(" DELETE", StringComparison.OrdinalIgnoreCase) ||
                logCategory.EndsWith(" OVERWRITE", StringComparison.OrdinalIgnoreCase) ||
                normalizedMessage.Contains("result=Cancel", StringComparison.OrdinalIgnoreCase) ||
                normalizedMessage.Contains("result=Canceled", StringComparison.OrdinalIgnoreCase) ||
                normalizedMessage.Contains("result=Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                return LogSeverityWarning;
            }

            return LogSeverityInfo;
        }

        private static bool ContainsText(string text, string value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   text.Contains(value, StringComparison.OrdinalIgnoreCase);
        }

        private static void WriteRawLine(string message)
        {
            try
            {
                lock (SyncRoot)
                {
                    string logDirectory = GetLogDirectory();
                    Directory.CreateDirectory(logDirectory);

                    string logFilePath = Path.Combine(logDirectory, $"EasyVersionBackup_{DateTime.Now:yyyy-MM-dd}.log");
                    string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} | {message}{Environment.NewLine}";

                    File.AppendAllText(logFilePath, logLine);
                }
            }
            catch
            {
            }
        }

        private static string GetLogDirectory()
        {
            return Path.Combine(AppContext.BaseDirectory, "Logs");
        }
    }
}
