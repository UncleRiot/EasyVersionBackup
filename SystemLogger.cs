using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace EasyVersionBackup
{
    public static class SystemLogger
    {
        private static readonly object SyncRoot =
            new object();

        private static readonly string LogDirectoryPath =
            Path.Combine(
                AppContext.BaseDirectory,
                "Logs");

        private static readonly string LogFilePath =
            Path.Combine(
                LogDirectoryPath,
                "EasyVersionBackup_system.log");

        public static void WriteSettingsChanges(
            AppSettings previousSettings,
            AppSettings currentSettings)
        {
            WriteValueChange("Global.ZipDestinationFiles", previousSettings.ZipDestinationFiles, currentSettings.ZipDestinationFiles);
            WriteValueChange("Global.DefaultVersioning", previousSettings.DefaultVersioning, currentSettings.DefaultVersioning);
            WriteValueChange("Global.AutoIncrementVersion", previousSettings.AutoIncrementVersion, currentSettings.AutoIncrementVersion);
            WriteValueChange("Global.MinimizeToSystray", previousSettings.MinimizeToSystray, currentSettings.MinimizeToSystray);
            WriteValueChange("Global.CloseToSystray", previousSettings.CloseToSystray, currentSettings.CloseToSystray);
            WriteValueChange("Global.AutoUpdateCheck", previousSettings.AutoUpdateCheck, currentSettings.AutoUpdateCheck);
            WriteValueChange("Global.StartWithWindows", previousSettings.StartWithWindows, currentSettings.StartWithWindows);
            WriteValueChange("Global.IgnoreCopyErrors", previousSettings.IgnoreCopyErrors, currentSettings.IgnoreCopyErrors);
            WriteValueChange("Global.AutoPurgeEnabled", previousSettings.AutoPurgeEnabled, currentSettings.AutoPurgeEnabled);
            WriteValueChange("Global.SourceCleanupEnabled", previousSettings.SourceCleanupEnabled, currentSettings.SourceCleanupEnabled);
            WriteValueChange("Global.ShowRetentionWarningDialogue", previousSettings.ShowRetentionWarningDialogue, currentSettings.ShowRetentionWarningDialogue);
            WriteValueChange("Global.BackupDestinationConflictHandling", previousSettings.BackupDestinationConflictHandling, currentSettings.BackupDestinationConflictHandling);
            WriteValueChange("Global.AutoBackupEnabled", previousSettings.AutoBackupEnabled, currentSettings.AutoBackupEnabled);
            WriteValueChange("Global.AutoBackupIntervalSeconds", previousSettings.AutoBackupIntervalSeconds, currentSettings.AutoBackupIntervalSeconds);
            WriteValueChange("Global.LogLevel", previousSettings.LogLevel, currentSettings.LogLevel);
            WriteListChange("Global.Tags", previousSettings.Tags, currentSettings.Tags);

            WriteBackupPairChanges(
                previousSettings.BackupPathPairs,
                currentSettings.BackupPathPairs);
        }

        public static void WriteExperimentalConfirmation(
            string featureName,
            string enteredText)
        {
            WriteLine(
                $"EXPERIMENTAL FEATURE CONFIRMATION | feature=\"{Escape(featureName)}\" | enteredText=\"{Escape(enteredText)}\"");
        }

        public static void WriteInitialDisclaimerConfirmation(
            string enteredText)
        {
            WriteLine(
                $"INITIAL DISCLAIMER ACCEPTED | enteredText=\"{Escape(enteredText)}\"");
        }

        public static void WriteRecurringDataLossWarning(
            bool retentionEnabled,
            bool sourceCleanupEnabled,
            bool accepted,
            string enteredText)
        {
            WriteLine(
                $"RECURRING DATA-LOSS WARNING | retentionEnabled={retentionEnabled} | sourceCleanupEnabled={sourceCleanupEnabled} | accepted={accepted} | enteredText=\"{Escape(enteredText)}\"");
        }

        public static void WriteDebugModeAccess(
            bool accepted)
        {
            WriteLine(
                $"DEBUG MODE ACCESS | accepted={accepted}");
        }

        public static void WriteDebugModeChanges(
            DebugModeSettings previousSettings,
            DebugModeSettings currentSettings)
        {
            WriteLine(
                "DEBUG MODE SETTINGS SAVED");

            WriteValueChange(
                "DebugMode.Enabled",
                previousSettings.Enabled,
                currentSettings.Enabled);
            WriteValueChange(
                "DebugMode.HideActiveDataLossWarning",
                previousSettings.HideActiveDataLossWarning,
                currentSettings.HideActiveDataLossWarning);
            WriteValueChange(
                "DebugMode.SkipInitialDisclaimer",
                previousSettings.SkipInitialDisclaimer,
                currentSettings.SkipInitialDisclaimer);
            WriteValueChange(
                "DebugMode.SkipRecurringDataLossWarning",
                previousSettings.SkipRecurringDataLossWarning,
                currentSettings.SkipRecurringDataLossWarning);
            WriteValueChange(
                "DebugMode.SkipExperimentalActivationConfirmations",
                previousSettings.SkipExperimentalActivationConfirmations,
                currentSettings.SkipExperimentalActivationConfirmations);
            WriteValueChange(
                "DebugMode.SkipDeletionConfirmationDialogs",
                previousSettings.SkipDeletionConfirmationDialogs,
                currentSettings.SkipDeletionConfirmationDialogs);
        }

        private static void WriteBackupPairChanges(
            IReadOnlyList<BackupPathPair> previousPairs,
            IReadOnlyList<BackupPathPair> currentPairs)
        {
            int sharedCount =
                Math.Min(
                    previousPairs.Count,
                    currentPairs.Count);

            for (int index = 0; index < sharedCount; index++)
            {
                BackupPathPair previousPair =
                    previousPairs[index];
                BackupPathPair currentPair =
                    currentPairs[index];

                string pairName =
                    $"BackupPair[{index}]";

                WriteValueChange(pairName + ".IsEnabled", previousPair.IsEnabled, currentPair.IsEnabled);
                WriteValueChange(pairName + ".SourceDirectory", previousPair.SourceDirectory, currentPair.SourceDirectory);
                WriteValueChange(pairName + ".TargetDirectory", previousPair.TargetDirectory, currentPair.TargetDirectory);
                WriteValueChange(pairName + ".Versioning", previousPair.Versioning, currentPair.Versioning);
                WriteValueChange(pairName + ".IgnoreCopyErrors", previousPair.IgnoreCopyErrors, currentPair.IgnoreCopyErrors);
                WriteValueChange(pairName + ".SkipDialogs", previousPair.SkipDialogs, currentPair.SkipDialogs);
                WriteValueChange(pairName + ".AutoBackupIntervalSeconds", previousPair.AutoBackupIntervalSeconds, currentPair.AutoBackupIntervalSeconds);
                WriteValueChange(pairName + ".RetentionKeepLastEnabled", previousPair.RetentionKeepLastEnabled, currentPair.RetentionKeepLastEnabled);
                WriteValueChange(pairName + ".RetentionKeepLastCount", previousPair.RetentionKeepLastCount, currentPair.RetentionKeepLastCount);
                WriteValueChange(pairName + ".RetentionKeepDaysEnabled", previousPair.RetentionKeepDaysEnabled, currentPair.RetentionKeepDaysEnabled);
                WriteValueChange(pairName + ".RetentionKeepDaysCount", previousPair.RetentionKeepDaysCount, currentPair.RetentionKeepDaysCount);
                WriteValueChange(pairName + ".RetentionMode", previousPair.RetentionMode, currentPair.RetentionMode);
                WriteListChange(pairName + ".RetentionExcludedTags", previousPair.RetentionExcludedTags, currentPair.RetentionExcludedTags);
                WriteListChange(pairName + ".ExcludedPaths", previousPair.ExcludedPaths, currentPair.ExcludedPaths);
                WriteValueChange(pairName + ".SourceCleanupEnabled", previousPair.SourceCleanupEnabled, currentPair.SourceCleanupEnabled);
                WriteValueChange(pairName + ".SourceCleanupRelativeDirectory", previousPair.SourceCleanupRelativeDirectory, currentPair.SourceCleanupRelativeDirectory);
                WriteListChange(pairName + ".SourceCleanupFileExtensions", previousPair.SourceCleanupFileExtensions, currentPair.SourceCleanupFileExtensions);
                WriteValueChange(pairName + ".SourceCleanupMode", previousPair.SourceCleanupMode, currentPair.SourceCleanupMode);
                WriteValueChange(pairName + ".SourceCleanupKeepLastCount", previousPair.SourceCleanupKeepLastCount, currentPair.SourceCleanupKeepLastCount);
                WriteValueChange(pairName + ".SourceCleanupKeepAfterDate", previousPair.SourceCleanupKeepAfterDate, currentPair.SourceCleanupKeepAfterDate);
            }

            for (int index = sharedCount; index < currentPairs.Count; index++)
            {
                BackupPathPair pair =
                    currentPairs[index];

                WriteLine(
                    $"SETTING ADDED | setting=\"BackupPair[{index}]\" | source=\"{Escape(pair.SourceDirectory)}\" | target=\"{Escape(pair.TargetDirectory)}\"");
            }

            for (int index = sharedCount; index < previousPairs.Count; index++)
            {
                BackupPathPair pair =
                    previousPairs[index];

                WriteLine(
                    $"SETTING REMOVED | setting=\"BackupPair[{index}]\" | source=\"{Escape(pair.SourceDirectory)}\" | target=\"{Escape(pair.TargetDirectory)}\"");
            }
        }

        private static void WriteValueChange<T>(
            string settingName,
            T previousValue,
            T currentValue)
        {
            if (EqualityComparer<T>.Default.Equals(
                    previousValue,
                    currentValue))
            {
                return;
            }

            WriteLine(
                $"SETTING CHANGED | setting=\"{Escape(settingName)}\" | old=\"{Escape(FormatValue(previousValue))}\" | new=\"{Escape(FormatValue(currentValue))}\"");
        }

        private static void WriteListChange(
            string settingName,
            IEnumerable<string>? previousValues,
            IEnumerable<string>? currentValues)
        {
            string previousValue =
                FormatList(previousValues);
            string currentValue =
                FormatList(currentValues);

            if (string.Equals(
                    previousValue,
                    currentValue,
                    StringComparison.Ordinal))
            {
                return;
            }

            WriteLine(
                $"SETTING CHANGED | setting=\"{Escape(settingName)}\" | old=\"{Escape(previousValue)}\" | new=\"{Escape(currentValue)}\"");
        }

        private static string FormatList(
            IEnumerable<string>? values)
        {
            return values == null
                ? string.Empty
                : string.Join(
                    " | ",
                    values);
        }

        private static string FormatValue<T>(
            T value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is IFormattable formattable)
            {
                return formattable.ToString(
                           null,
                           CultureInfo.InvariantCulture) ??
                       string.Empty;
            }

            return value.ToString() ??
                   string.Empty;
        }

        private static string Escape(
            string value)
        {
            return value
                .Replace(
                    "\\",
                    "\\\\",
                    StringComparison.Ordinal)
                .Replace(
                    "\"",
                    "\\\"",
                    StringComparison.Ordinal)
                .Replace(
                    "\r",
                    "\\r",
                    StringComparison.Ordinal)
                .Replace(
                    "\n",
                    "\\n",
                    StringComparison.Ordinal);
        }

        private static void WriteLine(
            string message)
        {
            try
            {
                lock (SyncRoot)
                {
                    Directory.CreateDirectory(
                        LogDirectoryPath);

                    string logLine =
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} | {message}{Environment.NewLine}";

                    File.AppendAllText(
                        LogFilePath,
                        logLine);
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(
                    "System log entry could not be written: " +
                    exception.Message);
            }
        }
    }
}
