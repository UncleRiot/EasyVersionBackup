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
            string defaultVersioning = !string.IsNullOrWhiteSpace(pair.Versioning)
                ? pair.Versioning.Trim()
                : settings.DefaultVersioning?.Trim() ?? "none";

            if (string.Equals(defaultVersioning, "none", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            string defaultVersion = VersionPatternHelper.CreateVersionFromPattern(defaultVersioning);

            if (VersionPatternHelper.IsDatePattern(defaultVersioning))
            {
                return defaultVersion;
            }

            string pairKey = SettingsStorage.CreatePairKey(pair.SourceDirectory, pair.TargetDirectory);
            string lastUsedVersion = settings.LastUsedVersionsByPair.TryGetValue(pairKey, out string? lastUsed)
                ? lastUsed
                : string.Empty;

            string compatibleLastUsedVersion = VersionPatternHelper.GetHighestCompatibleVersion(defaultVersion, new[]
            {
        lastUsedVersion
    });

            string highestExistingVersion = GetHighestExistingVersion(pair, defaultVersion);

            string highestKnownVersion = VersionPatternHelper.GetHighestCompatibleVersion(defaultVersion, new[]
            {
        defaultVersion,
        compatibleLastUsedVersion,
        highestExistingVersion
    });

            if (string.IsNullOrWhiteSpace(highestKnownVersion))
            {
                return defaultVersion;
            }

            if (settings.AutoIncrementVersion)
            {
                return VersionPatternHelper.IncrementVersion(highestKnownVersion);
            }

            return highestKnownVersion;
        }

        public static string IncrementVersion(string version)
        {
            return VersionPatternHelper.IncrementVersion(version);
        }

        public static bool IsValidVersion(string version)
        {
            return VersionPatternHelper.IsValidVersionValue(version);
        }

        public static string BuildVersionedName(string folderName, string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                return folderName;
            }

            return $"{folderName}_{version}";
        }

        private static string GetHighestExistingVersion(BackupPathPair pair, string defaultVersion)
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

            Regex zipRegex = new Regex($"^{escapedSourceFolderName}_(?<version>.+)\\.zip$", RegexOptions.IgnoreCase);
            Regex directoryRegex = new Regex($"^{escapedSourceFolderName}_(?<version>.+)$", RegexOptions.IgnoreCase);

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

            return VersionPatternHelper.GetHighestCompatibleVersion(defaultVersion, foundVersions);
        }
    }
}