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
        private bool _isRefreshingConfiguredPaths;
        private readonly ToolTip _mainToolTip = new ToolTip();

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
            SizeGripStyle = SizeGripStyle.Show;
            InitializeResizeGripPanel();
            InitializeMainToolbar();

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
            dataGridViewConfiguredPaths.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;

            dataGridViewConfiguredPaths.Columns["ColumnConfiguredSourceDirectory"].ReadOnly = false;
            dataGridViewConfiguredPaths.Columns["ColumnConfiguredTargetDirectory"].ReadOnly = false;

            InitializeAutoBackupTimerColumn();
            InitializeBackupInfoColumn();
            InitializeConfiguredPathActionColumns();

            dataGridViewConfiguredPaths.CellContentClick += dataGridViewConfiguredPaths_CellContentClick;
            dataGridViewConfiguredPaths.CellPainting += dataGridViewConfiguredPaths_CellPainting;

            _autoBackupCountdownTimer.Interval = TitleRefreshIntervalMilliseconds;
            _autoBackupCountdownTimer.Tick += autoBackupCountdownTimer_Tick;

            LoadSettings();
            ApplyWindowSettings();
            ApplyMainWindowHeightForThreeRows();
            RefreshConfiguredPaths();
            RestartAutoBackupCountdown();
        }
        private void InitializeMainToolbar()
        {
            menuStrip1.Visible = false;
            labelConfiguredPaths.Visible = false;

            int toolbarTop = 12;
            int buttonSize = 32;
            int buttonSpacing = 6;
            int left = 12;

            dataGridViewConfiguredPaths.Location = new Point(12, 56);
            dataGridViewConfiguredPaths.Size = new Size(ClientSize.Width - 24, ClientSize.Height - dataGridViewConfiguredPaths.Top - 12);

            buttonBackup.Location = new Point(ClientSize.Width - buttonBackup.Width - 12, toolbarTop + 3);
            buttonBackup.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _mainToolTip.SetToolTip(buttonBackup, "Start backup");

            Button buttonExit = CreateToolbarButton("buttonExit", string.Empty, "Exit", "Exit", new Point(left, toolbarTop));
            buttonExit.Click += exitToolStripMenuItem_Click;
            left += buttonSize + buttonSpacing;

            Button buttonAddConfiguredPath = CreateToolbarButton("buttonAddConfiguredPath", "+", string.Empty, "Add backup path", new Point(left, toolbarTop));
            buttonAddConfiguredPath.Click += buttonAddConfiguredPath_Click;
            left += buttonSize + buttonSpacing;

            Button buttonRemoveConfiguredPath = CreateToolbarButton("buttonRemoveConfiguredPath", "−", string.Empty, "Remove selected backup path", new Point(left, toolbarTop));
            buttonRemoveConfiguredPath.Click += buttonRemoveConfiguredPath_Click;
            left += buttonSize + buttonSpacing;

            Button buttonSettings = CreateToolbarButton("buttonSettings", string.Empty, "Settings", "Settings", new Point(left, toolbarTop));
            buttonSettings.Click += generalToolStripMenuItem_Click;
            left += buttonSize + buttonSpacing;

            Button buttonAbout = CreateToolbarButton("buttonAbout", "?", string.Empty, "About EasyVersionBackup", new Point(left, toolbarTop));
            buttonAbout.Click += helpToolStripMenuItem_Click;

            Controls.Add(buttonExit);
            Controls.Add(buttonAddConfiguredPath);
            Controls.Add(buttonRemoveConfiguredPath);
            Controls.Add(buttonSettings);
            Controls.Add(buttonAbout);

            buttonExit.BringToFront();
            buttonAddConfiguredPath.BringToFront();
            buttonRemoveConfiguredPath.BringToFront();
            buttonSettings.BringToFront();
            buttonAbout.BringToFront();
            buttonBackup.BringToFront();
        }
        private Button CreateToolbarButton(string name, string text, string iconType, string toolTipText, Point location)
        {
            Button button = new Button
            {
                Name = name,
                Text = text,
                Size = new Size(32, 32),
                Location = location,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 0, 0, 2),
                UseVisualStyleBackColor = false
            };

            button.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 210);
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(245, 245, 245);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(230, 230, 230);

            if (!string.IsNullOrWhiteSpace(toolTipText))
            {
                _mainToolTip.SetToolTip(button, toolTipText);
            }

            if (!string.IsNullOrWhiteSpace(iconType))
            {
                button.Paint += (sender, e) =>
                {
                    if (iconType == "Exit")
                    {
                        DrawToolbarExitIcon(e.Graphics, button.ClientRectangle);
                    }

                    if (iconType == "Settings")
                    {
                        DrawToolbarSettingsIcon(e.Graphics, button.ClientRectangle);
                    }
                };
            }

            return button;
        }
        private void DrawToolbarExitIcon(Graphics graphics, Rectangle bounds)
        {
            System.Drawing.Drawing2D.SmoothingMode previousSmoothingMode = graphics.SmoothingMode;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using Pen pen = new Pen(Color.Black, 1.2F)
            {
                StartCap = System.Drawing.Drawing2D.LineCap.Square,
                EndCap = System.Drawing.Drawing2D.LineCap.Square,
                LineJoin = System.Drawing.Drawing2D.LineJoin.Miter
            };

            using SolidBrush brush = new SolidBrush(Color.Black);

            int iconLeft = bounds.Left - 1;
            int iconTop = bounds.Top + 7;
            int iconBottom = bounds.Top + 25;
            int iconCenterY = bounds.Top + 16;

            graphics.DrawLine(pen, iconLeft + 10, iconTop + 1, iconLeft + 17, iconTop + 1);
            graphics.DrawLine(pen, iconLeft + 10, iconTop + 1, iconLeft + 10, iconCenterY - 2);
            graphics.DrawLine(pen, iconLeft + 10, iconCenterY + 3, iconLeft + 10, iconBottom - 1);
            graphics.DrawLine(pen, iconLeft + 10, iconBottom - 1, iconLeft + 17, iconBottom - 1);

            Point[] doorPoints =
            {
        new Point(iconLeft + 18, iconTop),
        new Point(iconLeft + 25, iconTop + 3),
        new Point(iconLeft + 25, iconBottom - 3),
        new Point(iconLeft + 18, iconBottom)
    };

            graphics.FillPolygon(brush, doorPoints);

            graphics.DrawLine(pen, iconLeft + 6, iconCenterY, iconLeft + 15, iconCenterY);
            graphics.DrawLine(pen, iconLeft + 12, iconCenterY - 2, iconLeft + 15, iconCenterY);
            graphics.DrawLine(pen, iconLeft + 12, iconCenterY + 2, iconLeft + 15, iconCenterY);

            graphics.SmoothingMode = previousSmoothingMode;
        }

        private void DrawToolbarSettingsIcon(Graphics graphics, Rectangle bounds)
        {
            System.Drawing.Drawing2D.SmoothingMode previousSmoothingMode = graphics.SmoothingMode;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using Pen pen = new Pen(Color.Black, 1.2F)
            {
                StartCap = System.Drawing.Drawing2D.LineCap.Square,
                EndCap = System.Drawing.Drawing2D.LineCap.Square,
                LineJoin = System.Drawing.Drawing2D.LineJoin.Miter
            };

            int left = bounds.Left + 7;
            int top = bounds.Top + 7;

            Point[] gearPoints =
            {
        new Point(left + 7, top),
        new Point(left + 11, top),
        new Point(left + 11, top + 3),
        new Point(left + 12, top + 4),
        new Point(left + 15, top + 2),
        new Point(left + 17, top + 4),
        new Point(left + 15, top + 7),
        new Point(left + 16, top + 8),
        new Point(left + 18, top + 8),
        new Point(left + 18, top + 10),
        new Point(left + 16, top + 10),
        new Point(left + 15, top + 12),
        new Point(left + 17, top + 15),
        new Point(left + 15, top + 17),
        new Point(left + 12, top + 15),
        new Point(left + 11, top + 16),
        new Point(left + 11, top + 19),
        new Point(left + 7, top + 19),
        new Point(left + 7, top + 16),
        new Point(left + 6, top + 15),
        new Point(left + 3, top + 17),
        new Point(left + 1, top + 15),
        new Point(left + 3, top + 12),
        new Point(left + 2, top + 10),
        new Point(left, top + 10),
        new Point(left, top + 8),
        new Point(left + 2, top + 8),
        new Point(left + 3, top + 7),
        new Point(left + 1, top + 4),
        new Point(left + 3, top + 2),
        new Point(left + 6, top + 4),
        new Point(left + 7, top + 3)
    };

            graphics.DrawPolygon(pen, gearPoints);
            graphics.DrawEllipse(pen, left + 6, top + 6, 6, 6);

            graphics.SmoothingMode = previousSmoothingMode;
        }
        private void buttonAddConfiguredPath_Click(object? sender, EventArgs e)
        {
            SyncEnabledPairsFromGrid();

            BackupPathPair pair = new BackupPathPair
            {
                IsEnabled = true,
                SourceDirectory = string.Empty,
                TargetDirectory = string.Empty,
                ExcludedPaths = new List<string>()
            };

            _settings.BackupPathPairs.Add(pair);

            SaveSettings();
            RefreshConfiguredPaths();

            int rowIndex = dataGridViewConfiguredPaths.Rows.Count - 1;

            if (rowIndex >= 0)
            {
                dataGridViewConfiguredPaths.ClearSelection();
                dataGridViewConfiguredPaths.Rows[rowIndex].Selected = true;
                dataGridViewConfiguredPaths.CurrentCell = dataGridViewConfiguredPaths.Rows[rowIndex].Cells["ColumnConfiguredSourceDirectory"];
                dataGridViewConfiguredPaths.BeginEdit(true);
            }
        }

        private void buttonRemoveConfiguredPath_Click(object? sender, EventArgs e)
        {
            if (dataGridViewConfiguredPaths.CurrentRow == null)
            {
                return;
            }

            int rowIndex = dataGridViewConfiguredPaths.CurrentRow.Index;

            if (rowIndex < 0 || rowIndex >= _settings.BackupPathPairs.Count)
            {
                return;
            }

            BackupPathPair pair = _settings.BackupPathPairs[rowIndex];

            if (ConfiguredPathContainsData(pair))
            {
                DialogResult result = MessageBox.Show(
                    "The selected path contains configured data. Do you really want to remove it?",
                    "Remove path",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            string pairKey = SettingsStorage.CreatePairKey(pair.SourceDirectory, pair.TargetDirectory);

            _settings.BackupPathPairs.RemoveAt(rowIndex);
            _settings.LastUsedVersionsByPair.Remove(pairKey);
            _settings.BackupStatusesByPair.Remove(pairKey);

            SaveSettings();
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
        private bool ConfiguredPathContainsData(BackupPathPair pair)
        {
            if (!string.IsNullOrWhiteSpace(pair.SourceDirectory))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(pair.TargetDirectory))
            {
                return true;
            }

            if (pair.ExcludedPaths.Any(excludedPath => !string.IsNullOrWhiteSpace(excludedPath)))
            {
                return true;
            }

            string pairKey = SettingsStorage.CreatePairKey(pair.SourceDirectory, pair.TargetDirectory);

            if (_settings.LastUsedVersionsByPair.ContainsKey(pairKey))
            {
                return true;
            }

            if (_settings.BackupStatusesByPair.ContainsKey(pairKey))
            {
                return true;
            }

            return false;
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
        private void ApplyMainWindowHeightForThreeRows()
        {
            int gridHeight = dataGridViewConfiguredPaths.ColumnHeadersHeight + (dataGridViewConfiguredPaths.RowTemplate.Height * 3) + 2;
            int clientHeight = dataGridViewConfiguredPaths.Top + gridHeight + 12;

            ClientSize = new Size(ClientSize.Width, clientHeight);
            MinimumSize = SizeFromClientSize(new Size(500, clientHeight));
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
            _isRefreshingConfiguredPaths = true;

            try
            {
                dataGridViewConfiguredPaths.Rows.Clear();

                if (_settings.BackupPathPairs.Count == 0)
                {
                    buttonBackup.Enabled = false;
                    return;
                }

                foreach (BackupPathPair pair in _settings.BackupPathPairs)
                {
                    int rowIndex = dataGridViewConfiguredPaths.Rows.Add();
                    DataGridViewRow row = dataGridViewConfiguredPaths.Rows[rowIndex];

                    row.Cells["ColumnConfiguredIsEnabled"].Value = pair.IsEnabled;
                    row.Cells["ColumnConfiguredAutoBackupTimer"].Value = string.Empty;
                    row.Cells["ColumnConfiguredSourceDirectory"].Value = pair.SourceDirectory;
                    row.Cells["ColumnConfiguredTargetDirectory"].Value = pair.TargetDirectory;
                    row.Cells["ColumnConfiguredBackupInfo"].Value = GetBackupInfoIcon(pair);
                    row.Cells["ColumnConfiguredBackupInfo"].ToolTipText = GetBackupInfoToolTipText(pair);

                    row.Tag = new List<string>(pair.ExcludedPaths);
                    UpdateConfiguredExclusionButtonStyle(rowIndex);
                }

                buttonBackup.Enabled = _settings.BackupPathPairs.Any(p => p.IsEnabled);
                RefreshAutoBackupTimerColumn();
            }
            finally
            {
                _isRefreshingConfiguredPaths = false;
            }
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

            notifyIconMain.Visible = true;
            notifyIconMain.BalloonTipTitle = "EasyVersionBackup";
            notifyIconMain.BalloonTipText = $"Backup completed. {skippedFiles} files skipped.";
            notifyIconMain.BalloonTipIcon = ToolTipIcon.Info;
            notifyIconMain.ShowBalloonTip(5000);
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
                DataGridViewRow row = dataGridViewConfiguredPaths.Rows[i];

                object? value = row.Cells["ColumnConfiguredIsEnabled"].Value;
                bool isEnabled = false;

                if (value != null)
                {
                    bool.TryParse(value.ToString(), out isEnabled);
                }

                _settings.BackupPathPairs[i].IsEnabled = isEnabled;
                _settings.BackupPathPairs[i].SourceDirectory = row.Cells["ColumnConfiguredSourceDirectory"].Value?.ToString()?.Trim() ?? string.Empty;
                _settings.BackupPathPairs[i].TargetDirectory = row.Cells["ColumnConfiguredTargetDirectory"].Value?.ToString()?.Trim() ?? string.Empty;
                _settings.BackupPathPairs[i].ExcludedPaths = row.Tag is List<string> excludedPaths
                    ? new List<string>(excludedPaths)
                    : new List<string>();
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
        private void InitializeConfiguredPathActionColumns()
        {
            if (!dataGridViewConfiguredPaths.Columns.Contains("ColumnConfiguredSourceBrowse"))
            {
                DataGridViewButtonColumn columnConfiguredSourceBrowse = new DataGridViewButtonColumn
                {
                    HeaderText = "",
                    Name = "ColumnConfiguredSourceBrowse",
                    Width = 35,
                    MinimumWidth = 35,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    FlatStyle = FlatStyle.Flat,
                    ToolTipText = "Browse source directory"
                };

                int sourceDirectoryColumnIndex = dataGridViewConfiguredPaths.Columns["ColumnConfiguredSourceDirectory"].Index;
                dataGridViewConfiguredPaths.Columns.Insert(sourceDirectoryColumnIndex + 1, columnConfiguredSourceBrowse);
            }

            if (!dataGridViewConfiguredPaths.Columns.Contains("ColumnConfiguredSourceExclusions"))
            {
                DataGridViewButtonColumn columnConfiguredSourceExclusions = new DataGridViewButtonColumn
                {
                    HeaderText = "",
                    Name = "ColumnConfiguredSourceExclusions",
                    Width = 35,
                    MinimumWidth = 35,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    FlatStyle = FlatStyle.Flat,
                    ToolTipText = "Edit excluded source paths"
                };

                int sourceBrowseColumnIndex = dataGridViewConfiguredPaths.Columns["ColumnConfiguredSourceBrowse"].Index;
                dataGridViewConfiguredPaths.Columns.Insert(sourceBrowseColumnIndex + 1, columnConfiguredSourceExclusions);
            }

            if (!dataGridViewConfiguredPaths.Columns.Contains("ColumnConfiguredTargetBrowse"))
            {
                DataGridViewButtonColumn columnConfiguredTargetBrowse = new DataGridViewButtonColumn
                {
                    HeaderText = "",
                    Name = "ColumnConfiguredTargetBrowse",
                    Width = 35,
                    MinimumWidth = 35,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    FlatStyle = FlatStyle.Flat,
                    ToolTipText = "Browse target directory"
                };

                int targetDirectoryColumnIndex = dataGridViewConfiguredPaths.Columns["ColumnConfiguredTargetDirectory"].Index;
                dataGridViewConfiguredPaths.Columns.Insert(targetDirectoryColumnIndex + 1, columnConfiguredTargetBrowse);
            }
        }

        private void ApplyInitialMainWindowHeightForThreeRows()
        {
            int gridHeight = dataGridViewConfiguredPaths.ColumnHeadersHeight + (dataGridViewConfiguredPaths.RowTemplate.Height * 3) + 2;
            int clientHeight = dataGridViewConfiguredPaths.Top + gridHeight + 12;

            ClientSize = new Size(ClientSize.Width, clientHeight);

            int minimumHeight = Height - ClientSize.Height + clientHeight;
            MinimumSize = new Size(500, minimumHeight);
        }

        private void dataGridViewConfiguredPaths_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string columnName = dataGridViewConfiguredPaths.Columns[e.ColumnIndex].Name;

            if (columnName == "ColumnConfiguredSourceBrowse")
            {
                using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

                string currentValue = dataGridViewConfiguredPaths.Rows[e.RowIndex].Cells["ColumnConfiguredSourceDirectory"].Value?.ToString() ?? string.Empty;
                if (Directory.Exists(currentValue))
                {
                    folderBrowserDialog.SelectedPath = currentValue;
                }

                if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
                {
                    dataGridViewConfiguredPaths.Rows[e.RowIndex].Cells["ColumnConfiguredSourceDirectory"].Value = folderBrowserDialog.SelectedPath;
                    SyncEnabledPairsFromGrid();
                }
            }

            if (columnName == "ColumnConfiguredSourceExclusions")
            {
                List<string> excludedPaths = dataGridViewConfiguredPaths.Rows[e.RowIndex].Tag is List<string> rowExcludedPaths
                    ? new List<string>(rowExcludedPaths)
                    : new List<string>();

                if (ShowConfiguredExclusionsDialog(excludedPaths, out List<string> resultExcludedPaths))
                {
                    dataGridViewConfiguredPaths.Rows[e.RowIndex].Tag = resultExcludedPaths;
                    UpdateConfiguredExclusionButtonStyle(e.RowIndex);
                    SyncEnabledPairsFromGrid();
                }
            }

            if (columnName == "ColumnConfiguredTargetBrowse")
            {
                using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

                string currentValue = dataGridViewConfiguredPaths.Rows[e.RowIndex].Cells["ColumnConfiguredTargetDirectory"].Value?.ToString() ?? string.Empty;
                if (Directory.Exists(currentValue))
                {
                    folderBrowserDialog.SelectedPath = currentValue;
                }

                if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
                {
                    dataGridViewConfiguredPaths.Rows[e.RowIndex].Cells["ColumnConfiguredTargetDirectory"].Value = folderBrowserDialog.SelectedPath;
                    SyncEnabledPairsFromGrid();
                }
            }
        }

        private void dataGridViewConfiguredPaths_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string columnName = dataGridViewConfiguredPaths.Columns[e.ColumnIndex].Name;

            if (columnName != "ColumnConfiguredSourceBrowse" &&
                columnName != "ColumnConfiguredSourceExclusions" &&
                columnName != "ColumnConfiguredTargetBrowse")
            {
                return;
            }

            e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);

            bool isSelected = (e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected;
            Color outlineColor = isSelected ? Color.White : Color.Black;

            if (columnName == "ColumnConfiguredSourceBrowse" || columnName == "ColumnConfiguredTargetBrowse")
            {
                DrawConfiguredBrowseIcon(e.Graphics, e.CellBounds, outlineColor);
            }

            if (columnName == "ColumnConfiguredSourceExclusions")
            {
                bool hasExclusions = dataGridViewConfiguredPaths.Rows[e.RowIndex].Tag is List<string> excludedPaths
                    && excludedPaths.Any(excludedPath => !string.IsNullOrWhiteSpace(excludedPath));

                DrawConfiguredFilterIcon(e.Graphics, e.CellBounds, hasExclusions, outlineColor);
            }

            e.Handled = true;
        }

        private void DrawConfiguredBrowseIcon(Graphics graphics, Rectangle cellBounds, Color outlineColor)
        {
            int centerX = cellBounds.Left + cellBounds.Width / 2;
            int centerY = cellBounds.Top + cellBounds.Height / 2 - 1;

            System.Drawing.Drawing2D.SmoothingMode previousSmoothingMode = graphics.SmoothingMode;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using Pen pen = new Pen(Color.Black, 1.6F);

            graphics.DrawEllipse(pen, centerX - 6, centerY - 6, 9, 9);
            graphics.DrawLine(pen, centerX + 1, centerY + 1, centerX + 7, centerY + 7);

            graphics.SmoothingMode = previousSmoothingMode;
        }

        private void DrawConfiguredFilterIcon(Graphics graphics, Rectangle cellBounds, bool hasExclusions, Color outlineColor)
        {
            int centerX = cellBounds.Left + cellBounds.Width / 2;
            int centerY = cellBounds.Top + cellBounds.Height / 2;

            Point[] funnelPoints =
            {
        new Point(centerX - 8, centerY - 7),
        new Point(centerX + 8, centerY - 7),
        new Point(centerX + 3, centerY - 1),
        new Point(centerX + 1, centerY + 7),
        new Point(centerX - 1, centerY + 7),
        new Point(centerX - 3, centerY - 1)
    };

            System.Drawing.Drawing2D.SmoothingMode previousSmoothingMode = graphics.SmoothingMode;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using Pen pen = new Pen(hasExclusions ? Color.Black : outlineColor, 1F);

            if (hasExclusions)
            {
                using SolidBrush brush = new SolidBrush(Color.LimeGreen);
                graphics.FillPolygon(brush, funnelPoints);
            }

            graphics.DrawPolygon(pen, funnelPoints);

            graphics.SmoothingMode = previousSmoothingMode;
        }

        private void UpdateConfiguredExclusionButtonStyle(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dataGridViewConfiguredPaths.Rows.Count)
            {
                return;
            }

            if (!dataGridViewConfiguredPaths.Columns.Contains("ColumnConfiguredSourceExclusions"))
            {
                return;
            }

            dataGridViewConfiguredPaths.InvalidateCell(dataGridViewConfiguredPaths.Rows[rowIndex].Cells["ColumnConfiguredSourceExclusions"]);
        }

        private bool ShowConfiguredExclusionsDialog(List<string> excludedPaths, out List<string> resultExcludedPaths)
        {
            resultExcludedPaths = excludedPaths;

            using Form form = new Form();
            using TextBox textBoxExclusions = new TextBox();
            using Button buttonOk = new Button();
            using Button buttonCancel = new Button();

            form.Text = "Exclusions";
            form.StartPosition = FormStartPosition.CenterParent;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.ClientSize = new Size(520, 321);

            textBoxExclusions.Multiline = true;
            textBoxExclusions.ScrollBars = ScrollBars.Vertical;
            textBoxExclusions.WordWrap = false;
            textBoxExclusions.Location = new Point(12, 12);
            textBoxExclusions.Size = new Size(496, 263);
            textBoxExclusions.Text = string.Join(Environment.NewLine, excludedPaths);

            buttonOk.Text = "OK";
            buttonOk.DialogResult = DialogResult.OK;
            buttonOk.TextAlign = ContentAlignment.MiddleCenter;
            buttonOk.Padding = Padding.Empty;
            buttonOk.Location = new Point(352, 282);
            buttonOk.Size = new Size(75, 27);

            buttonCancel.Text = "Cancel";
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.TextAlign = ContentAlignment.MiddleCenter;
            buttonCancel.Padding = Padding.Empty;
            buttonCancel.Location = new Point(433, 282);
            buttonCancel.Size = new Size(75, 27);

            form.Controls.Add(textBoxExclusions);
            form.Controls.Add(buttonOk);
            form.Controls.Add(buttonCancel);
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            if (form.ShowDialog(this) != DialogResult.OK)
            {
                return false;
            }

            resultExcludedPaths = textBoxExclusions.Lines
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            return true;
        }
        private void dataGridViewConfiguredPaths_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_isRefreshingConfiguredPaths)
            {
                return;
            }

            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string columnName = dataGridViewConfiguredPaths.Columns[e.ColumnIndex].Name;

            if (columnName == "ColumnConfiguredIsEnabled" ||
                columnName == "ColumnConfiguredSourceDirectory" ||
                columnName == "ColumnConfiguredTargetDirectory")
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
        private void InitializeResizeGripPanel()
        {
            Panel panelResizeGrip = new Panel
            {
                Name = "panelResizeGrip",
                Size = new Size(18, 18),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(ClientSize.Width - 18, ClientSize.Height - 18),
                Cursor = Cursors.SizeNWSE,
                BackColor = BackColor
            };

            panelResizeGrip.Paint += (sender, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using Pen penDark = new Pen(Color.FromArgb(120, 120, 120), 1F);
                using Pen penLight = new Pen(Color.White, 1F);

                for (int offset = 4; offset <= 14; offset += 4)
                {
                    e.Graphics.DrawLine(penLight, 18 - offset + 1, 18, 18, 18 - offset + 1);
                    e.Graphics.DrawLine(penDark, 18 - offset, 18, 18, 18 - offset);
                }
            };

            panelResizeGrip.MouseDown += (sender, e) =>
            {
                if (e.Button != MouseButtons.Left)
                {
                    return;
                }

                const int wmNclbuttondown = 0xA1;
                const int htBottomRight = 17;

                Capture = false;
                Message message = Message.Create(Handle, wmNclbuttondown, new IntPtr(htBottomRight), IntPtr.Zero);
                DefWndProc(ref message);
            };

            Controls.Add(panelResizeGrip);
            panelResizeGrip.BringToFront();
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