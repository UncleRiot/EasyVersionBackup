using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EasyVersionBackup
{
    public static class VersionHelper
    {
        public static string GetSuggestedVersion(AppSettings settings, BackupPathPair pair)
        {
            string pairKey = SettingsStorage.CreatePairKey(pair.SourceDirectory, pair.TargetDirectory);
            string defaultVersion = settings.DefaultVersioning?.Trim() ?? "none";

            if (string.Equals(defaultVersion, "none", StringComparison.OrdinalIgnoreCase))
            {
                if (settings.LastUsedVersionsByPair.TryGetValue(pairKey, out string? lastUsedNone) &&
                    !string.IsNullOrWhiteSpace(lastUsedNone) &&
                    settings.AutoIncrementVersion)
                {
                    return IncrementVersion(lastUsedNone);
                }

                return string.Empty;
            }

            string highestExistingVersion = GetHighestExistingVersion(pair);
            string lastUsedVersion = settings.LastUsedVersionsByPair.TryGetValue(pairKey, out string? lastUsed)
                ? lastUsed
                : string.Empty;

            string highestKnownVersion = GetHighestVersion(new[]
            {
                defaultVersion,
                highestExistingVersion,
                lastUsedVersion
            });

            if (string.IsNullOrWhiteSpace(highestKnownVersion))
            {
                return defaultVersion;
            }

            if (settings.AutoIncrementVersion)
            {
                return IncrementVersion(highestKnownVersion);
            }

            return highestKnownVersion;
        }

        public static string IncrementVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                return "0.0.1";
            }

            string[] parts = version.Split('.', StringSplitOptions.RemoveEmptyEntries);
            List<int> numbers = new List<int>();

            foreach (string part in parts)
            {
                if (!int.TryParse(part, out int value))
                {
                    return "0.0.1";
                }

                numbers.Add(value);
            }

            if (numbers.Count == 0)
            {
                return "0.0.1";
            }

            numbers[numbers.Count - 1]++;

            return string.Join(".", numbers);
        }

        public static bool IsVersionGreater(string left, string right)
        {
            int[] leftParts = ParseVersion(left);
            int[] rightParts = ParseVersion(right);

            int maxLength = Math.Max(leftParts.Length, rightParts.Length);

            for (int i = 0; i < maxLength; i++)
            {
                int leftValue = i < leftParts.Length ? leftParts[i] : 0;
                int rightValue = i < rightParts.Length ? rightParts[i] : 0;

                if (leftValue > rightValue)
                {
                    return true;
                }

                if (leftValue < rightValue)
                {
                    return false;
                }
            }

            return false;
        }

        public static string GetHighestVersion(IEnumerable<string> versions)
        {
            string highest = string.Empty;

            foreach (string version in versions)
            {
                if (string.IsNullOrWhiteSpace(version))
                {
                    continue;
                }

                if (!IsValidVersion(version))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(highest) || IsVersionGreater(version, highest))
                {
                    highest = version;
                }
            }

            return highest;
        }

        public static bool IsValidVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                return false;
            }

            return Regex.IsMatch(version, @"^\d+(\.\d+)*$");
        }

        public static string BuildVersionedName(string folderName, string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                return folderName;
            }

            return $"{folderName}_v{version}";
        }

        private static string GetHighestExistingVersion(BackupPathPair pair)
        {
            if (string.IsNullOrWhiteSpace(pair.SourceDirectory) || string.IsNullOrWhiteSpace(pair.TargetDirectory))
            {
                return string.Empty;
            }

            if (!Directory.Exists(pair.TargetDirectory))
            {
                return string.Empty;
            }

            string sourceFolderName = new DirectoryInfo(pair.SourceDirectory).Name;
            string escapedSourceFolderName = Regex.Escape(sourceFolderName);

            Regex zipRegex = new Regex($"^{escapedSourceFolderName}_v(?<version>\\d+(\\.\\d+)*)\\.zip$", RegexOptions.IgnoreCase);
            Regex directoryRegex = new Regex($"^{escapedSourceFolderName}_v(?<version>\\d+(\\.\\d+)*)$", RegexOptions.IgnoreCase);

            List<string> foundVersions = new List<string>();

            foreach (string filePath in Directory.GetFiles(pair.TargetDirectory, "*.zip"))
            {
                string fileName = Path.GetFileName(filePath);
                Match match = zipRegex.Match(fileName);

                if (match.Success)
                {
                    foundVersions.Add(match.Groups["version"].Value);
                }
            }

            foreach (string directoryPath in Directory.GetDirectories(pair.TargetDirectory))
            {
                string directoryName = Path.GetFileName(directoryPath);
                Match match = directoryRegex.Match(directoryName);

                if (match.Success)
                {
                    foundVersions.Add(match.Groups["version"].Value);
                }
            }

            return GetHighestVersion(foundVersions);
        }

        private static int[] ParseVersion(string version)
        {
            return version
                .Split('.', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => int.TryParse(part, out int number) ? number : 0)
                .ToArray();
        }
    }
}