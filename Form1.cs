using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;

namespace EasyVersionBackup
{
    public partial class Form1 : Form
    {
        private AppSettings _settings = new AppSettings();
        private bool _ignoreAllFileErrors;
        private readonly System.Windows.Forms.Timer _autoBackupCountdownTimer = new System.Windows.Forms.Timer();
        private DateTime _nextAutoBackupRun;

        // # Title Refresh Interval
        private const int TitleRefreshIntervalMilliseconds = 1000;
        private string _baseWindowTitle = string.Empty;

        public Form1()
        {
            InitializeComponent();

            _baseWindowTitle = Text;

            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            notifyIconMain.Icon = Icon;

            // VISUAL IMPROVEMENTS
            Font = new Font("Segoe UI", 9F);
            BackColor = Color.FromArgb(245, 245, 245);

            buttonBackup.FlatStyle = FlatStyle.Flat;
            buttonBackup.FlatAppearance.BorderSize = 0;
            buttonBackup.BackColor = Color.FromArgb(0, 120, 215);
            buttonBackup.ForeColor = Color.White;
            buttonBackup.Cursor = Cursors.Hand;

            buttonBackup.MouseEnter += (s, e) => buttonBackup.BackColor = Color.FromArgb(30, 144, 255);
            buttonBackup.MouseLeave += (s, e) => buttonBackup.BackColor = Color.FromArgb(0, 120, 215);

            dataGridViewConfiguredPaths.BorderStyle = BorderStyle.None;
            dataGridViewConfiguredPaths.BackgroundColor = Color.White;
            dataGridViewConfiguredPaths.EnableHeadersVisualStyles = false;
            dataGridViewConfiguredPaths.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            dataGridViewConfiguredPaths.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            dataGridViewConfiguredPaths.ColumnHeadersDefaultCellStyle.SelectionBackColor =
                dataGridViewConfiguredPaths.ColumnHeadersDefaultCellStyle.BackColor;
            dataGridViewConfiguredPaths.ColumnHeadersDefaultCellStyle.SelectionForeColor =
                dataGridViewConfiguredPaths.ColumnHeadersDefaultCellStyle.ForeColor;

            dataGridViewConfiguredPaths.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 235, 252);
            dataGridViewConfiguredPaths.DefaultCellStyle.SelectionForeColor = Color.Black;

            dataGridViewConfiguredPaths.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);

            InitializeAutoBackupTimerColumn();
            InitializeBackupInfoColumn();

            _autoBackupCountdownTimer.Interval = TitleRefreshIntervalMilliseconds;
            _autoBackupCountdownTimer.Tick += autoBackupCountdownTimer_Tick;

