// Design-Rule / UI consistency:
// Keep layout, spacing, colors, sizes, and fonts aligned with ModernTheme.
// Add new shared visual values to ModernTheme instead of hardcoding local exceptions here.
// 03.05.2026 /dc


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

            string trimmedDestinationPath = destinationPath.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);

            string? directoryName = Path.GetDirectoryName(trimmedDestinationPath);
            bool isDirectory = Directory.Exists(destinationPath);
            string baseName = isDirectory
                ? Path.GetFileName(trimmedDestinationPath)
                : Path.GetFileNameWithoutExtension(trimmedDestinationPath);
            string extension = isDirectory
                ? string.Empty
                : Path.GetExtension(trimmedDestinationPath);

            if (string.IsNullOrWhiteSpace(directoryName))
            {
                directoryName = string.Empty;
            }

            for (int number = 1; number <= 999; number++)
            {
                string candidatePath = Path.Combine(
                    directoryName,
                    $"{baseName}_{number:000}{extension}");

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

            string fullSourceDirectory = NormalizeDirectoryPath(sourceDirectory);
            string fullPath = NormalizeDirectoryPath(path);
            string relativePath = Path.GetRelativePath(fullSourceDirectory, fullPath)
                .Replace(Path.DirectorySeparatorChar, '\\')
                .Replace(Path.AltDirectorySeparatorChar, '\\')
                .Trim('\\');

            bool isDirectory = Directory.Exists(fullPath);

            foreach (string excludedPath in excludedPaths)
            {
                if (string.IsNullOrWhiteSpace(excludedPath))
                {
                    continue;
                }

                string normalizedExclusion = excludedPath.Trim()
                    .Replace(Path.AltDirectorySeparatorChar, '\\')
                    .Replace(Path.DirectorySeparatorChar, '\\');

                bool directoryOnly = normalizedExclusion.EndsWith("\\", StringComparison.Ordinal);
                normalizedExclusion = normalizedExclusion.TrimEnd('\\');

                if (string.IsNullOrWhiteSpace(normalizedExclusion) ||
                    normalizedExclusion.Contains('?'))
                {
                    continue;
                }

                if (directoryOnly && !isDirectory)
                {
                    continue;
                }

                if (Path.IsPathRooted(normalizedExclusion))
                {
                    if (normalizedExclusion.Contains('*'))
                    {
                        continue;
                    }

                    string fullExcludedPath = Path.GetFullPath(normalizedExclusion)
                        .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                    if (string.Equals(fullPath, fullExcludedPath, StringComparison.OrdinalIgnoreCase) ||
                        fullPath.StartsWith(
                            fullExcludedPath + Path.DirectorySeparatorChar,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    continue;
                }

                bool extensionShorthand =
                    normalizedExclusion.StartsWith(".", StringComparison.Ordinal) &&
                    !normalizedExclusion.Contains('\\') &&
                    normalizedExclusion.IndexOf('*') < 0;

                if (extensionShorthand)
                {
                    if (isDirectory)
                    {
                        continue;
                    }

                    normalizedExclusion = "*" + normalizedExclusion;
                }

                bool containsDirectorySeparator =
                    normalizedExclusion.Contains('\\');

                string regexPattern = BuildExclusionRegex(
                    normalizedExclusion,
                    containsDirectorySeparator);

                if (Regex.IsMatch(
                        relativePath,
                        regexPattern,
                        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    return true;
                }
            }

            return false;
        }

        private static string BuildExclusionRegex(
            string exclusion,
            bool anchoredToSourceRoot)
        {
            StringBuilder regexBuilder = new StringBuilder();

            if (anchoredToSourceRoot)
            {
                regexBuilder.Append("^");
            }
            else
            {
                regexBuilder.Append(@"(?:^|\\)");
            }

            for (int index = 0; index < exclusion.Length; index++)
            {
                char currentCharacter = exclusion[index];

                if (currentCharacter == '*')
                {
                    bool isDoubleWildcard =
                        index + 1 < exclusion.Length &&
                        exclusion[index + 1] == '*';

                    if (isDoubleWildcard)
                    {
                        bool followedBySeparator =
                            index + 2 < exclusion.Length &&
                            exclusion[index + 2] == '\\';

                        if (followedBySeparator)
                        {
                            regexBuilder.Append(@"(?:.*\\)?");
                            index += 2;
                        }
                        else
                        {
                            regexBuilder.Append(".*");
                            index++;
                        }

                        continue;
                    }

                    regexBuilder.Append(@"[^\\]*");
                    continue;
                }

                if (currentCharacter == '\\')
                {
                    regexBuilder.Append(@"\\");
                    continue;
                }

                regexBuilder.Append(
                    Regex.Escape(
                        currentCharacter.ToString()));
            }

            regexBuilder.Append("$");
            return regexBuilder.ToString();
        }


        public static string NormalizeDirectoryPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            string fullPath = Path.GetFullPath(path.Trim());
            string? rootPath = Path.GetPathRoot(fullPath);

            if (!string.IsNullOrWhiteSpace(rootPath) &&
                string.Equals(
                    fullPath.TrimEnd(
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar),
                    rootPath.TrimEnd(
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar),
                    StringComparison.OrdinalIgnoreCase))
            {
                return rootPath;
            }

            return fullPath.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
        }

        public static string? GetBackupPathValidationError(
            BackupPathPair pair,
            IEnumerable<BackupPathPair> allPairs)
        {
            if (string.IsNullOrWhiteSpace(pair.SourceDirectory) ||
                string.IsNullOrWhiteSpace(pair.TargetDirectory))
            {
                return "Source and target directories are required.";
            }

            string sourceDirectory;
            string targetDirectory;

            try
            {
                sourceDirectory = NormalizeDirectoryPath(pair.SourceDirectory);
                targetDirectory = NormalizeDirectoryPath(pair.TargetDirectory);
            }
            catch (Exception exception)
            {
                return $"Invalid source or target path: {exception.Message}";
            }

            if (string.IsNullOrWhiteSpace(new DirectoryInfo(sourceDirectory).Name))
            {
                return "A drive root cannot be used as the source directory.";
            }

            if (string.Equals(sourceDirectory, targetDirectory, StringComparison.OrdinalIgnoreCase))
            {
                return "Source and target directories must not be identical.";
            }

            if (IsSamePathOrDescendant(targetDirectory, sourceDirectory))
            {
                return "The target directory must not be located inside the source directory.";
            }

            foreach (BackupPathPair otherPair in allPairs)
            {
                if (ReferenceEquals(pair, otherPair) ||
                    !otherPair.IsEnabled ||
                    string.IsNullOrWhiteSpace(otherPair.SourceDirectory) ||
                    string.IsNullOrWhiteSpace(otherPair.TargetDirectory))
                {
                    continue;
                }

                string otherSourceDirectory;
                string otherTargetDirectory;

                try
                {
                    otherSourceDirectory = NormalizeDirectoryPath(otherPair.SourceDirectory);
                    otherTargetDirectory = NormalizeDirectoryPath(otherPair.TargetDirectory);
                }
                catch
                {
                    continue;
                }

                if (string.Equals(sourceDirectory, otherSourceDirectory, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(targetDirectory, otherTargetDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    return "Another enabled backup entry uses the same source and target directories.";
                }

                if (IsSamePathOrDescendant(otherTargetDirectory, sourceDirectory))
                {
                    return $"The target directory of another enabled backup entry is located inside this source directory: {otherPair.TargetDirectory}";
                }
            }

            return null;
        }

        private static bool IsSamePathOrDescendant(
            string candidatePath,
            string parentPath)
        {
            if (string.Equals(
                candidatePath,
                parentPath,
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string parentPathWithSeparator =
                parentPath.EndsWith(
                    Path.DirectorySeparatorChar.ToString(),
                    StringComparison.Ordinal) ||
                parentPath.EndsWith(
                    Path.AltDirectorySeparatorChar.ToString(),
                    StringComparison.Ordinal)
                    ? parentPath
                    : parentPath + Path.DirectorySeparatorChar;

            return candidatePath.StartsWith(
                parentPathWithSeparator,
                StringComparison.OrdinalIgnoreCase);
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

        public static int ApplyRetention(
            BackupPathPair pair,
            bool zipDestinationFiles,
            out List<string> purgedPaths)
        {
            List<string> purgePaths = GetRetentionPurgePreviewPaths(
                pair,
                zipDestinationFiles);

            return ApplyRetention(
                pair,
                zipDestinationFiles,
                purgePaths,
                out purgedPaths);
        }

        public static int ApplyRetention(
            BackupPathPair pair,
            bool zipDestinationFiles,
            IReadOnlyCollection<string> confirmedPurgePaths,
            out List<string> purgedPaths)
        {
            purgedPaths = new List<string>();

            BackupLogger.WriteLine(
                $"RETENTION SETTINGS | source={pair.SourceDirectory} | target={pair.TargetDirectory} | zip={zipDestinationFiles} | keepLast={(pair.RetentionKeepLastEnabled ? pair.RetentionKeepLastCount.ToString() : "off")} | keepDays={(pair.RetentionKeepDaysEnabled ? pair.RetentionKeepDaysCount.ToString() : "off")} | mode={FormatRetentionModeForLog(NormalizeRetentionMode(pair.RetentionMode))} | exclusions={FormatRetentionExcludedTags(pair)}");

            if (!zipDestinationFiles)
            {
                BackupLogger.WriteLine("RETENTION SKIP | reason=zip backup disabled");
                return 0;
            }

            if (confirmedPurgePaths.Count == 0)
            {
                BackupLogger.WriteLine("RETENTION SKIP | reason=no confirmed backup files to purge");
                return 0;
            }

            if (string.IsNullOrWhiteSpace(pair.SourceDirectory) ||
                string.IsNullOrWhiteSpace(pair.TargetDirectory) ||
                !Directory.Exists(pair.TargetDirectory))
            {
                BackupLogger.WriteLine("RETENTION SKIP | reason=source or target directory missing");
                return 0;
            }

            string sourceName = new DirectoryInfo(pair.SourceDirectory).Name;
            string normalizedTargetDirectory = NormalizeDirectoryPath(pair.TargetDirectory);

            foreach (string purgePath in confirmedPurgePaths)
            {
                if (string.IsNullOrWhiteSpace(purgePath))
                {
                    continue;
                }

                string normalizedPurgePath;

                try
                {
                    normalizedPurgePath = Path.GetFullPath(purgePath);
                }
                catch (Exception exception)
                {
                    BackupLogger.WriteLine(
                        $"RETENTION SKIP | path={purgePath} | reason=invalid path | error={exception.Message}");
                    continue;
                }

                string? parentDirectory = Path.GetDirectoryName(normalizedPurgePath);
                string backupNameWithoutExtension = Path.GetFileNameWithoutExtension(normalizedPurgePath);

                if (string.IsNullOrWhiteSpace(parentDirectory) ||
                    !string.Equals(
                        NormalizeDirectoryPath(parentDirectory),
                        normalizedTargetDirectory,
                        StringComparison.OrdinalIgnoreCase) ||
                    !IsRetentionBackupName(sourceName, backupNameWithoutExtension))
                {
                    BackupLogger.WriteLine(
                        $"RETENTION SKIP | path={normalizedPurgePath} | reason=path is not a recognized backup for this pair");
                    continue;
                }

                if (IsProtectedByRetentionExcludedTag(normalizedPurgePath, pair))
                {
                    BackupLogger.WriteNormalLine(
                        $"RETENTION KEEP | {Path.GetFileName(normalizedPurgePath)} | reason=excluded tag");
                    continue;
                }

                if (!File.Exists(normalizedPurgePath))
                {
                    BackupLogger.WriteVerboseLine(
                        $"RETENTION SKIP | path={normalizedPurgePath} | reason=file no longer exists");
                    continue;
                }

                try
                {
                    File.Delete(normalizedPurgePath);
                    purgedPaths.Add(normalizedPurgePath);
                    BackupLogger.WriteLine(
                        $"RETENTION DELETE | {Path.GetFileName(normalizedPurgePath)} | reason=confirmed retention candidate");
                }
                catch (Exception exception)
                {
                    BackupLogger.WriteLine(
                        $"RETENTION DELETE WARNING | path={normalizedPurgePath} | error={exception.Message}");
                }
            }

            BackupLogger.WriteLine(
                $"RETENTION SUMMARY | confirmed={confirmedPurgePaths.Count} | deleted={purgedPaths.Count}");

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
            if (string.IsNullOrWhiteSpace(backupNameWithoutSourceName) ||
                string.IsNullOrWhiteSpace(tag))
            {
                return false;
            }

            string normalizedName = backupNameWithoutSourceName.Trim();
            string normalizedTag = tag.Trim();

            if (string.Equals(
                    normalizedName,
                    normalizedTag,
                    StringComparison.OrdinalIgnoreCase) ||
                normalizedName.EndsWith(
                    "_" + normalizedTag,
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            Match appendNumberMatch = Regex.Match(
                normalizedName,
                @"^(?<name>.+)_\d{3}$",
                RegexOptions.CultureInvariant);

            if (!appendNumberMatch.Success)
            {
                return false;
            }

            string nameWithoutAppendNumber =
                appendNumberMatch.Groups["name"].Value;

            return string.Equals(
                    nameWithoutAppendNumber,
                    normalizedTag,
                    StringComparison.OrdinalIgnoreCase) ||
                nameWithoutAppendNumber.EndsWith(
                    "_" + normalizedTag,
                    StringComparison.OrdinalIgnoreCase);
        }

        private static List<FileInfo> GetRetentionZipBackupItems(
            string targetDirectory,
            string sourceName)
        {
            List<FileInfo> items = new List<FileInfo>();

            foreach (string filePath in Directory.GetFiles(
                targetDirectory,
                sourceName + "*.zip",
                SearchOption.TopDirectoryOnly))
            {
                string backupNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

                if (IsRetentionBackupName(sourceName, backupNameWithoutExtension))
                {
                    items.Add(new FileInfo(filePath));
                }
            }

            return items
                .GroupBy(file => file.FullName, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();
        }



        private static bool IsRetentionBackupName(
            string sourceName,
            string backupNameWithoutExtension)
        {
            if (string.IsNullOrWhiteSpace(sourceName) ||
                string.IsNullOrWhiteSpace(
                    backupNameWithoutExtension))
            {
                return false;
            }

            if (string.Equals(
                    backupNameWithoutExtension,
                    sourceName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string sourcePrefix = sourceName + "_";

            return backupNameWithoutExtension.StartsWith(
                    sourcePrefix,
                    StringComparison.OrdinalIgnoreCase) &&
                backupNameWithoutExtension.Length >
                    sourcePrefix.Length;
        }

        public static int ApplyRetentionDisabled(out List<string> purgedPaths)
        {
            BackupLogger.WriteLine("RETENTION SETTINGS | enabled=false");
            purgedPaths = new List<string>();
            return 0;
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
        public static List<string> GetRetentionPurgePreviewPaths(
            BackupPathPair pair,
            bool zipDestinationFiles)
        {
            return GetRetentionPurgeCandidates(
                pair,
                zipDestinationFiles,
                DateTime.UtcNow,
                0)
                .Select(file => file.FullName)
                .ToList();
        }

        public static List<string> GetRetentionPurgePreviewPathsForNextBackup(
            BackupPathPair pair,
            bool zipDestinationFiles)
        {
            return GetRetentionPurgeCandidates(
                pair,
                zipDestinationFiles,
                DateTime.UtcNow,
                1)
                .Select(file => file.FullName)
                .ToList();
        }

        private static List<FileInfo> GetRetentionPurgeCandidates(
            BackupPathPair pair,
            bool zipDestinationFiles,
            DateTime nowUtc,
            int additionalNewestBackupCount)
        {
            List<FileInfo> purgeCandidates = new List<FileInfo>();

            if (!zipDestinationFiles ||
                (!pair.RetentionKeepLastEnabled && !pair.RetentionKeepDaysEnabled) ||
                string.IsNullOrWhiteSpace(pair.SourceDirectory) ||
                string.IsNullOrWhiteSpace(pair.TargetDirectory) ||
                !Directory.Exists(pair.TargetDirectory))
            {
                return purgeCandidates;
            }

            string sourceName = new DirectoryInfo(pair.SourceDirectory).Name;

            if (string.IsNullOrWhiteSpace(sourceName))
            {
                return purgeCandidates;
            }

            List<FileInfo> backupFiles;

            try
            {
                backupFiles = GetRetentionZipBackupItems(
                    pair.TargetDirectory,
                    sourceName)
                    .OrderByDescending(
                        file => file.LastWriteTimeUtc)
                    .ToList();
            }
            catch (Exception exception)
            {
                BackupLogger.WriteLine(
                    $"RETENTION SCAN WARNING | source={pair.SourceDirectory} | target={pair.TargetDirectory} | error={exception.Message}");
                return purgeCandidates;
            }

            DateTime deleteBeforeUtc = nowUtc.AddDays(
                -Math.Max(1, pair.RetentionKeepDaysCount));

            string retentionMode = NormalizeRetentionMode(pair.RetentionMode);
            int newestRelevantBackupNumber =
                Math.Max(0, additionalNewestBackupCount);

            foreach (FileInfo file in backupFiles)
            {
                if (IsProtectedByRetentionExcludedTag(file.FullName, pair))
                {
                    continue;
                }

                newestRelevantBackupNumber++;

                bool deleteByLast = pair.RetentionKeepLastEnabled &&
                    newestRelevantBackupNumber > Math.Max(
                        1,
                        pair.RetentionKeepLastCount);

                bool deleteByDays = pair.RetentionKeepDaysEnabled &&
                    file.LastWriteTimeUtc < deleteBeforeUtc;

                bool shouldDelete;

                if (pair.RetentionKeepLastEnabled &&
                    pair.RetentionKeepDaysEnabled)
                {
                    shouldDelete = retentionMode == RetentionModeAll
                        ? deleteByLast && deleteByDays
                        : deleteByLast || deleteByDays;
                }
                else
                {
                    shouldDelete = deleteByLast || deleteByDays;
                }

                if (shouldDelete)
                {
                    purgeCandidates.Add(file);
                }
            }

            return purgeCandidates;
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