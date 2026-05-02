using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace EasyVersionBackup
{
    public partial class GeneralSettingsForm : Form
    {
        private readonly ToolTip _settingsHintToolTip = new ToolTip();
        private PictureBox? _activeHintOwner;
        public AppSettings ResultSettings { get; private set; }

        public GeneralSettingsForm(AppSettings settings)
        {
            InitializeComponent();

            InitializeExclusionColumn();
            InitializeAutoBackupControls();
            InitializeSettingsHintIcons();
            InitializeSettingsControlToolTips();

            this.MouseDown += GeneralSettingsForm_MouseDown;

            dataGridViewPaths.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;
            dataGridViewPaths.KeyDown += dataGridViewPaths_KeyDown;
            dataGridViewPaths.CellPainting += dataGridViewPaths_CellPainting;

            ResultSettings = CloneSettings(settings);
            ApplySettingsToUi(ResultSettings);
        }

        private void checkBoxDummy1_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateAutoBackupControls();
        }
        private void UpdateAutoBackupControls()
        {
            TextBox textBoxAutoBackupInterval = GetAutoBackupIntervalTextBox();

            textBoxAutoBackupInterval.Enabled = checkBoxDummy1.Checked;
            checkBoxIgnoreCopyErrors.Enabled = !checkBoxDummy1.Checked;

            if (checkBoxDummy1.Checked)
            {
                checkBoxIgnoreCopyErrors.Checked = true;
            }
        }
        private void InitializeSettingsControlToolTips()
        {
            _settingsHintToolTip.SetToolTip(checkBoxZipDestinationFiles, "Create a ZIP archive instead of copying to a folder");
            _settingsHintToolTip.SetToolTip(comboBoxDefaultVersioning, "Default version or date pattern for new backups");
            _settingsHintToolTip.SetToolTip(checkBoxAutoIncrementVersion, "Automatically increment the suggested version");
            _settingsHintToolTip.SetToolTip(checkBoxMinimizeToSystray, "Hide the window in the system tray when minimized");
            _settingsHintToolTip.SetToolTip(checkBoxIgnoreCopyErrors, "Skip files that cannot be copied");
            _settingsHintToolTip.SetToolTip(checkBoxDummy1, "Run backups automatically / timer based");

            TextBox textBoxAutoBackupInterval = GetAutoBackupIntervalTextBox();
            _settingsHintToolTip.SetToolTip(textBoxAutoBackupInterval, "Time between automatic backups");

            if (dataGridViewPaths.Columns.Contains("ColumnSourceBrowse"))
            {
                dataGridViewPaths.Columns["ColumnSourceBrowse"].ToolTipText = "Browse source directory";
            }

            if (dataGridViewPaths.Columns.Contains("ColumnSourceExclusions"))
            {
                dataGridViewPaths.Columns["ColumnSourceExclusions"].ToolTipText = "Edit excluded source paths";
            }

            if (dataGridViewPaths.Columns.Contains("ColumnTargetBrowse"))
            {
                dataGridViewPaths.Columns["ColumnTargetBrowse"].ToolTipText = "Browse target directory";
            }
        }
        private void InitializeExclusionColumn()
        {
            if (dataGridViewPaths.Columns.Contains("ColumnSourceExclusions"))
            {
                return;
            }

            DataGridViewButtonColumn columnSourceExclusions = new DataGridViewButtonColumn
            {
                HeaderText = "",
                Name = "ColumnSourceExclusions",
                Text = "\uE71C",
                UseColumnTextForButtonValue = true,
                Width = 35,
                MinimumWidth = 35,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                FlatStyle = FlatStyle.Flat
            };

            columnSourceExclusions.DefaultCellStyle.Font = new Font("Segoe MDL2 Assets", 9F);
            columnSourceExclusions.DefaultCellStyle.ForeColor = System.Drawing.Color.Black;

            int sourceBrowseColumnIndex = dataGridViewPaths.Columns["ColumnSourceBrowse"].Index;
            dataGridViewPaths.Columns.Insert(sourceBrowseColumnIndex + 1, columnSourceExclusions);
        }
        private TextBox GetAutoBackupIntervalTextBox()
        {
            foreach (Control control in Controls)
            {
                if (control.Name == "textBoxAutoBackupInterval" && control is TextBox textBox)
                {
                    return textBox;
                }
            }

            throw new InvalidOperationException("textBoxAutoBackupInterval not found.");
        }
        private void InitializeAutoBackupControls()
        {
            labelDummy1.Text = "Backup Timer";
            checkBoxDummy1.CheckedChanged += checkBoxDummy1_CheckedChanged;

            TextBox textBoxAutoBackupInterval = new TextBox
            {
                Name = "textBoxAutoBackupInterval",
                Size = new System.Drawing.Size(45, 23),
                TabIndex = 13
            };

            textBoxAutoBackupInterval.Left = checkBoxDummy1.Right + 5;
            textBoxAutoBackupInterval.Top = checkBoxDummy1.Top + (checkBoxDummy1.Height - textBoxAutoBackupInterval.Height) / 2;

            Controls.Add(textBoxAutoBackupInterval);
        }
        private void InitializeSettingsHintIcons()
        {
            PictureBox pictureBoxDefaultVersioningHint = CreateSettingsHintIcon(
                "pictureBoxDefaultVersioningHint",
                "Default Versioning examples:" + Environment.NewLine +
                "none = no version suffix" + Environment.NewLine +
                "v1.0 = prefix + version number" + Environment.NewLine +
                "version1.0 = custom prefix + version number" + Environment.NewLine +
                "1.0 = version number without prefix" + Environment.NewLine +
                "yyyy-MM-dd = date, e.g. 2026-05-01" + Environment.NewLine +
                "yyyyMMdd = date, e.g. 20260501" + Environment.NewLine +
                "yyyy-MM-dd-HH-mm = date and time" + Environment.NewLine +
                "yyyyMMddHHmm = compact date and time" + Environment.NewLine +
                "... and much more, just experiment ;)");

            pictureBoxDefaultVersioningHint.Left = comboBoxDefaultVersioning.Right + 5;
            pictureBoxDefaultVersioningHint.Top = comboBoxDefaultVersioning.Top + (comboBoxDefaultVersioning.Height - pictureBoxDefaultVersioningHint.Height) / 2;

            TextBox textBoxAutoBackupInterval = GetAutoBackupIntervalTextBox();

            PictureBox pictureBoxAutoBackupTimerHint = CreateSettingsHintIcon(
                "pictureBoxAutoBackupTimerHint",
                "Backup Timer examples:" + Environment.NewLine +
                "30s = 30 seconds" + Environment.NewLine +
                "5m = 5 minutes" + Environment.NewLine +
                "1h = 1 hour" + Environment.NewLine +
                "15 = 15 minutes");

            pictureBoxAutoBackupTimerHint.Left = textBoxAutoBackupInterval.Right + 5;
            pictureBoxAutoBackupTimerHint.Top = textBoxAutoBackupInterval.Top + (textBoxAutoBackupInterval.Height - pictureBoxAutoBackupTimerHint.Height) / 2;

            Controls.Add(pictureBoxDefaultVersioningHint);
            Controls.Add(pictureBoxAutoBackupTimerHint);
        }
        private void GeneralSettingsForm_MouseDown(object? sender, MouseEventArgs e)
        {
            if (_activeHintOwner != null)
            {
                _settingsHintToolTip.Hide(_activeHintOwner);
                _activeHintOwner = null;
            }
        }
        private PictureBox CreateSettingsHintIcon(string name, string hintText)
        {
            PictureBox pictureBox = new PictureBox
            {
                Name = name,
                Size = new System.Drawing.Size(18, 18),
                Image = CreateSettingsHintIconBitmap(),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Cursor = Cursors.Help
            };

            _settingsHintToolTip.SetToolTip(pictureBox, hintText);

            pictureBox.Click += (sender, e) =>
            {
                _settingsHintToolTip.Hide(pictureBox);

                _activeHintOwner = pictureBox;
                _settingsHintToolTip.Show(hintText, pictureBox, pictureBox.Width + 5, 0, 8000);
            };

            return pictureBox;
        }
        private Bitmap CreateSettingsHintIconBitmap()
        {
            Bitmap bitmap = new Bitmap(18, 18);

            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Transparent);

            using SolidBrush brush = new SolidBrush(Color.FromArgb(0, 120, 215));
            graphics.FillEllipse(brush, 1, 1, 16, 16);

            using Font font = new Font("Segoe UI", 9F, FontStyle.Bold);
            Size textSize = TextRenderer.MeasureText("?", font);

            int x = (18 - textSize.Width) / 2 + 5; // ← HIER erhöhen (z.B. +2, +3)
            int y = (18 - textSize.Height) / 2 - 0;

            TextRenderer.DrawText(
                graphics,
                "?",
                font,
                new Point(x, y),
                Color.White,
                TextFormatFlags.NoPadding);

            return bitmap;
        }
        private void dataGridViewPaths_KeyDown(object? sender, KeyEventArgs e)
        {
            if (!e.Control || e.KeyCode != Keys.C)
            {
                return;
            }

            List<int> selectedRowIndexes = new List<int>();

            foreach (DataGridViewCell cell in dataGridViewPaths.SelectedCells)
            {
                if (cell.RowIndex < 0 || cell.RowIndex >= dataGridViewPaths.Rows.Count)
                {
                    continue;
                }

                if (dataGridViewPaths.Rows[cell.RowIndex].IsNewRow)
                {
                    continue;
                }

                if (!selectedRowIndexes.Contains(cell.RowIndex))
                {
                    selectedRowIndexes.Add(cell.RowIndex);
                }
            }

            foreach (DataGridViewRow row in dataGridViewPaths.SelectedRows)
            {
                if (row.Index < 0 || row.Index >= dataGridViewPaths.Rows.Count)
                {
                    continue;
                }

                if (row.IsNewRow)
                {
                    continue;
                }

                if (!selectedRowIndexes.Contains(row.Index))
                {
                    selectedRowIndexes.Add(row.Index);
                }
            }

            if (selectedRowIndexes.Count == 0)
            {
                return;
            }

            selectedRowIndexes.Sort();

            List<string> lines = new List<string>();

            foreach (int rowIndex in selectedRowIndexes)
            {
                DataGridViewRow row = dataGridViewPaths.Rows[rowIndex];

                string sourceDirectory = row.Cells["ColumnSourceDirectory"].Value?.ToString() ?? string.Empty;
                string targetDirectory = row.Cells["ColumnTargetDirectory"].Value?.ToString() ?? string.Empty;

                lines.Add(sourceDirectory + "\t" + targetDirectory);
            }

            Clipboard.SetText(string.Join(Environment.NewLine, lines));

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void ApplySettingsToUi(AppSettings settings)
        {
            checkBoxZipDestinationFiles.Checked = settings.ZipDestinationFiles;

            comboBoxDefaultVersioning.DropDownStyle = ComboBoxStyle.DropDown;
            comboBoxDefaultVersioning.Items.Clear();
            comboBoxDefaultVersioning.Items.Add("none");
            comboBoxDefaultVersioning.Items.Add("v1.0");
            comboBoxDefaultVersioning.Items.Add("1.0");
            comboBoxDefaultVersioning.Items.Add("yyyy-MM-dd");
            comboBoxDefaultVersioning.Items.Add("yyyyMMdd");
            comboBoxDefaultVersioning.Items.Add("yyyy-MM-dd-HH-mm");
            comboBoxDefaultVersioning.Items.Add("yyyyMMddHHmm");

            comboBoxDefaultVersioning.Text = string.IsNullOrWhiteSpace(settings.DefaultVersioning)
                ? "v1.0"
                : settings.DefaultVersioning;

            checkBoxAutoIncrementVersion.Checked = settings.AutoIncrementVersion;
            checkBoxMinimizeToSystray.Checked = settings.MinimizeToSystray;
            checkBoxIgnoreCopyErrors.Checked = settings.IgnoreCopyErrors;
            checkBoxDummy1.Checked = settings.AutoBackupEnabled;

            TextBox textBoxAutoBackupInterval = Controls.Find("textBoxAutoBackupInterval", true).OfType<TextBox>().First();
            textBoxAutoBackupInterval.Text = Math.Clamp(settings.AutoBackupIntervalMinutes, 1, 60).ToString();

            UpdateAutoBackupControls();

            dataGridViewPaths.Rows.Clear();

            foreach (BackupPathPair pair in settings.BackupPathPairs)
            {
                int rowIndex = dataGridViewPaths.Rows.Add(
                    pair.IsEnabled,
                    pair.SourceDirectory,
                    "Browse...",
                    string.Empty,
                    pair.TargetDirectory,
                    "Browse...");

                dataGridViewPaths.Rows[rowIndex].Tag = new List<string>(pair.ExcludedPaths);
                UpdateExclusionButtonStyle(rowIndex);
            }
        }

        private AppSettings ReadSettingsFromUi()
        {
            AppSettings settings = CloneSettings(ResultSettings);

            TextBox textBoxAutoBackupInterval = GetAutoBackupIntervalTextBox();

            settings.ZipDestinationFiles = checkBoxZipDestinationFiles.Checked;
            settings.DefaultVersioning = comboBoxDefaultVersioning.Text.Trim();
            settings.MinimizeToSystray = checkBoxMinimizeToSystray.Checked;
            settings.AutoIncrementVersion = checkBoxAutoIncrementVersion.Checked;
            settings.AutoBackupEnabled = checkBoxDummy1.Checked;
            settings.AutoBackupIntervalMinutes = int.Parse(textBoxAutoBackupInterval.Text.Trim());
            settings.IgnoreCopyErrors = checkBoxDummy1.Checked || checkBoxIgnoreCopyErrors.Checked;
            settings.BackupPathPairs = new List<BackupPathPair>();

            foreach (DataGridViewRow row in dataGridViewPaths.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                bool isEnabled = false;
                object? enabledValue = row.Cells["ColumnIsEnabled"].Value;

                if (enabledValue != null)
                {
                    bool.TryParse(enabledValue.ToString(), out isEnabled);
                }

                string sourceDirectory = row.Cells["ColumnSourceDirectory"].Value?.ToString()?.Trim() ?? string.Empty;
                string targetDirectory = row.Cells["ColumnTargetDirectory"].Value?.ToString()?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(sourceDirectory) && string.IsNullOrWhiteSpace(targetDirectory))
                {
                    continue;
                }

                List<string> excludedPaths = row.Tag is List<string> rowExcludedPaths
                    ? new List<string>(rowExcludedPaths)
                    : new List<string>();

                settings.BackupPathPairs.Add(new BackupPathPair
                {
                    IsEnabled = isEnabled,
                    SourceDirectory = sourceDirectory,
                    TargetDirectory = targetDirectory,
                    ExcludedPaths = excludedPaths
                });
            }

            return settings;
        }

        private void buttonAddRow_Click(object sender, EventArgs e)
        {
            int rowIndex = dataGridViewPaths.Rows.Add(true, string.Empty, "Browse...", string.Empty, string.Empty, "Browse...");
            dataGridViewPaths.Rows[rowIndex].Tag = new List<string>();
            UpdateExclusionButtonStyle(rowIndex);
        }

        private void buttonRemoveRow_Click(object sender, EventArgs e)
        {
            if (dataGridViewPaths.CurrentRow == null || dataGridViewPaths.CurrentRow.IsNewRow)
            {
                return;
            }

            dataGridViewPaths.Rows.Remove(dataGridViewPaths.CurrentRow);
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            TextBox textBoxAutoBackupInterval = GetAutoBackupIntervalTextBox();

            if (checkBoxDummy1.Checked &&
                (!int.TryParse(textBoxAutoBackupInterval.Text.Trim(), out int autoBackupIntervalMinutes) ||
                 autoBackupIntervalMinutes < 1 ||
                 autoBackupIntervalMinutes > 60))
            {
                MessageBox.Show("Auto-Backup (min) must be between 1 and 60.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string defaultVersioning = comboBoxDefaultVersioning.Text.Trim();

            if (!string.Equals(defaultVersioning, "none", StringComparison.OrdinalIgnoreCase) &&
                !VersionPatternHelper.IsDatePattern(defaultVersioning) &&
                !VersionPatternHelper.IsValidVersionValue(defaultVersioning))
            {
                MessageBox.Show("Default Versioning contains invalid filename characters.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            AppSettings newSettings = ReadSettingsFromUi();

            foreach (BackupPathPair pair in newSettings.BackupPathPairs)
            {
                if (string.IsNullOrWhiteSpace(pair.SourceDirectory) || string.IsNullOrWhiteSpace(pair.TargetDirectory))
                {
                    MessageBox.Show("Each row must contain source and target.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            ResultSettings = newSettings;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void dataGridViewPaths_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (dataGridViewPaths.Columns[e.ColumnIndex].Name == "ColumnSourceBrowse")
            {
                using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

                string currentValue = dataGridViewPaths.Rows[e.RowIndex].Cells["ColumnSourceDirectory"].Value?.ToString() ?? string.Empty;
                if (Directory.Exists(currentValue))
                {
                    folderBrowserDialog.SelectedPath = currentValue;
                }

                if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
                {
                    dataGridViewPaths.Rows[e.RowIndex].Cells["ColumnSourceDirectory"].Value = folderBrowserDialog.SelectedPath;
                }
            }

            if (dataGridViewPaths.Columns[e.ColumnIndex].Name == "ColumnSourceExclusions")
            {
                List<string> excludedPaths = dataGridViewPaths.Rows[e.RowIndex].Tag is List<string> rowExcludedPaths
                    ? new List<string>(rowExcludedPaths)
                    : new List<string>();

                if (ShowExclusionsDialog(excludedPaths, out List<string> resultExcludedPaths))
                {
                    dataGridViewPaths.Rows[e.RowIndex].Tag = resultExcludedPaths;
                    UpdateExclusionButtonStyle(e.RowIndex);
                }
            }

            if (dataGridViewPaths.Columns[e.ColumnIndex].Name == "ColumnTargetBrowse")
            {
                using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

                string currentValue = dataGridViewPaths.Rows[e.RowIndex].Cells["ColumnTargetDirectory"].Value?.ToString() ?? string.Empty;
                if (Directory.Exists(currentValue))
                {
                    folderBrowserDialog.SelectedPath = currentValue;
                }

                if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
                {
                    dataGridViewPaths.Rows[e.RowIndex].Cells["ColumnTargetDirectory"].Value = folderBrowserDialog.SelectedPath;
                }
            }
        }
        private void dataGridViewPaths_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            if (dataGridViewPaths.Columns[e.ColumnIndex].Name != "ColumnSourceExclusions")
            {
                return;
            }

            e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);

            bool hasExclusions = dataGridViewPaths.Rows[e.RowIndex].Tag is List<string> excludedPaths
                && excludedPaths.Any(excludedPath => !string.IsNullOrWhiteSpace(excludedPath));

            bool isSelected = (e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected;

            int centerX = e.CellBounds.Left + e.CellBounds.Width / 2;
            int centerY = e.CellBounds.Top + e.CellBounds.Height / 2;

            Point[] funnelPoints =
            {
        new Point(centerX - 8, centerY - 7),
        new Point(centerX + 8, centerY - 7),
        new Point(centerX + 3, centerY - 1),
        new Point(centerX + 1, centerY + 7),
        new Point(centerX - 1, centerY + 7),
        new Point(centerX - 3, centerY - 1)
    };

            System.Drawing.Drawing2D.SmoothingMode previousSmoothingMode = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (hasExclusions)
            {
                using SolidBrush brush = new SolidBrush(System.Drawing.Color.LimeGreen);
                using Pen pen = new Pen(System.Drawing.Color.Black, 1F);

                e.Graphics.FillPolygon(brush, funnelPoints);
                e.Graphics.DrawPolygon(pen, funnelPoints);
            }
            else
            {
                System.Drawing.Color outlineColor = isSelected
                    ? System.Drawing.Color.White
                    : System.Drawing.Color.Black;

                using Pen pen = new Pen(outlineColor, 1F);

                e.Graphics.DrawPolygon(pen, funnelPoints);
            }

            e.Graphics.SmoothingMode = previousSmoothingMode;

            e.Handled = true;
        }
        private void UpdateExclusionButtonStyle(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dataGridViewPaths.Rows.Count)
            {
                return;
            }

            if (!dataGridViewPaths.Columns.Contains("ColumnSourceExclusions"))
            {
                return;
            }

            dataGridViewPaths.InvalidateCell(dataGridViewPaths.Rows[rowIndex].Cells["ColumnSourceExclusions"]);
        }
        private bool ShowExclusionsDialog(List<string> excludedPaths, out List<string> resultExcludedPaths)
        {
            resultExcludedPaths = excludedPaths;

            using ExclusionsDialog dialog = new ExclusionsDialog(excludedPaths);

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return false;
            }

            resultExcludedPaths = dialog.ResultExcludedPaths;
            return true;
        }
        private int ParseAutoBackupIntervalSeconds(string value)
        {
            if (!TryParseAutoBackupIntervalSeconds(value, out int seconds))
            {
                throw new InvalidOperationException("Invalid Backup Timer value.");
            }

            return seconds;
        }
        private int GetAutoBackupIntervalSeconds(AppSettings settings)
        {
            if (settings.AutoBackupIntervalSeconds > 0)
            {
                return settings.AutoBackupIntervalSeconds;
            }

            return Math.Max(1, settings.AutoBackupIntervalMinutes) * 60;
        }
        private string FormatAutoBackupIntervalText(int seconds)
        {
            if (seconds % 3600 == 0)
            {
                return (seconds / 3600) + "h";
            }

            if (seconds % 60 == 0)
            {
                return (seconds / 60) + "m";
            }

            return seconds + "s";
        }
        private bool TryParseAutoBackupIntervalSeconds(string value, out int seconds)
        {
            seconds = 0;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string normalizedValue = value.Trim().ToLowerInvariant();

            if (normalizedValue.EndsWith("s"))
            {
                return int.TryParse(normalizedValue[..^1], out seconds) && seconds >= 1;
            }

            if (normalizedValue.EndsWith("m"))
            {
                if (!int.TryParse(normalizedValue[..^1], out int minutes) || minutes < 1)
                {
                    return false;
                }

                seconds = minutes * 60;
                return true;
            }

            if (normalizedValue.EndsWith("h"))
            {
                if (!int.TryParse(normalizedValue[..^1], out int hours) || hours < 1)
                {
                    return false;
                }

                seconds = hours * 3600;
                return true;
            }

            if (!int.TryParse(normalizedValue, out int defaultMinutes) || defaultMinutes < 1)
            {
                return false;
            }

            seconds = defaultMinutes * 60;
            return true;
        }

        private AppSettings CloneSettings(AppSettings settings)
        {
            AppSettings clone = new AppSettings
            {
                ZipDestinationFiles = settings.ZipDestinationFiles,
                DefaultVersioning = settings.DefaultVersioning,
                AutoIncrementVersion = settings.AutoIncrementVersion,
                MinimizeToSystray = settings.MinimizeToSystray,
                IgnoreCopyErrors = settings.IgnoreCopyErrors,
                AutoBackupEnabled = settings.AutoBackupEnabled,
                AutoBackupIntervalMinutes = settings.AutoBackupIntervalMinutes,
                AutoBackupIntervalSeconds = settings.AutoBackupIntervalSeconds,
                BackupPathPairs = new List<BackupPathPair>(),
                LastUsedVersionsByPair = new Dictionary<string, string>(settings.LastUsedVersionsByPair),
                BackupStatusesByPair = new Dictionary<string, BackupPathStatus>(settings.BackupStatusesByPair),
                MainWindowWidth = settings.MainWindowWidth,
                MainWindowHeight = settings.MainWindowHeight,
                MainWindowLeft = settings.MainWindowLeft,
                MainWindowTop = settings.MainWindowTop
            };

            foreach (BackupPathPair pair in settings.BackupPathPairs)
            {
                clone.BackupPathPairs.Add(new BackupPathPair
                {
                    IsEnabled = pair.IsEnabled,
                    SourceDirectory = pair.SourceDirectory,
                    TargetDirectory = pair.TargetDirectory,
                    ExcludedPaths = new List<string>(pair.ExcludedPaths)
                });
            }

            return clone;
        }
    }
}