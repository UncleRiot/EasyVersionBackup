using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace EasyVersionBackup
{
    public sealed class SourceCleanupResult
    {
        public int DeletedCount { get; set; }
        public List<string> DeletedPaths { get; set; } = new List<string>();
        public List<string> FailedPaths { get; set; } = new List<string>();
    }

    public static class SourceCleanupService
    {
        public const string ModeKeepLast = "KeepLast";
        public const string ModeKeepAfterDate = "KeepAfterDate";

        public static string NormalizeMode(string? mode)
        {
            return string.Equals(
                mode,
                ModeKeepAfterDate,
                StringComparison.OrdinalIgnoreCase)
                ? ModeKeepAfterDate
                : ModeKeepLast;
        }

        public static List<string> GetNormalizedPatterns(BackupPathPair pair)
        {
            IEnumerable<string> configuredValues =
                pair.SourceCleanupFileExtensions ??
                new List<string>();

            List<string> values = configuredValues
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .ToList();

            if (!string.IsNullOrWhiteSpace(pair.SourceCleanupRelativeDirectory) &&
                values.All(IsValidExtension))
            {
                string relativeDirectory =
                    pair.SourceCleanupRelativeDirectory
                        .Trim()
                        .TrimStart(
                            Path.DirectorySeparatorChar,
                            Path.AltDirectorySeparatorChar)
                        .TrimEnd(
                            Path.DirectorySeparatorChar,
                            Path.AltDirectorySeparatorChar);

                values = values
                    .Select(extension =>
                        Path.Combine(
                            relativeDirectory,
                            "*" + extension))
                    .ToList();
            }

            return NormalizePatterns(values);
        }

        public static List<string> NormalizePatterns(
            IEnumerable<string>? patterns)
        {
            List<string> result = new List<string>();

            if (patterns == null)
            {
                return result;
            }

            foreach (string patternValue in patterns)
            {
                if (!TryNormalizePattern(
                        patternValue,
                        out string normalizedPattern,
                        out _))
                {
                    continue;
                }

                if (!result.Contains(
                        normalizedPattern,
                        StringComparer.OrdinalIgnoreCase))
                {
                    result.Add(normalizedPattern);
                }
            }

            return result;
        }

        public static bool TryGetAffectedPath(
            string sourceDirectory,
            string pattern,
            out string affectedPath,
            out string errorMessage)
        {
            affectedPath = string.Empty;

            if (!TryResolvePattern(
                    sourceDirectory,
                    pattern,
                    false,
                    out _,
                    out string searchPattern,
                    out string normalizedPattern,
                    out errorMessage))
            {
                return false;
            }

            if (IsRootExtensionPattern(normalizedPattern))
            {
                affectedPath = Path.Combine(
                    Path.GetFullPath(sourceDirectory),
                    searchPattern);
                return true;
            }

            string relativeDirectory =
                Path.GetDirectoryName(normalizedPattern) ??
                string.Empty;

            affectedPath = Path.Combine(
                Path.GetFullPath(sourceDirectory),
                relativeDirectory,
                searchPattern);

            return true;
        }

        public static bool TryValidateSettings(
            BackupPathPair pair,
            out string cleanupDirectory,
            out string errorMessage)
        {
            cleanupDirectory = string.Empty;
            errorMessage = string.Empty;

            if (!pair.SourceCleanupEnabled)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(pair.SourceDirectory))
            {
                errorMessage = "The source directory is empty.";
                return false;
            }

            string sourceDirectory;

            try
            {
                sourceDirectory =
                    Path.GetFullPath(pair.SourceDirectory);
            }
            catch (Exception exception)
            {
                errorMessage = exception.Message;
                return false;
            }

            if (!Directory.Exists(sourceDirectory))
            {
                errorMessage =
                    "The source directory does not exist.";
                return false;
            }

            List<string> patterns =
                GetNormalizedPatterns(pair);

            if (patterns.Count == 0)
            {
                errorMessage =
                    "Add at least one cleanup rule.";
                return false;
            }

            foreach (string pattern in patterns)
            {
                if (!TryResolvePattern(
                        sourceDirectory,
                        pattern,
                        true,
                        out string patternDirectory,
                        out _,
                        out _,
                        out errorMessage))
                {
                    return false;
                }

                cleanupDirectory = patternDirectory;
            }

            if (NormalizeMode(pair.SourceCleanupMode) ==
                ModeKeepLast)
            {
                if (pair.SourceCleanupKeepLastCount < 1)
                {
                    errorMessage =
                        "The number of files to keep must be greater than 0.";
                    return false;
                }
            }
            else if (!TryParseKeepAfterDate(
                         pair.SourceCleanupKeepAfterDate,
                         out _))
            {
                errorMessage =
                    "Select a valid keep-from date.";
                return false;
            }

            return true;
        }

        public static SourceCleanupResult Apply(
            BackupPathPair pair)
        {
            SourceCleanupResult result =
                new SourceCleanupResult();

            if (!pair.SourceCleanupEnabled)
            {
                return result;
            }

            if (!TryValidateSettings(
                    pair,
                    out _,
                    out string errorMessage))
            {
                throw new InvalidOperationException(
                    errorMessage);
            }

            Dictionary<string, FileInfo> candidatesByPath =
                new Dictionary<string, FileInfo>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (string pattern in
                     GetNormalizedPatterns(pair))
            {
                if (!TryResolvePattern(
                        pair.SourceDirectory,
                        pattern,
                        true,
                        out string patternDirectory,
                        out string searchPattern,
                        out _,
                        out errorMessage))
                {
                    throw new InvalidOperationException(
                        errorMessage);
                }

                foreach (string filePath in
                         Directory.EnumerateFiles(
                             patternDirectory,
                             searchPattern,
                             SearchOption.TopDirectoryOnly))
                {
                    string fullPath =
                        Path.GetFullPath(filePath);

                    candidatesByPath[fullPath] =
                        new FileInfo(fullPath);
                }
            }

            List<FileInfo> candidates =
                candidatesByPath.Values
                    .OrderByDescending(fileInfo =>
                        fileInfo.LastWriteTimeUtc)
                    .ThenBy(
                        fileInfo => fileInfo.FullName,
                        StringComparer.OrdinalIgnoreCase)
                    .ToList();

            IEnumerable<FileInfo> filesToDelete;

            if (NormalizeMode(pair.SourceCleanupMode) ==
                ModeKeepAfterDate)
            {
                TryParseKeepAfterDate(
                    pair.SourceCleanupKeepAfterDate,
                    out DateTime keepAfterDate);

                DateTime keepAfterDateUtc =
                    DateTime.SpecifyKind(
                            keepAfterDate.Date,
                            DateTimeKind.Local)
                        .ToUniversalTime();

                filesToDelete = candidates.Where(
                    fileInfo =>
                        fileInfo.LastWriteTimeUtc <
                        keepAfterDateUtc);
            }
            else
            {
                filesToDelete = candidates.Skip(
                    pair.SourceCleanupKeepLastCount);
            }

            foreach (FileInfo fileInfo in filesToDelete)
            {
                try
                {
                    fileInfo.Delete();
                    result.DeletedCount++;
                    result.DeletedPaths.Add(
                        fileInfo.FullName);

                    BackupLogger.WriteLine(
                        $"SOURCE CLEANUP DELETE | source=\"{pair.SourceDirectory}\" | path=\"{fileInfo.FullName}\"");
                }
                catch (Exception exception)
                {
                    result.FailedPaths.Add(
                        fileInfo.FullName);

                    BackupLogger.WriteLine(
                        $"SOURCE CLEANUP ERROR | source=\"{pair.SourceDirectory}\" | path=\"{fileInfo.FullName}\" | error=\"{exception.Message}\"");
                }
            }

            return result;
        }

        public static List<string> NormalizeExtensions(
            IEnumerable<string>? extensions)
        {
            return NormalizePatterns(extensions);
        }

        public static bool IsValidExtension(
            string extension)
        {
            if (string.IsNullOrWhiteSpace(extension) ||
                extension.Length < 2 ||
                extension[0] != '.')
            {
                return false;
            }

            if (extension.IndexOfAny(
                    Path.GetInvalidFileNameChars()) >= 0 ||
                extension.Contains('*') ||
                extension.Contains('?') ||
                extension.Contains('/') ||
                extension.Contains('\\'))
            {
                return false;
            }

            return extension.LastIndexOf('.') == 0;
        }

        public static bool TryParseKeepAfterDate(
            string? value,
            out DateTime date)
        {
            return DateTime.TryParseExact(
                value,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date);
        }

        private static bool TryNormalizePattern(
            string? patternValue,
            out string normalizedPattern,
            out string errorMessage)
        {
            normalizedPattern = string.Empty;
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(patternValue))
            {
                errorMessage =
                    "The cleanup rule is empty.";
                return false;
            }

            string pattern =
                patternValue.Trim()
                    .Replace(
                        Path.AltDirectorySeparatorChar,
                        Path.DirectorySeparatorChar)
                    .TrimStart(
                        Path.DirectorySeparatorChar);

            if (IsValidExtension(pattern))
            {
                normalizedPattern = pattern;
                return true;
            }

            if (pattern.StartsWith(
                    "*.",
                    StringComparison.Ordinal) &&
                IsValidExtension(pattern.Substring(1)))
            {
                normalizedPattern =
                    pattern.Substring(1);
                return true;
            }

            if (Path.IsPathRooted(pattern) ||
                pattern.Contains(':'))
            {
                errorMessage =
                    "Cleanup rules must be relative to the backup source.";
                return false;
            }

            string filePattern =
                Path.GetFileName(pattern);

            if (IsValidExtension(filePattern))
            {
                filePattern = "*" + filePattern;
            }

            if (!filePattern.StartsWith(
                    "*.",
                    StringComparison.Ordinal) ||
                !IsValidExtension(
                    filePattern.Substring(1)))
            {
                errorMessage =
                    "Use .sav for the source folder or Saved\\SaveGames\\.sav for a subfolder.";
                return false;
            }

            string relativeDirectory =
                Path.GetDirectoryName(pattern) ??
                string.Empty;

            string[] pathParts =
                relativeDirectory.Split(
                    new[]
                    {
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar
                    },
                    StringSplitOptions.RemoveEmptyEntries);

            if (pathParts.Length == 0 ||
                pathParts.Any(pathPart =>
                    pathPart == "." ||
                    pathPart == ".." ||
                    pathPart.IndexOfAny(
                        Path.GetInvalidFileNameChars()) >= 0))
            {
                errorMessage =
                    "The cleanup subfolder is invalid.";
                return false;
            }

            normalizedPattern =
                Path.Combine(
                    relativeDirectory,
                    filePattern);

            return true;
        }

        private static bool TryResolvePattern(
            string sourceDirectory,
            string pattern,
            bool requireDirectoryExists,
            out string patternDirectory,
            out string searchPattern,
            out string normalizedPattern,
            out string errorMessage)
        {
            patternDirectory = string.Empty;
            searchPattern = string.Empty;

            if (!TryNormalizePattern(
                    pattern,
                    out normalizedPattern,
                    out errorMessage))
            {
                return false;
            }

            string normalizedSourceDirectory;

            try
            {
                normalizedSourceDirectory =
                    Path.TrimEndingDirectorySeparator(
                        Path.GetFullPath(
                            sourceDirectory));

                if (IsRootExtensionPattern(
                        normalizedPattern))
                {
                    patternDirectory =
                        normalizedSourceDirectory;
                    searchPattern =
                        "*" + normalizedPattern;
                }
                else
                {
                    string relativeDirectory =
                        Path.GetDirectoryName(
                            normalizedPattern) ??
                        string.Empty;

                    patternDirectory =
                        Path.TrimEndingDirectorySeparator(
                            Path.GetFullPath(
                                Path.Combine(
                                    normalizedSourceDirectory,
                                    relativeDirectory)));

                    searchPattern =
                        Path.GetFileName(
                            normalizedPattern);
                }
            }
            catch (Exception exception)
            {
                errorMessage = exception.Message;
                return false;
            }

            if (!IsSameOrChildPath(
                    normalizedSourceDirectory,
                    patternDirectory))
            {
                errorMessage =
                    "The cleanup rule points outside the backup source.";
                return false;
            }

            if (requireDirectoryExists &&
                !Directory.Exists(patternDirectory))
            {
                errorMessage =
                    $"The cleanup folder does not exist: {patternDirectory}";
                return false;
            }

            if (Directory.Exists(patternDirectory) &&
                ContainsReparsePoint(
                    normalizedSourceDirectory,
                    patternDirectory))
            {
                errorMessage =
                    "The cleanup path must not pass through a symbolic link or junction.";
                return false;
            }

            return true;
        }

        private static bool IsRootExtensionPattern(
            string normalizedPattern)
        {
            return IsValidExtension(
                normalizedPattern);
        }

        private static bool IsSameOrChildPath(
            string parentPath,
            string childPath)
        {
            string normalizedParent =
                Path.TrimEndingDirectorySeparator(
                    Path.GetFullPath(parentPath));

            string normalizedChild =
                Path.TrimEndingDirectorySeparator(
                    Path.GetFullPath(childPath));

            if (string.Equals(
                    normalizedParent,
                    normalizedChild,
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string parentPrefix =
                normalizedParent +
                Path.DirectorySeparatorChar;

            return normalizedChild.StartsWith(
                parentPrefix,
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool ContainsReparsePoint(
            string sourceDirectory,
            string cleanupDirectory)
        {
            string relativePath =
                Path.GetRelativePath(
                    sourceDirectory,
                    cleanupDirectory);

            string currentPath =
                Path.GetFullPath(sourceDirectory);

            if (IsReparsePoint(currentPath))
            {
                return true;
            }

            foreach (string pathPart in relativePath.Split(
                         new[]
                         {
                             Path.DirectorySeparatorChar,
                             Path.AltDirectorySeparatorChar
                         },
                         StringSplitOptions.RemoveEmptyEntries))
            {
                currentPath = Path.Combine(
                    currentPath,
                    pathPart);

                if (IsReparsePoint(currentPath))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsReparsePoint(
            string path)
        {
            return (File.GetAttributes(path) &
                    FileAttributes.ReparsePoint) != 0;
        }
    }
}
