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

        public Form1()
        {
            InitializeComponent();

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

            // FIX: Header selection color (no blue highlight)
            dataGridViewConfiguredPaths.ColumnHeadersDefaultCellStyle.SelectionBackColor =
                dataGridViewConfiguredPaths.ColumnHeadersDefaultCellStyle.BackColor;
            dataGridViewConfiguredPaths.ColumnHeadersDefaultCellStyle.SelectionForeColor =
                dataGridViewConfiguredPaths.ColumnHeadersDefaultCellStyle.ForeColor;

            // FIX: softer row selection color
            dataGridViewConfiguredPaths.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 235, 252);
            dataGridViewConfiguredPaths.DefaultCellStyle.SelectionForeColor = Color.Black;

            dataGridViewConfiguredPaths.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);

            LoadSettings();
            ApplyWindowSettings();
            RefreshConfiguredPaths();
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
                dataGridViewConfiguredPaths.Rows.Add(
                    pair.IsEnabled,
                    pair.SourceDirectory,
                    pair.TargetDirectory);
            }

            buttonBackup.Enabled = _settings.BackupPathPairs.Any(p => p.IsEnabled);
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

                skippedFiles += ExecuteBackup(pair, versionItem.Version);

                string key = SettingsStorage.CreatePairKey(pair.SourceDirectory, pair.TargetDirectory);
                _settings.LastUsedVersionsByPair[key] = versionItem.Version;
            }

            SaveSettings();

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

                skipped += CreateZipFromDirectory(pair.SourceDirectory, zipPath);
                return skipped;
            }

            string destinationDirectory = Path.Combine(pair.TargetDirectory, versionedName);

            if (Directory.Exists(destinationDirectory))
            {
                Directory.Delete(destinationDirectory, true);
            }

            skipped += CopyDirectory(pair.SourceDirectory, destinationDirectory);

            return skipped;
        }

        private int CopyDirectory(string sourceDirectory, string destinationDirectory)
        {
            int skipped = 0;

            Directory.CreateDirectory(destinationDirectory);

            foreach (string directoryPath in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(sourceDirectory, directoryPath);
                string targetDirectoryPath = Path.Combine(destinationDirectory, relativePath);
                Directory.CreateDirectory(targetDirectoryPath);
            }

            foreach (string filePath in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(sourceDirectory, filePath);
                string targetFilePath = Path.Combine(destinationDirectory, relativePath);

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

            return skipped;
        }

        private int CreateZipFromDirectory(string sourceDirectory, string zipPath)
        {
            int skipped = 0;

            using FileStream zipStream = new FileStream(zipPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            using ZipArchive zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create);

            foreach (string filePath in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
            {
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

            return skipped;
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