            LoadSettings();
            ApplyWindowSettings();
            RefreshConfiguredPaths();
            RestartAutoBackupCountdown();
        }

        private void LoadSettings()
        {
            _settings = SettingsStorage.Load();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using AboutForm form = new AboutForm();
            form.ShowDialog(this);
        }

        private void ApplyWindowSettings()
        {
            StartPosition = FormStartPosition.Manual;

            if (_settings.MainWindowWidth > 0 && _settings.MainWindowHeight > 0)
            {
                Size = new Size(_settings.MainWindowWidth, _settings.MainWindowHeight);
            }

            if (_settings.MainWindowLeft >= 0 && _settings.MainWindowTop >= 0)
            {
                Location = new Point(_settings.MainWindowLeft, _settings.MainWindowTop);
            }
            else
            {
                StartPosition = FormStartPosition.CenterScreen;
            }
        }

        private void SaveWindowSettings()
        {
            Rectangle bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;

            _settings.MainWindowWidth = bounds.Width;
            _settings.MainWindowHeight = bounds.Height;
            _settings.MainWindowLeft = bounds.Left;
            _settings.MainWindowTop = bounds.Top;

            SaveSettings();
        }

        private void SaveSettings()
        {
            SettingsStorage.Save(_settings);
        }

        private void RefreshConfiguredPaths()
        {
            dataGridViewConfiguredPaths.Rows.Clear();

            if (_settings.BackupPathPairs.Count == 0)
            {
                buttonBackup.Enabled = false;
                return;
            }

            foreach (BackupPathPair pair in _settings.BackupPathPairs)
            {
                int rowIndex = dataGridViewConfiguredPaths.Rows.Add(
                    pair.IsEnabled,
                    string.Empty,
                    pair.SourceDirectory,
                    pair.TargetDirectory,
                    GetBackupInfoIcon(pair));

                dataGridViewConfiguredPaths.Rows[rowIndex].Cells["ColumnConfiguredBackupInfo"].ToolTipText = GetBackupInfoToolTipText(pair);
            }

            buttonBackup.Enabled = _settings.BackupPathPairs.Any(p => p.IsEnabled);
            RefreshAutoBackupTimerColumn();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIconMain.Visible = false;
            Close();
        }

        private void generalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using GeneralSettingsForm form = new GeneralSettingsForm(_settings);

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                _settings = form.ResultSettings;
                SaveSettings();
                RefreshConfiguredPaths();
                RestartAutoBackupCountdown();
            }
        }

        private void buttonBackup_Click(object sender, EventArgs e)
        {
            _ignoreAllFileErrors = _settings.IgnoreCopyErrors;

            SyncEnabledPairsFromGrid();

            List<BackupPathPair> validPairs = _settings.BackupPathPairs
                .Where(p => p.IsEnabled && !string.IsNullOrWhiteSpace(p.SourceDirectory) && !string.IsNullOrWhiteSpace(p.TargetDirectory))
                .ToList();

            if (validPairs.Count == 0)
            {
                MessageBox.Show("No active paths selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (BackupPathPair pair in validPairs)
            {
                if (!Directory.Exists(pair.SourceDirectory))
                {
                    SetBackupStatus(pair, "Error", $"Source directory not found: {pair.SourceDirectory}");
                    SaveSettings();
                    RefreshBackupInfoColumn();

                    MessageBox.Show(
                        $"Source directory not found:{Environment.NewLine}{pair.SourceDirectory}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                if (!Directory.Exists(pair.TargetDirectory))
                {
                    Directory.CreateDirectory(pair.TargetDirectory);
                }
            }

            List<BackupVersionItem> versionItems = validPairs
                .Select(pair => new BackupVersionItem
                {
                    SourceDirectory = pair.SourceDirectory,
                    TargetDirectory = pair.TargetDirectory,
                    SourceName = new DirectoryInfo(pair.SourceDirectory).Name,
                    Version = VersionHelper.GetSuggestedVersion(_settings, pair)
                })
                .ToList();

            using VersionInputForm versionForm = new VersionInputForm(versionItems, _settings.IgnoreCopyErrors);

            if (versionForm.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _settings.IgnoreCopyErrors = versionForm.IgnoreCopyErrors;
            _ignoreAllFileErrors = _settings.IgnoreCopyErrors;

            int skippedFiles = 0;

            for (int i = 0; i < validPairs.Count; i++)
            {
                BackupPathPair pair = validPairs[i];
                BackupVersionItem versionItem = versionForm.ResultItems[i];

                try
                {
                    int skippedForPair = ExecuteBackup(pair, versionItem.Version);
                    skippedFiles += skippedForPair;

                    SetBackupStatus(
                        pair,
                        skippedForPair == 0 ? "OK" : "Warning",
                        skippedForPair == 0 ? string.Empty : $"{skippedForPair} files skipped.");

                    string key = SettingsStorage.CreatePairKey(pair.SourceDirectory, pair.TargetDirectory);
                    _settings.LastUsedVersionsByPair[key] = versionItem.Version;
                }
                catch (Exception exception)
                {
                    SetBackupStatus(pair, "Error", exception.Message);
                    SaveSettings();
                    RefreshBackupInfoColumn();
                    throw;
                }
            }

            SaveSettings();
            RefreshBackupInfoColumn();

            MessageBox.Show($"Backup completed.{Environment.NewLine}{skippedFiles} files skipped.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private int ExecuteBackup(BackupPathPair pair, string version)
        {
            int skipped = 0;

            string sourceName = new DirectoryInfo(pair.SourceDirectory).Name;
            string versionedName = VersionHelper.BuildVersionedName(sourceName, version);

            if (_settings.ZipDestinationFiles)
            {
                string zipPath = Path.Combine(pair.TargetDirectory, $"{versionedName}.zip");

                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }

                skipped += CreateZipFromDirectory(pair.SourceDirectory, zipPath, pair.ExcludedPaths);
                return skipped;
            }

            string destinationDirectory = Path.Combine(pair.TargetDirectory, versionedName);

            if (Directory.Exists(destinationDirectory))
            {
                Directory.Delete(destinationDirectory, true);
            }

            skipped += CopyDirectory(pair.SourceDirectory, destinationDirectory, pair.ExcludedPaths);

            return skipped;
        }

        private int CopyDirectory(string sourceDirectory, string destinationDirectory, List<string> excludedPaths)
        {
            int skipped = 0;

            Directory.CreateDirectory(destinationDirectory);

            foreach (string directoryPath in GetIncludedDirectories(sourceDirectory, excludedPaths))
            {
                string relativePath = Path.GetRelativePath(sourceDirectory, directoryPath);
                string targetDirectoryPath = Path.Combine(destinationDirectory, relativePath);
                Directory.CreateDirectory(targetDirectoryPath);

                foreach (string filePath in Directory.GetFiles(directoryPath, "*", SearchOption.TopDirectoryOnly))
                {
                    if (IsExcludedPath(sourceDirectory, filePath, excludedPaths))
                    {
                        continue;
                    }

                    string relativeFilePath = Path.GetRelativePath(sourceDirectory, filePath);
                    string targetFilePath = Path.Combine(destinationDirectory, relativeFilePath);

                    try
                    {
                        using FileStream sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        using FileStream targetStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.None);

                        sourceStream.CopyTo(targetStream);
                    }
                    catch
                    {
                        skipped++;
                        if (!_ignoreAllFileErrors)
                        {
                            throw;
                        }
                    }
                }
            }

            return skipped;
        }

        private int CreateZipFromDirectory(string sourceDirectory, string zipPath, List<string> excludedPaths)
        {
            int skipped = 0;

            using FileStream zipStream = new FileStream(zipPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            using ZipArchive zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create);

            foreach (string directoryPath in GetIncludedDirectories(sourceDirectory, excludedPaths))
            {
                foreach (string filePath in Directory.GetFiles(directoryPath, "*", SearchOption.TopDirectoryOnly))
                {
                    if (IsExcludedPath(sourceDirectory, filePath, excludedPaths))
                    {
                        continue;
                    }

                    string relativeFilePath = Path.GetRelativePath(sourceDirectory, filePath).Replace('\\', '/');

                    try
                    {
                        using FileStream sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        ZipArchiveEntry entry = zipArchive.CreateEntry(relativeFilePath, CompressionLevel.Optimal);

                        using Stream entryStream = entry.Open();
                        sourceStream.CopyTo(entryStream);
                    }
                    catch
                    {
                        skipped++;
                        if (!_ignoreAllFileErrors)
                        {
                            throw;
                        }
                    }
                }
            }

            return skipped;
        }
        private List<string> GetIncludedDirectories(string sourceDirectory, List<string> excludedPaths)
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
        private bool IsExcludedPath(string sourceDirectory, string path, List<string> excludedPaths)
        {
            if (excludedPaths.Count == 0)
            {
                return false;
            }

            string fullSourceDirectory = Path.GetFullPath(sourceDirectory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            foreach (string excludedPath in excludedPaths)
            {
                if (string.IsNullOrWhiteSpace(excludedPath))
                {
                    continue;
                }

                string fullExcludedPath = Path.IsPathRooted(excludedPath)
                    ? Path.GetFullPath(excludedPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    : Path.GetFullPath(Path.Combine(fullSourceDirectory, excludedPath)).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                if (string.Equals(fullPath, fullExcludedPath, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (fullPath.StartsWith(fullExcludedPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (fullPath.StartsWith(fullExcludedPath + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
        private FileErrorAction ShowFileErrorActionDialog(string filePath, Exception exception)
        {
            if (_ignoreAllFileErrors)
            {
                return FileErrorAction.IgnoreAll;
            }

            using FileErrorDialog dialog = new FileErrorDialog(filePath, exception.Message);
            DialogResult result = dialog.ShowDialog(this);

            if (result == DialogResult.Retry)
            {
                return FileErrorAction.Retry;
            }

            if (result == DialogResult.Ignore)
            {
                return FileErrorAction.Skip;
            }

            if (result == DialogResult.Yes)
            {
                return FileErrorAction.IgnoreAll;
            }

            return FileErrorAction.Abort;
        }

        private void ExecuteAutomaticBackup()
        {
            _ignoreAllFileErrors = true;

            List<BackupPathPair> validPairs = _settings.BackupPathPairs
                .Where(p => p.IsEnabled && !string.IsNullOrWhiteSpace(p.SourceDirectory) && !string.IsNullOrWhiteSpace(p.TargetDirectory))
                .ToList();

            if (validPairs.Count == 0)
            {
                return;
            }

            foreach (BackupPathPair pair in validPairs)
            {
                if (!Directory.Exists(pair.SourceDirectory))
                {
                    SetBackupStatus(pair, "Error", $"Source directory not found: {pair.SourceDirectory}");
                    SaveSettings();
                    RefreshBackupInfoColumn();
                    return;
                }

                if (!Directory.Exists(pair.TargetDirectory))
                {
                    Directory.CreateDirectory(pair.TargetDirectory);
                }
            }

            int skippedFiles = 0;
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");

            foreach (BackupPathPair pair in validPairs)
            {
                string suggestedVersion = VersionHelper.GetSuggestedVersion(_settings, pair);
                string automaticVersion = string.IsNullOrWhiteSpace(suggestedVersion)
                    ? timestamp
                    : suggestedVersion + "_" + timestamp;

                try
                {
                    int skippedForPair = ExecuteBackup(pair, automaticVersion);
                    skippedFiles += skippedForPair;

                    SetBackupStatus(
                        pair,
                        skippedForPair == 0 ? "OK" : "Warning",
                        skippedForPair == 0 ? string.Empty : $"{skippedForPair} files skipped.");

                    string key = SettingsStorage.CreatePairKey(pair.SourceDirectory, pair.TargetDirectory);
                    _settings.LastUsedVersionsByPair[key] = suggestedVersion;
                }
                catch (Exception exception)
                {
                    SetBackupStatus(pair, "Error", exception.Message);
                }
            }

            SaveSettings();
            RefreshBackupInfoColumn();

            notifyIconMain.Visible = true;
            notifyIconMain.BalloonTipTitle = "EasyVersionBackup";
            notifyIconMain.BalloonTipText = $"Auto-Backup completed. {skippedFiles} files skipped.";
            notifyIconMain.ShowBalloonTip(5000);
        }
        private void autoBackupCountdownTimer_Tick(object? sender, EventArgs e)
        {
            RefreshAutoBackupTimerColumn();
            RefreshWindowTitleCountdown();
            RefreshNotifyIconText();

            if (DateTime.Now < _nextAutoBackupRun)
            {
                return;
            }

            ExecuteAutomaticBackup();

            _nextAutoBackupRun = DateTime.Now.AddSeconds(GetAutoBackupIntervalSeconds());
            RefreshAutoBackupTimerColumn();
            RefreshWindowTitleCountdown();
            RefreshNotifyIconText();
        }
        private void RefreshAutoBackupTimerColumn()
        {
            for (int i = 0; i < dataGridViewConfiguredPaths.Rows.Count && i < _settings.BackupPathPairs.Count; i++)
            {
                DataGridViewRow row = dataGridViewConfiguredPaths.Rows[i];

                if (!_settings.AutoBackupEnabled || !_settings.BackupPathPairs[i].IsEnabled)
                {
                    row.Cells["ColumnConfiguredAutoBackupTimer"].Value = string.Empty;
                    continue;
                }

                TimeSpan remaining = _nextAutoBackupRun - DateTime.Now;

                if (remaining < TimeSpan.Zero)
                {
                    remaining = TimeSpan.Zero;
                }

                row.Cells["ColumnConfiguredAutoBackupTimer"].Value = FormatAutoBackupInterval(remaining);
            }
        }
        private void RefreshWindowTitleCountdown()
        {
            if (!_settings.AutoBackupEnabled || !_settings.BackupPathPairs.Any(p => p.IsEnabled))
            {
                Text = _baseWindowTitle;
                return;
            }

            TimeSpan remaining = _nextAutoBackupRun - DateTime.Now;

            if (remaining < TimeSpan.Zero)
            {
                remaining = TimeSpan.Zero;
            }

            int totalSeconds = Math.Max(0, (int)Math.Ceiling(remaining.TotalSeconds));
            Text = $"{_baseWindowTitle} (Backup in: {totalSeconds} sek)";
        }
        private string FormatAutoBackupInterval(TimeSpan interval)
        {
            int totalSeconds = Math.Max(0, (int)Math.Ceiling(interval.TotalSeconds));

            if (totalSeconds < 60)
            {
                return totalSeconds + "s";
            }

            if (totalSeconds % 3600 == 0)
            {
                return (totalSeconds / 3600) + "h";
            }

            if (totalSeconds % 60 == 0)
            {
                return (totalSeconds / 60) + "m";
            }

            return totalSeconds + "s";
        }
        private int GetAutoBackupIntervalSeconds()
        {
            if (_settings.AutoBackupIntervalSeconds > 0)
            {
                return _settings.AutoBackupIntervalSeconds;
            }

            return Math.Max(1, _settings.AutoBackupIntervalMinutes) * 60;
        }
        private void RestartAutoBackupCountdown()
        {
            _autoBackupCountdownTimer.Stop();

            if (!_settings.AutoBackupEnabled || !_settings.BackupPathPairs.Any(p => p.IsEnabled))
            {
                RefreshAutoBackupTimerColumn();
                RefreshWindowTitleCountdown();
                RefreshNotifyIconText();
                return;
            }

            _nextAutoBackupRun = DateTime.Now.AddSeconds(GetAutoBackupIntervalSeconds());
            RefreshAutoBackupTimerColumn();
            RefreshWindowTitleCountdown();
            RefreshNotifyIconText();
            _autoBackupCountdownTimer.Start();
        }



        private void RefreshBackupInfoColumn()
        {
            for (int i = 0; i < dataGridViewConfiguredPaths.Rows.Count && i < _settings.BackupPathPairs.Count; i++)
            {
                BackupPathPair pair = _settings.BackupPathPairs[i];

                dataGridViewConfiguredPaths.Rows[i].Cells["ColumnConfiguredBackupInfo"].Value = GetBackupInfoIcon(pair);
                dataGridViewConfiguredPaths.Rows[i].Cells["ColumnConfiguredBackupInfo"].ToolTipText = GetBackupInfoToolTipText(pair);
            }
        }
        private string GetBackupInfoToolTipText(BackupPathPair pair)
        {
            string key = SettingsStorage.CreatePairKey(pair.SourceDirectory, pair.TargetDirectory);

            if (!_settings.BackupStatusesByPair.TryGetValue(key, out BackupPathStatus? status))
            {
                return "Last Backup: -";
            }

            string text = $"Last Backup: {status.LastBackupDateTime}";

            if (!string.IsNullOrWhiteSpace(status.LastBackupErrorMessage))
            {
                if (status.LastBackupStatus == "Error")
                {
                    text += $"{Environment.NewLine}Error: {status.LastBackupErrorMessage}";
                }
                else if (status.LastBackupStatus == "Warning")
                {
                    text += $"{Environment.NewLine}Warning: {status.LastBackupErrorMessage}";
                }
            }

            return text;
        }
        private Color GetBackupInfoColor(BackupPathPair pair)
        {
            string key = SettingsStorage.CreatePairKey(pair.SourceDirectory, pair.TargetDirectory);

            if (!_settings.BackupStatusesByPair.TryGetValue(key, out BackupPathStatus? status))
            {
                return Color.FromArgb(0, 120, 215);
            }

            if (status.LastBackupStatus == "OK")
            {
                return Color.FromArgb(0, 160, 80);
            }

            if (status.LastBackupStatus == "Warning")
            {
                return Color.FromArgb(230, 180, 0);
            }

            if (status.LastBackupStatus == "Error")
            {
                return Color.FromArgb(200, 0, 0);
            }

            return Color.FromArgb(0, 120, 215);
        }
        private Bitmap GetBackupInfoIcon(BackupPathPair pair)
        {
            Color color = GetBackupInfoColor(pair);
            Bitmap bitmap = new Bitmap(16, 16);

            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Transparent);

            using SolidBrush brush = new SolidBrush(color);
            graphics.FillEllipse(brush, 1, 1, 14, 14);

            using Font font = new Font("Segoe UI", 8F, FontStyle.Regular);
            Size textSize = TextRenderer.MeasureText("i", font);

            int x = (16 - textSize.Width) / 2 + 5;
            int y = (16 - textSize.Height) / 2 - 1;

            TextRenderer.DrawText(
                graphics,
                "i",
                font,
                new Point(x, y),
                Color.Black,
                TextFormatFlags.NoPadding);

            return bitmap;
        }
        private void SetBackupStatus(BackupPathPair pair, string status, string errorMessage)
        {
            string key = SettingsStorage.CreatePairKey(pair.SourceDirectory, pair.TargetDirectory);

            _settings.BackupStatusesByPair[key] = new BackupPathStatus
            {
                LastBackupDateTime = DateTime.Now.ToString("dd.MM.yyyy, HH:mm"),
                LastBackupStatus = status,
                LastBackupErrorMessage = errorMessage
            };
        }

        private void InitializeBackupInfoColumn()
        {
            if (dataGridViewConfiguredPaths.Columns.Contains("ColumnConfiguredBackupInfo"))
            {
                return;
            }

            DataGridViewImageColumn columnConfiguredBackupInfo = new DataGridViewImageColumn
            {
                HeaderText = "ℹ",
                Name = "ColumnConfiguredBackupInfo",
                ReadOnly = true,
                Width = 35,
                MinimumWidth = 35,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                ImageLayout = DataGridViewImageCellLayout.Normal
            };

            int targetColumnIndex = dataGridViewConfiguredPaths.Columns["ColumnConfiguredTargetDirectory"].Index;
            dataGridViewConfiguredPaths.Columns.Insert(targetColumnIndex + 1, columnConfiguredBackupInfo);
        }
        private void InitializeAutoBackupTimerColumn()
        {
            if (dataGridViewConfiguredPaths.Columns.Contains("ColumnConfiguredAutoBackupTimer"))
            {
                return;
            }

            DataGridViewTextBoxColumn columnConfiguredAutoBackupTimer = new DataGridViewTextBoxColumn
            {
                HeaderText = "Timer",
                Name = "ColumnConfiguredAutoBackupTimer",
                ReadOnly = true,
                Width = 60,
                MinimumWidth = 60,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            };

            dataGridViewConfiguredPaths.Columns.Insert(1, columnConfiguredAutoBackupTimer);
        }
        private void SyncEnabledPairsFromGrid()
        {
            for (int i = 0; i < _settings.BackupPathPairs.Count && i < dataGridViewConfiguredPaths.Rows.Count; i++)
            {
                object? value = dataGridViewConfiguredPaths.Rows[i].Cells["ColumnConfiguredIsEnabled"].Value;
                bool isEnabled = false;

                if (value != null)
                {
                    bool.TryParse(value.ToString(), out isEnabled);
                }

                _settings.BackupPathPairs[i].IsEnabled = isEnabled;
            }

            buttonBackup.Enabled = _settings.BackupPathPairs.Any(p => p.IsEnabled);
            RefreshAutoBackupTimerColumn();
            SaveSettings();
        }

        private void dataGridViewConfiguredPaths_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridViewConfiguredPaths.IsCurrentCellDirty)
            {
                dataGridViewConfiguredPaths.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void dataGridViewConfiguredPaths_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (dataGridViewConfiguredPaths.Columns[e.ColumnIndex].Name == "ColumnConfiguredIsEnabled")
            {
                SyncEnabledPairsFromGrid();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            notifyIconMain.Visible = false;
            SaveWindowSettings();
        }

        private void Form1_Move(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                SaveWindowSettings();
            }
        }
        private string FormatNotifyIconRemainingText(TimeSpan remaining)
        {
            int totalSeconds = Math.Max(0, (int)Math.Ceiling(remaining.TotalSeconds));

            if (totalSeconds < 60)
            {
                return totalSeconds == 1 ? "1 second" : totalSeconds + " seconds";
            }

            int totalMinutes = Math.Max(1, (int)Math.Ceiling(totalSeconds / 60.0));

            if (totalMinutes < 60)
            {
                return totalMinutes == 1 ? "1 minute" : totalMinutes + " minutes";
            }

            int totalHours = Math.Max(1, (int)Math.Ceiling(totalMinutes / 60.0));
            return totalHours == 1 ? "1 hour" : totalHours + " hours";
        }
        private void RefreshNotifyIconText()
        {
            if (!_settings.AutoBackupEnabled || !_settings.BackupPathPairs.Any(p => p.IsEnabled))
            {
                notifyIconMain.Text = "No backup scheduled";
                return;
            }

            TimeSpan remaining = _nextAutoBackupRun - DateTime.Now;

            if (remaining < TimeSpan.Zero)
            {
                remaining = TimeSpan.Zero;
            }

            notifyIconMain.Text = $"Next backup in {FormatNotifyIconRemainingText(remaining)} ({_nextAutoBackupRun:HH:mm})";
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (!_settings.MinimizeToSystray)
            {
                return;
            }

            if (WindowState != FormWindowState.Minimized)
            {
                return;
            }

            RefreshNotifyIconText();

            Hide();
            ShowInTaskbar = false;
            notifyIconMain.Visible = true;
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                SaveWindowSettings();
            }
        }

        private void notifyIconMain_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            RestoreFromSystray();
        }

        private void toolStripMenuItemOpen_Click(object sender, EventArgs e)
        {
            RestoreFromSystray();
        }

        private void toolStripMenuItemBackup_Click(object sender, EventArgs e)
        {
            buttonBackup_Click(sender, e);
        }

        private void toolStripMenuItemExitTray_Click(object sender, EventArgs e)
        {
            notifyIconMain.Visible = false;
            Close();
        }

        private void RestoreFromSystray()
        {
            Show();
            ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
            Activate();
            notifyIconMain.Visible = false;
        }

        private enum FileErrorAction
        {
            Retry,
            Skip,
            IgnoreAll,
            Abort
        }
    }
}