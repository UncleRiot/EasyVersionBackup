// Design-Rule / UI consistency:
// Keep layout, spacing, colors, sizes, and fonts aligned with ModernTheme.
// Add new shared visual values to ModernTheme instead of hardcoding local exceptions here.
// 03.05.2026 /dc


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;


namespace EasyVersionBackup
{
    public static class BackupHelper
    {
        public const string DestinationConflictAsk = "Ask";
        public const string DestinationConflictCancel = "Cancel";
        public const string DestinationConflictOverwrite = "Overwrite";
        public const string DestinationConflictAppend = "Append";

        public const string DestinationActionCreated = "created";
        public const string DestinationActionAppended = "appended";
        public const string DestinationActionOverwritten = "overwritten";

        public const string RetentionModeAny = "Any";
        public const string RetentionModeAll = "All";

        public static bool DestinationExists(string destinationPath)
        {
            return File.Exists(destinationPath) || Directory.Exists(destinationPath);
        }

        public static string GetNumberedDestinationPath(string destinationPath)
        {
            if (!DestinationExists(destinationPath))
            {
                return destinationPath;
            }

            string? directoryName = Path.GetDirectoryName(destinationPath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(destinationPath);
            string extension = Path.GetExtension(destinationPath);

            if (string.IsNullOrWhiteSpace(directoryName))
            {
                directoryName = string.Empty;
            }

            for (int number = 1; number <= 999; number++)
            {
                string candidatePath = Path.Combine(
                    directoryName,
                    $"{fileNameWithoutExtension}_{number:000}{extension}");

                if (!DestinationExists(candidatePath))
                {
                    return candidatePath;
                }
            }

            throw new IOException($"No free numbered destination name found for: {destinationPath}");
        }

        public static bool IsExcludedPath(string sourceDirectory, string path, List<string> excludedPaths)
        {
            if (excludedPaths.Count == 0)
            {
                return false;
            }

            string fullSourceDirectory = Path.GetFullPath(sourceDirectory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string relativePath = Path.GetRelativePath(fullSourceDirectory, fullPath)
                .Replace(Path.DirectorySeparatorChar, '\\')
                .Replace(Path.AltDirectorySeparatorChar, '\\')
                .Trim('\\');

            string pathName = Path.GetFileName(fullPath);
            string[] relativeParts = relativePath
                .Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string excludedPath in excludedPaths)
            {
                if (string.IsNullOrWhiteSpace(excludedPath))
                {
                    continue;
                }

                string normalizedExcludedPath = excludedPath.Trim()
                    .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

                bool endsWithDirectorySeparator = normalizedExcludedPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal);
                string normalizedExcludedPathWithoutSlash = normalizedExcludedPath.TrimEnd(Path.DirectorySeparatorChar);

                if (string.IsNullOrWhiteSpace(normalizedExcludedPathWithoutSlash))
                {
                    continue;
                }

                if (normalizedExcludedPathWithoutSlash.Contains('?'))
                {
                    continue;
                }

                if (normalizedExcludedPathWithoutSlash.Contains('*'))
                {
                    string wildcardPattern = "^" + Regex.Escape(normalizedExcludedPathWithoutSlash)
                        .Replace("\\*", ".*") + "$";

                    if (Regex.IsMatch(relativePath, wildcardPattern, RegexOptions.IgnoreCase))
                    {
                        return true;
                    }

                    if (Regex.IsMatch(pathName, wildcardPattern, RegexOptions.IgnoreCase))
                    {
                        return true;
                    }

                    foreach (string relativePart in relativeParts)
                    {
                        if (Regex.IsMatch(relativePart, wildcardPattern, RegexOptions.IgnoreCase))
                        {
                            return true;
                        }
                    }

                    continue;
                }

                if (Path.IsPathRooted(normalizedExcludedPathWithoutSlash))
                {
                    string fullExcludedPath = Path.GetFullPath(normalizedExcludedPathWithoutSlash).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                    if (string.Equals(fullPath, fullExcludedPath, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    if (fullPath.StartsWith(fullExcludedPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    continue;
                }

                if (normalizedExcludedPathWithoutSlash.Contains(Path.DirectorySeparatorChar))
                {
                    string normalizedRelativeExclusion = normalizedExcludedPathWithoutSlash.Trim(Path.DirectorySeparatorChar);

                    if (string.Equals(relativePath, normalizedRelativeExclusion, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    if (relativePath.StartsWith(normalizedRelativeExclusion + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    if (relativePath.EndsWith(Path.DirectorySeparatorChar + normalizedRelativeExclusion, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    if (relativePath.Contains(Path.DirectorySeparatorChar + normalizedRelativeExclusion + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    continue;
                }

                foreach (string relativePart in relativeParts)
                {
                    if (string.Equals(relativePart, normalizedExcludedPathWithoutSlash, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                if (!endsWithDirectorySeparator && string.Equals(pathName, normalizedExcludedPathWithoutSlash, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static List<string> GetIncludedDirectories(string sourceDirectory, List<string> excludedPaths)
        {
            List<string> directories = new List<string>();
            Stack<string> pendingDirectories = new Stack<string>();

            pendingDirectories.Push(sourceDirectory);

            while (pendingDirectories.Count > 0)
            {
                string currentDirectory = pendingDirectories.Pop();

                if (IsExcludedPath(sourceDirectory, currentDirectory, excludedPaths))
                {
                    continue;
                }

                directories.Add(currentDirectory);

                foreach (string childDirectory in Directory.GetDirectories(currentDirectory, "*", SearchOption.TopDirectoryOnly))
                {
                    if (!IsExcludedPath(sourceDirectory, childDirectory, excludedPaths))
                    {
                        pendingDirectories.Push(childDirectory);
                    }
                }
            }

            return directories;
        }

        public static string NormalizeDestinationConflictHandling(string? value)
        {
            if (string.Equals(value, DestinationConflictCancel, StringComparison.OrdinalIgnoreCase))
            {
                return DestinationConflictCancel;
            }

            if (string.Equals(value, DestinationConflictOverwrite, StringComparison.OrdinalIgnoreCase))
            {
                return DestinationConflictOverwrite;
            }

            if (string.Equals(value, DestinationConflictAppend, StringComparison.OrdinalIgnoreCase))
            {
                return DestinationConflictAppend;
            }

            return DestinationConflictAsk;
        }

        public static string NormalizeRetentionMode(string? value)
        {
            if (string.Equals(value, RetentionModeAll, StringComparison.OrdinalIgnoreCase))
            {
                return RetentionModeAll;
            }

            return RetentionModeAny;
        }

        public static int ApplyRetention(BackupPathPair pair, bool zipDestinationFiles, out List<string> purgedPaths)
        {
            purgedPaths = new List<string>();

            BackupLogger.WriteLine($"RETENTION SETTINGS | source={pair.SourceDirectory} | target={pair.TargetDirectory} | zip={zipDestinationFiles} | keepLast={(pair.RetentionKeepLastEnabled ? pair.RetentionKeepLastCount.ToString() : "off")} | keepDays={(pair.RetentionKeepDaysEnabled ? pair.RetentionKeepDaysCount.ToString() : "off")} | mode={FormatRetentionModeForLog(NormalizeRetentionMode(pair.RetentionMode))} | exclusions={FormatRetentionExcludedTags(pair)}");

            if (!zipDestinationFiles)
            {
                BackupLogger.WriteLine("RETENTION SKIP | reason=zip backup disabled");
                return 0;
            }

            if (!pair.RetentionKeepLastEnabled && !pair.RetentionKeepDaysEnabled)
            {
                BackupLogger.WriteLine("RETENTION SKIP | reason=retention disabled for backup pair");
                return 0;
            }

            if (string.IsNullOrWhiteSpace(pair.SourceDirectory) || string.IsNullOrWhiteSpace(pair.TargetDirectory))
            {
                BackupLogger.WriteLine("RETENTION SKIP | reason=source or target directory missing");
                return 0;
            }

            if (!Directory.Exists(pair.TargetDirectory))
            {
                BackupLogger.WriteLine($"RETENTION SKIP | reason=target directory not found | target={pair.TargetDirectory}");
                return 0;
            }

            string sourceName = new DirectoryInfo(pair.SourceDirectory).Name;

            if (string.IsNullOrWhiteSpace(sourceName))
            {
                BackupLogger.WriteLine("RETENTION SKIP | reason=source name empty");
                return 0;
            }

            List<FileInfo> backupFiles = GetRetentionZipBackupItems(pair.TargetDirectory, sourceName)
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .ToList();

            if (backupFiles.Count == 0)
            {
                BackupLogger.WriteLine("RETENTION SKIP | reason=no matching backup files found");
                return 0;
            }

            DateTime deleteBeforeUtc = DateTime.UtcNow.AddDays(-Math.Max(1, pair.RetentionKeepDaysCount));
            string retentionMode = NormalizeRetentionMode(pair.RetentionMode);
            int newestRelevantBackupNumber = 0;
            int keptByExcludedTag = 0;
            int keptByNewestBackups = 0;
            int keptByDays = 0;
            int keptByOtherRule = 0;

            foreach (FileInfo file in backupFiles)
            {
                string? excludedTag = GetRetentionExcludedTag(file.FullName, pair);

                if (!string.IsNullOrWhiteSpace(excludedTag))
                {
                    keptByExcludedTag++;
                    BackupLogger.WriteNormalLine($"RETENTION KEEP | {file.Name} | reason=excluded tag {excludedTag}");
                    continue;
                }

                newestRelevantBackupNumber++;

                bool deleteByLast = pair.RetentionKeepLastEnabled &&
                    newestRelevantBackupNumber > Math.Max(1, pair.RetentionKeepLastCount);

                bool deleteByDays = pair.RetentionKeepDaysEnabled &&
                    file.LastWriteTimeUtc < deleteBeforeUtc;

                bool shouldDelete;

                if (pair.RetentionKeepLastEnabled && pair.RetentionKeepDaysEnabled)
                {
                    shouldDelete = retentionMode == RetentionModeAll
                        ? deleteByLast && deleteByDays
                        : deleteByLast || deleteByDays;
                }
                else
                {
                    shouldDelete = deleteByLast || deleteByDays;
                }

                if (!shouldDelete)
                {
                    if (pair.RetentionKeepLastEnabled && !deleteByLast)
                    {
                        keptByNewestBackups++;
                    }
                    else if (pair.RetentionKeepDaysEnabled && !deleteByDays)
                    {
                        keptByDays++;
                    }
                    else
                    {
                        keptByOtherRule++;
                    }

                    BackupLogger.WriteVerboseLine($"RETENTION KEEP | {file.Name} | reason={BuildRetentionKeepReason(pair, retentionMode, file.LastWriteTimeUtc, deleteByLast, deleteByDays)}");
                    continue;
                }

                string fullName = file.FullName;
                string deleteReason = BuildRetentionDeleteReason(pair, retentionMode, file.LastWriteTimeUtc, deleteByLast, deleteByDays);

                file.Delete();
                purgedPaths.Add(fullName);

                BackupLogger.WriteLine($"RETENTION DELETE | {file.Name} | reason={deleteReason}");
            }

            BackupLogger.WriteLine($"RETENTION SUMMARY | checked={backupFiles.Count} | kept={backupFiles.Count - purgedPaths.Count} | deleted={purgedPaths.Count} | protected={keptByExcludedTag} | withinNewestBackups={keptByNewestBackups} | withinDays={keptByDays} | otherKeeps={keptByOtherRule}");

            return purgedPaths.Count;
        }

        public static bool IsProtectedByRetentionExcludedTag(string path, BackupPathPair pair)
        {
            return !string.IsNullOrWhiteSpace(GetRetentionExcludedTag(path, pair));
        }

        public static string? GetRetentionExcludedTag(string path, BackupPathPair pair)
        {
            if (pair.RetentionExcludedTags == null || pair.RetentionExcludedTags.Count == 0)
            {
                return null;
            }

            string name = Path.GetFileNameWithoutExtension(path);

            if (string.IsNullOrWhiteSpace(name))
            {
                name = Path.GetFileName(path);
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            string sourceName = string.IsNullOrWhiteSpace(pair.SourceDirectory)
                ? string.Empty
                : new DirectoryInfo(pair.SourceDirectory).Name;

            string searchableName = name;

            if (!string.IsNullOrWhiteSpace(sourceName))
            {
                if (string.Equals(searchableName, sourceName, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                string sourcePrefix = sourceName + "_";

                if (searchableName.StartsWith(sourcePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    searchableName = searchableName.Substring(sourcePrefix.Length);
                }
            }

            foreach (string tag in pair.RetentionExcludedTags)
            {
                if (string.IsNullOrWhiteSpace(tag))
                {
                    continue;
                }

                string normalizedTag = tag.Trim();

                if (IsRetentionExcludedTagInBackupName(searchableName, normalizedTag))
                {
                    return normalizedTag;
                }
            }

            return null;
        }

        private static bool IsRetentionExcludedTagInBackupName(string backupNameWithoutSourceName, string tag)
        {
            if (string.IsNullOrWhiteSpace(backupNameWithoutSourceName) || string.IsNullOrWhiteSpace(tag))
            {
                return false;
            }

            string[] nameParts = backupNameWithoutSourceName
                .Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string namePart in nameParts)
            {
                if (string.Equals(namePart.Trim(), tag.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static List<FileInfo> GetRetentionZipBackupItems(string targetDirectory, string sourceName)
        {
            List<FileInfo> items = new List<FileInfo>();

            string exactZipPath = Path.Combine(targetDirectory, sourceName + ".zip");

            if (File.Exists(exactZipPath))
            {
                items.Add(new FileInfo(exactZipPath));
            }

            foreach (string filePath in Directory.GetFiles(targetDirectory, sourceName + "_*.zip", SearchOption.TopDirectoryOnly))
            {
                string fileName = Path.GetFileName(filePath);

                if (string.Equals(fileName, sourceName + ".zip", StringComparison.OrdinalIgnoreCase) ||
                    fileName.StartsWith(sourceName + "_", StringComparison.OrdinalIgnoreCase))
                {
                    items.Add(new FileInfo(filePath));
                }
            }

            return items
                .GroupBy(file => file.FullName, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();
        }

        private static List<FileSystemInfo> GetRetentionDirectoryBackupItems(string targetDirectory, string sourceName)
        {
            return new List<FileSystemInfo>();
        }

        private static bool IsRetentionBackupName(string sourceName, string backupNameWithoutExtension)
        {
            if (string.Equals(backupNameWithoutExtension, sourceName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!backupNameWithoutExtension.StartsWith(sourceName + "_", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string suffix = backupNameWithoutExtension.Substring(sourceName.Length + 1);

            if (string.IsNullOrWhiteSpace(suffix))
            {
                return false;
            }

            return IsRetentionVersionSuffix(suffix);
        }
        public static int ApplyRetentionDisabled(out List<string> purgedPaths)
        {
            BackupLogger.WriteLine("RETENTION SETTINGS | enabled=false");
            purgedPaths = new List<string>();
            return 0;
        }
        private static bool IsRetentionVersionSuffix(string suffix)
        {
            if (Regex.IsMatch(suffix, @"^[A-Za-z]*\d+(\.\d+)*(_\d{3})?$", RegexOptions.IgnoreCase))
            {
                return true;
            }

            if (Regex.IsMatch(suffix, @"^[A-Za-z]*\d+(\.\d+)*_\d{8}_\d{4}(_\d{3})?$", RegexOptions.IgnoreCase))
            {
                return true;
            }

            if (Regex.IsMatch(suffix, @"^\d{8}(\d{4})?(_\d{3})?$", RegexOptions.IgnoreCase))
            {
                return true;
            }

            if (Regex.IsMatch(suffix, @"^\d{8}_\d{4}(_\d{3})?$", RegexOptions.IgnoreCase))
            {
                return true;
            }

            if (Regex.IsMatch(suffix, @"^\d{4}-\d{2}-\d{2}(-\d{2}-\d{2})?(_\d{3})?$", RegexOptions.IgnoreCase))
            {
                return true;
            }

            return false;
        }


        private static string FormatRetentionExcludedTags(BackupPathPair pair)
        {
            if (pair.RetentionExcludedTags == null || pair.RetentionExcludedTags.Count == 0)
            {
                return "none";
            }

            return string.Join(", ", pair.RetentionExcludedTags
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim()));
        }

        private static string FormatRetentionModeForLog(string retentionMode)
        {
            if (string.Equals(retentionMode, RetentionModeAll, StringComparison.OrdinalIgnoreCase))
            {
                return "AND";
            }

            if (string.Equals(retentionMode, RetentionModeAny, StringComparison.OrdinalIgnoreCase))
            {
                return "OR";
            }

            return retentionMode;
        }

        private static string BuildRetentionKeepReason(BackupPathPair pair, string retentionMode, DateTime lastWriteTimeUtc, bool deleteByLast, bool deleteByDays)
        {
            if (pair.RetentionKeepLastEnabled && !deleteByLast)
            {
                return $"within newest {Math.Max(1, pair.RetentionKeepLastCount)} backups";
            }

            if (pair.RetentionKeepDaysEnabled && !deleteByDays)
            {
                return $"only {GetFileAgeDays(lastWriteTimeUtc)} days old, keepDays={Math.Max(1, pair.RetentionKeepDaysCount)}";
            }

            if (pair.RetentionKeepLastEnabled && pair.RetentionKeepDaysEnabled && retentionMode == RetentionModeAll)
            {
                return $"mode=AND requires older than {Math.Max(1, pair.RetentionKeepDaysCount)} days and outside newest {Math.Max(1, pair.RetentionKeepLastCount)} backups";
            }

            return "retention rule not matched";
        }

        private static string BuildRetentionDeleteReason(BackupPathPair pair, string retentionMode, DateTime lastWriteTimeUtc, bool deleteByLast, bool deleteByDays)
        {
            List<string> reasons = new List<string>();

            if (deleteByDays)
            {
                reasons.Add($"older than {Math.Max(1, pair.RetentionKeepDaysCount)} days");
            }

            if (deleteByLast)
            {
                reasons.Add($"outside newest {Math.Max(1, pair.RetentionKeepLastCount)} backups");
            }

            string separator = retentionMode == RetentionModeAll ? " AND " : " OR ";
            return string.Join(separator, reasons);
        }

        private static int GetFileAgeDays(DateTime lastWriteTimeUtc)
        {
            return Math.Max(0, (int)Math.Floor((DateTime.UtcNow - lastWriteTimeUtc).TotalDays));
        }

        public static string FormatRetentionSummary(int purgedCount)
        {
            if (purgedCount <= 0)
            {
                return string.Empty;
            }

            return $" Retention: {purgedCount} old backup(s) purged.";
        }

        public static string FormatRetentionStatusMessage(List<string> purgedPaths)
        {
            if (purgedPaths.Count == 0)
            {
                return string.Empty;
            }

            return "Retention purged:" + Environment.NewLine + string.Join(Environment.NewLine, purgedPaths);
        }

        public static string FormatBackupCanceledBecauseDestinationExistsMessage(string destinationPath)
        {
            return $"Backup canceled. File already exists:{Environment.NewLine}{destinationPath}";
        }

        public static string FormatDestinationActionSummary(IEnumerable<string> destinationActions)
        {
            int appended = 0;
            int overwritten = 0;

            foreach (string destinationAction in destinationActions)
            {
                if (string.Equals(destinationAction, DestinationActionAppended, StringComparison.OrdinalIgnoreCase))
                {
                    appended++;
                }

                if (string.Equals(destinationAction, DestinationActionOverwritten, StringComparison.OrdinalIgnoreCase))
                {
                    overwritten++;
                }
            }

            if (appended == 0 && overwritten == 0)
            {
                return string.Empty;
            }

            List<string> parts = new List<string>();

            if (appended > 0)
            {
                parts.Add($"{appended} appended");
            }

            if (overwritten > 0)
            {
                parts.Add($"{overwritten} overwritten");
            }

            return " Destination: " + string.Join(", ", parts) + ".";
        }
    }
}