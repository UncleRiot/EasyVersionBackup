// Design-Rule / UI consistency:
// Keep layout, spacing, colors, sizes, and fonts aligned with ModernTheme.
// Add new shared visual values to ModernTheme instead of hardcoding local exceptions here.
// 03.05.2026 /dc

using System;
using System.Collections.Generic;
using System.Globalization;
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
        public const string LogSeverityCleanup = "Source Cleanup";
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

        public static string GetCurrentLogFilePath()
        {
            return Path.Combine(GetLogDirectory(), $"EasyVersionBackup_{DateTime.Now:yyyy-MM-dd}.log");
        }

        public static List<BackupLogEntry> ReadBackupPairEntries(
            string sourceDirectory,
            string targetDirectory,
            string lastBackupFileName,
            int maxEntries)
        {
            List<BackupLogEntry> entries =
                new List<BackupLogEntry>();

            try
            {
                string logDirectory = GetLogDirectory();

                if (!Directory.Exists(logDirectory))
                {
                    return entries;
                }

                string normalizedSourceDirectory =
                    NormalizeForSearch(sourceDirectory ?? string.Empty);

                string normalizedTargetDirectory =
                    NormalizeForSearch(targetDirectory ?? string.Empty);

                string normalizedLastBackupFileName =
                    (lastBackupFileName ?? string.Empty).Trim();

                List<string> logFilePaths = Directory
                    .GetFiles(
                        logDirectory,
                        "EasyVersionBackup_*.log",
                        SearchOption.TopDirectoryOnly)
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                bool isInsideMatchingBackupBlock = false;
                HashSet<string> addedLines =
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (string logFilePath in logFilePaths)
                {
                    foreach (string line in File.ReadLines(logFilePath))
                    {
                        if (!TryParseLogLine(
                            line,
                            out BackupLogEntry entry))
                        {
                            continue;
                        }

                        bool isBackupStartLine =
                            entry.Text.StartsWith(
                                "MANUAL BACKUP START |",
                                StringComparison.OrdinalIgnoreCase) ||
                            entry.Text.StartsWith(
                                "AUTOMATIC BACKUP START |",
                                StringComparison.OrdinalIgnoreCase);

                        bool isBackupEndLine =
                            entry.Text.StartsWith(
                                "MANUAL BACKUP END |",
                                StringComparison.OrdinalIgnoreCase) ||
                            entry.Text.StartsWith(
                                "AUTOMATIC BACKUP END |",
                                StringComparison.OrdinalIgnoreCase);

                        if (isBackupStartLine)
                        {
                            isInsideMatchingBackupBlock =
                                ContainsPairField(
                                    entry.Text,
                                    "source",
                                    normalizedSourceDirectory) &&
                                ContainsPairField(
                                    entry.Text,
                                    "target",
                                    normalizedTargetDirectory);
                        }

                        bool matchesLegacyLine =
                            ContainsPairField(
                                entry.Text,
                                "source",
                                normalizedSourceDirectory) &&
                            ContainsPairField(
                                entry.Text,
                                "target",
                                normalizedTargetDirectory);

                        bool matchesBackupFileName =
                            !string.IsNullOrWhiteSpace(
                                normalizedLastBackupFileName) &&
                            entry.Text.Contains(
                                "backup=\"" +
                                normalizedLastBackupFileName +
                                "\"",
                                StringComparison.OrdinalIgnoreCase);

                        if (isInsideMatchingBackupBlock ||
                            matchesLegacyLine ||
                            matchesBackupFileName)
                        {
                            string uniqueKey =
                                entry.Timestamp.ToString("O") +
                                "|" +
                                entry.Text;

                            if (addedLines.Add(uniqueKey))
                            {
                                entries.Add(entry);
                            }
                        }

                        if (isBackupEndLine &&
                            isInsideMatchingBackupBlock)
                        {
                            isInsideMatchingBackupBlock = false;
                        }
                    }
                }

                return entries
                    .OrderByDescending(entry => entry.Timestamp)
                    .Take(Math.Max(1, maxEntries))
                    .ToList();
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Log entries could not be read: " +
                    exception.Message);

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

            if (parts.Length != 2 ||
                !DateTime.TryParseExact(
                    parts[0],
                    "yyyy-MM-dd HH:mm:ss.fff",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime timestamp))
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

            if (logCategory.StartsWith("SOURCE CLEANUP ", StringComparison.OrdinalIgnoreCase))
            {
                return LogSeverityCleanup;
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

        private static bool ContainsPairField(
            string text,
            string fieldName,
            string normalizedValue)
        {
            if (string.IsNullOrWhiteSpace(text) ||
                string.IsNullOrWhiteSpace(fieldName) ||
                string.IsNullOrWhiteSpace(normalizedValue))
            {
                return false;
            }

            string quotedPrefix = fieldName + "=\"";
            int quotedStart = text.IndexOf(
                quotedPrefix,
                StringComparison.OrdinalIgnoreCase);

            if (quotedStart >= 0)
            {
                quotedStart += quotedPrefix.Length;
                int quotedEnd = text.IndexOf(
                    '"',
                    quotedStart);

                if (quotedEnd >= quotedStart)
                {
                    string fieldValue = text.Substring(
                        quotedStart,
                        quotedEnd - quotedStart);

                    return string.Equals(
                        NormalizeForSearch(fieldValue),
                        normalizedValue,
                        StringComparison.OrdinalIgnoreCase);
                }
            }

            string unquotedPrefix = fieldName + "=";
            int unquotedStart = text.IndexOf(
                unquotedPrefix,
                StringComparison.OrdinalIgnoreCase);

            if (unquotedStart < 0)
            {
                return false;
            }

            unquotedStart += unquotedPrefix.Length;
            int unquotedEnd = text.IndexOf(
                " | ",
                unquotedStart,
                StringComparison.Ordinal);

            string unquotedValue = unquotedEnd >= 0
                ? text.Substring(
                    unquotedStart,
                    unquotedEnd - unquotedStart)
                : text.Substring(unquotedStart);

            return string.Equals(
                NormalizeForSearch(unquotedValue),
                normalizedValue,
                StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeForSearch(string value)
        {
            return value
                .Trim()
                .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
                .TrimEnd(Path.DirectorySeparatorChar);
        }

        private static void WriteRawLine(string message)
        {
            try
            {
                lock (SyncRoot)
                {
                    string logDirectory = GetLogDirectory();
                    Directory.CreateDirectory(logDirectory);

                    string logFilePath = GetCurrentLogFilePath();
                    string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} | {message}{Environment.NewLine}";

                    File.AppendAllText(logFilePath, logLine);
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Log entry could not be written: " +
                    exception.Message);
            }
        }

        public static void CleanupOldLogFiles(int retentionDays)
        {
            try
            {
                string logDirectory = GetLogDirectory();

                if (!Directory.Exists(logDirectory))
                {
                    return;
                }

                DateTime deleteBeforeUtc = DateTime.UtcNow.AddDays(
                    -Math.Max(1, retentionDays));

                foreach (string logFilePath in Directory.GetFiles(
                    logDirectory,
                    "EasyVersionBackup_*.log",
                    SearchOption.TopDirectoryOnly))
                {
                    if (File.GetLastWriteTimeUtc(logFilePath) <
                        deleteBeforeUtc)
                    {
                        File.Delete(logFilePath);
                    }
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Old log files could not be cleaned up: " +
                    exception.Message);
            }
        }

        private static string GetLogDirectory()
        {
            return Path.Combine(AppContext.BaseDirectory, "Logs");
        }
    }
}
