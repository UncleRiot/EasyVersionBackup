using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;



namespace EasyVersionBackup
{
    public partial class GeneralSettingsForm : Form
    {
        private readonly ToolTip _settingsHintToolTip = new ToolTip();
        private PictureBox? _activeHintOwner;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        public AppSettings ResultSettings { get; private set; }

        public GeneralSettingsForm(AppSettings settings)
        {
            InitializeComponent();

            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            FormBorderStyle = FormBorderStyle.None;
            BackColor = ModernTheme.WindowBackColor;
            Font = new Font(ModernTheme.FontFamilyName, ModernTheme.DefaultFontSize);
            DoubleBuffered = true;
            ModernWindowFrame.Apply(this);

            OffsetExistingControlsForModernTitleBar(32);
            InitializeModernTitleBar();

            InitializeExclusionColumn();
            InitializeAutoBackupControls();
            InitializeSettingsHintIcons();
            InitializeSettingsControlToolTips();
            ApplyModernSettingsStyle();

            this.MouseDown += GeneralSettingsForm_MouseDown;

            dataGridViewPaths.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;
            dataGridViewPaths.KeyDown += dataGridViewPaths_KeyDown;
            dataGridViewPaths.CellPainting += dataGridViewPaths_CellPainting;

            ResultSettings = CloneSettings(settings);
            ApplySettingsToUi(ResultSettings);
        }
        private void OffsetExistingControlsForModernTitleBar(int offsetY)
        {
            ClientSize = new Size(ClientSize.Width, ClientSize.Height + offsetY);

            foreach (Control control in Controls)
            {
                control.Top += offsetY;
            }
        }
        private void InitializeModernTitleBar()
        {
            Panel panelModernTitleBar = new Panel
            {
                Name = "panelModernTitleBar",
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = ModernTheme.TitleBarBackColor
            };

            PictureBox pictureBoxModernTitleIcon = new PictureBox
            {
                Name = "pictureBoxModernTitleIcon",
                Location = new Point(8, 8),
                Size = new Size(16, 16),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = Icon?.ToBitmap(),
                BackColor = Color.Transparent
            };

            Label labelModernTitle = new Label
            {
                Name = "labelModernTitle",
                Text = "Settings",
                AutoSize = false,
                Location = new Point(30, 0),
                Size = new Size(ClientSize.Width - 66, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = ModernTheme.TextColor,
                Font = new Font(ModernTheme.FontFamilyName, ModernTheme.TitleFontSize, FontStyle.Regular),
                BackColor = Color.Transparent
            };

            Button buttonModernClose = CreateModernTitleBarButton("buttonModernClose", "Close", new Point(ClientSize.Width - 36, 0));
            buttonModernClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonModernClose.MouseEnter += (sender, e) => buttonModernClose.BackColor = ModernTheme.CloseButtonHoverColor;
            buttonModernClose.MouseLeave += (sender, e) => buttonModernClose.BackColor = ModernTheme.TitleBarBackColor;
            buttonModernClose.Click += (sender, e) => Close();

            panelModernTitleBar.MouseDown += ModernTitleBar_MouseDown;
            pictureBoxModernTitleIcon.MouseDown += ModernTitleBar_MouseDown;
            labelModernTitle.MouseDown += ModernTitleBar_MouseDown;

            panelModernTitleBar.Controls.Add(pictureBoxModernTitleIcon);
            panelModernTitleBar.Controls.Add(labelModernTitle);
            panelModernTitleBar.Controls.Add(buttonModernClose);

            Controls.Add(panelModernTitleBar);
            panelModernTitleBar.BringToFront();
        }
        private Button CreateModernTitleBarButton(string name, string text, Point location)
        {
            Button button = new Button
            {
                Name = name,
                Text = string.Empty,
                Tag = text,
                Size = new Size(36, 32),
                Location = location,
                FlatStyle = FlatStyle.Flat,
                BackColor = ModernTheme.TitleBarBackColor,
                ForeColor = ModernTheme.TextColor,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = Padding.Empty,
                UseVisualStyleBackColor = false
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = ModernTheme.ControlBackColor;
            button.FlatAppearance.MouseDownBackColor = ModernTheme.AccentColor;

            button.Paint += (sender, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using Pen pen = new Pen(ModernTheme.TextColor, 1.4F)
                {
                    StartCap = System.Drawing.Drawing2D.LineCap.Square,
                    EndCap = System.Drawing.Drawing2D.LineCap.Square
                };

                if (button.Tag?.ToString() == "Close")
                {
                    e.Graphics.DrawLine(pen, 13, 11, 23, 21);
                    e.Graphics.DrawLine(pen, 23, 11, 13, 21);
                }
            };

            return button;
        }
        private void ApplyModernSettingsStyle()
        {
            foreach (Control control in Controls)
            {
                ApplyModernControlStyle(
                    control,
                    ModernTheme.WindowBackColor,
                    ModernTheme.TitleBarBackColor,
                    ModernTheme.ControlBackColor,
                    ModernTheme.TextColor,
                    ModernTheme.AccentColor);
            }

            StyleModernButton(buttonAddRow, ModernTheme.ControlBackColor, ModernTheme.TextColor, ModernTheme.AccentColor);
            StyleModernButton(buttonRemoveRow, ModernTheme.ControlBackColor, ModernTheme.TextColor, ModernTheme.AccentColor);
            StyleModernButton(buttonOk, ModernTheme.AccentColor, ModernTheme.DarkTextColor, ModernTheme.AccentColor);
            StyleModernButton(buttonCancel, ModernTheme.ControlBackColor, ModernTheme.TextColor, ModernTheme.AccentColor);

            dataGridViewPaths.BorderStyle = BorderStyle.None;
            dataGridViewPaths.BackgroundColor = ModernTheme.WindowBackColor;
            dataGridViewPaths.GridColor = ModernTheme.ControlBackColor;
            dataGridViewPaths.EnableHeadersVisualStyles = false;

            dataGridViewPaths.ColumnHeadersDefaultCellStyle.BackColor = ModernTheme.ControlBackColor;
            dataGridViewPaths.ColumnHeadersDefaultCellStyle.ForeColor = ModernTheme.TextColor;
            dataGridViewPaths.ColumnHeadersDefaultCellStyle.SelectionBackColor = ModernTheme.ControlBackColor;
            dataGridViewPaths.ColumnHeadersDefaultCellStyle.SelectionForeColor = ModernTheme.TextColor;
            dataGridViewPaths.ColumnHeadersDefaultCellStyle.Font = new Font(ModernTheme.FontFamilyName, ModernTheme.HeaderFontSize, FontStyle.Bold);

            dataGridViewPaths.DefaultCellStyle.BackColor = ModernTheme.TitleBarBackColor;
            dataGridViewPaths.DefaultCellStyle.ForeColor = ModernTheme.TextColor;
            dataGridViewPaths.DefaultCellStyle.SelectionBackColor = ModernTheme.AccentColor;
            dataGridViewPaths.DefaultCellStyle.SelectionForeColor = ModernTheme.DarkTextColor;

            dataGridViewPaths.AlternatingRowsDefaultCellStyle.BackColor = ModernTheme.WindowBackColor;
            dataGridViewPaths.AlternatingRowsDefaultCellStyle.ForeColor = ModernTheme.TextColor;
        }
        private void ApplyModernControlStyle(Control control, Color windowBackColor, Color panelBackColor, Color controlBackColor, Color textColor, Color accentColor)
        {
            if (control.Name == "panelModernTitleBar")
            {
                return;
            }

            if (control is Label)
            {
                control.BackColor = Color.Transparent;
                control.ForeColor = textColor;
            }
            else if (control is CheckBox checkBox)
            {
                checkBox.BackColor = windowBackColor;
                checkBox.ForeColor = textColor;
                checkBox.UseVisualStyleBackColor = false;
            }
            else if (control is TextBox textBox)
            {
                textBox.BackColor = panelBackColor;
                textBox.ForeColor = textColor;
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is ComboBox comboBox)
            {
                comboBox.BackColor = panelBackColor;
                comboBox.ForeColor = textColor;
                comboBox.FlatStyle = FlatStyle.Flat;
            }
            else if (control is DataGridView)
            {
                control.BackColor = windowBackColor;
                control.ForeColor = textColor;
            }

            foreach (Control childControl in control.Controls)
            {
                ApplyModernControlStyle(childControl, windowBackColor, panelBackColor, controlBackColor, textColor, accentColor);
            }
        }
        private void StyleModernButton(Button button, Color backColor, Color foreColor, Color accentColor)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;

            button.FlatAppearance.BorderColor = accentColor;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = ModernTheme.ControlHoverBackColor;
            button.FlatAppearance.MouseDownBackColor = ModernTheme.AccentColor;
        }
        private void ModernTitleBar_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            const int wmNclbuttondown = 0xA1;
            const int htCaption = 0x2;

            ReleaseCapture();
            SendMessage(Handle, wmNclbuttondown, htCaption, 0);
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
            if (dataGridViewPaths.Columns.Contains("ColumnSourceBrowse"))
            {
                dataGridViewPaths.Columns["ColumnSourceBrowse"].HeaderText = "";
                dataGridViewPaths.Columns["ColumnSourceBrowse"].Width = 35;
                dataGridViewPaths.Columns["ColumnSourceBrowse"].MinimumWidth = 35;
                dataGridViewPaths.Columns["ColumnSourceBrowse"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

                if (dataGridViewPaths.Columns["ColumnSourceBrowse"] is DataGridViewButtonColumn columnSourceBrowse)
                {
                    columnSourceBrowse.Text = string.Empty;
                    columnSourceBrowse.UseColumnTextForButtonValue = false;
                    columnSourceBrowse.FlatStyle = FlatStyle.Flat;
                }
            }

            if (dataGridViewPaths.Columns.Contains("ColumnTargetBrowse"))
            {
                dataGridViewPaths.Columns["ColumnTargetBrowse"].HeaderText = "";
                dataGridViewPaths.Columns["ColumnTargetBrowse"].Width = 35;
                dataGridViewPaths.Columns["ColumnTargetBrowse"].MinimumWidth = 35;
                dataGridViewPaths.Columns["ColumnTargetBrowse"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

                if (dataGridViewPaths.Columns["ColumnTargetBrowse"] is DataGridViewButtonColumn columnTargetBrowse)
                {
                    columnTargetBrowse.Text = string.Empty;
                    columnTargetBrowse.UseColumnTextForButtonValue = false;
                    columnTargetBrowse.FlatStyle = FlatStyle.Flat;
                }
            }

            if (dataGridViewPaths.Columns.Contains("ColumnSourceExclusions"))
            {
                dataGridViewPaths.Columns["ColumnSourceExclusions"].HeaderText = "";
                dataGridViewPaths.Columns["ColumnSourceExclusions"].Width = 35;
                dataGridViewPaths.Columns["ColumnSourceExclusions"].MinimumWidth = 35;
                dataGridViewPaths.Columns["ColumnSourceExclusions"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

                if (dataGridViewPaths.Columns["ColumnSourceExclusions"] is DataGridViewButtonColumn existingColumnSourceExclusions)
                {
                    existingColumnSourceExclusions.Text = string.Empty;
                    existingColumnSourceExclusions.UseColumnTextForButtonValue = false;
                    existingColumnSourceExclusions.FlatStyle = FlatStyle.Flat;
                }

                return;
            }

            DataGridViewButtonColumn columnSourceExclusions = new DataGridViewButtonColumn
            {
                HeaderText = "",
                Name = "ColumnSourceExclusions",
                Text = string.Empty,
                UseColumnTextForButtonValue = false,
                Width = 35,
                MinimumWidth = 35,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                FlatStyle = FlatStyle.Flat
            };

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

            using SolidBrush brush = new SolidBrush(ModernTheme.AccentColor);
            graphics.FillEllipse(brush, 1, 1, 16, 16);

            // hint icon font settings start
            using Font font = new Font(ModernTheme.FontFamilyName, 8F, FontStyle.Regular);
            Size textSize = TextRenderer.MeasureText("?", font);

            int x = (18 - textSize.Width) / 2 + 5;
            int y = (18 - textSize.Height) / 2;
            // hint icon font settings end

            TextRenderer.DrawText(
                graphics,
                "?",
                font,
                new Point(x, y),
                ModernTheme.DarkTextColor,
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

            if (ConfiguredSettingsPathRowContainsData(dataGridViewPaths.CurrentRow))
            {
                DialogResult result = ShowModernConfirmationDialog(
                    "Remove path",
                    "The selected path contains configured data. Do you really want to remove it?");

                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            dataGridViewPaths.Rows.Remove(dataGridViewPaths.CurrentRow);
        }
        private DialogResult ShowModernConfirmationDialog(string title, string message)
        {
            return ModernConfirmationDialog.Show(this, title, message);
        }
        private bool ConfiguredSettingsPathRowContainsData(DataGridViewRow row)
        {
            string sourceDirectory = row.Cells["ColumnSourceDirectory"].Value?.ToString()?.Trim() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(sourceDirectory))
            {
                return true;
            }

            string targetDirectory = row.Cells["ColumnTargetDirectory"].Value?.ToString()?.Trim() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(targetDirectory))
            {
                return true;
            }

            if (row.Tag is List<string> excludedPaths &&
                excludedPaths.Any(excludedPath => !string.IsNullOrWhiteSpace(excludedPath)))
            {
                return true;
            }

            return false;
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
                string currentValue = dataGridViewPaths.Rows[e.RowIndex].Cells["ColumnSourceDirectory"].Value?.ToString() ?? string.Empty;

                if (ModernFolderBrowserDialog.Show(this, "Select source directory", currentValue, out string selectedPath))
                {
                    dataGridViewPaths.Rows[e.RowIndex].Cells["ColumnSourceDirectory"].Value = selectedPath;
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
                string currentValue = dataGridViewPaths.Rows[e.RowIndex].Cells["ColumnTargetDirectory"].Value?.ToString() ?? string.Empty;

                if (ModernFolderBrowserDialog.Show(this, "Select target directory", currentValue, out string selectedPath))
                {
                    dataGridViewPaths.Rows[e.RowIndex].Cells["ColumnTargetDirectory"].Value = selectedPath;
                }
            }
        }
        private void dataGridViewPaths_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string columnName = dataGridViewPaths.Columns[e.ColumnIndex].Name;

            if (columnName != "ColumnSourceBrowse" &&
                columnName != "ColumnSourceExclusions" &&
                columnName != "ColumnTargetBrowse")
            {
                return;
            }

            e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);

            bool isSelected = (e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected;
            Color outlineColor = isSelected ? Color.FromArgb(23, 26, 33) : Color.FromArgb(199, 213, 224);

            if (columnName == "ColumnSourceBrowse" || columnName == "ColumnTargetBrowse")
            {
                DrawSettingsBrowseIcon(e.Graphics, e.CellBounds, outlineColor);
            }

            if (columnName == "ColumnSourceExclusions")
            {
                bool hasExclusions = dataGridViewPaths.Rows[e.RowIndex].Tag is List<string> excludedPaths
                    && excludedPaths.Any(excludedPath => !string.IsNullOrWhiteSpace(excludedPath));

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
                    using SolidBrush brush = new SolidBrush(Color.LimeGreen);
                    using Pen pen = new Pen(Color.FromArgb(23, 26, 33), 1F);

                    e.Graphics.FillPolygon(brush, funnelPoints);
                    e.Graphics.DrawPolygon(pen, funnelPoints);
                }
                else
                {
                    using Pen pen = new Pen(outlineColor, 1F);

                    e.Graphics.DrawPolygon(pen, funnelPoints);
                }

                e.Graphics.SmoothingMode = previousSmoothingMode;
            }

            e.Handled = true;
        }
        private void DrawSettingsBrowseIcon(Graphics graphics, Rectangle cellBounds, Color outlineColor)
        {
            int centerX = cellBounds.Left + cellBounds.Width / 2;
            int centerY = cellBounds.Top + cellBounds.Height / 2 - 1;

            System.Drawing.Drawing2D.SmoothingMode previousSmoothingMode = graphics.SmoothingMode;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using Pen pen = new Pen(outlineColor, 1.6F);

            graphics.DrawEllipse(pen, centerX - 6, centerY - 6, 9, 9);
            graphics.DrawLine(pen, centerX + 1, centerY + 1, centerX + 7, centerY + 7);

            graphics.SmoothingMode = previousSmoothingMode;
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