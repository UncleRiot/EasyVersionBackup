using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EasyVersionBackup
{
    public sealed class SourceCleanupSettingsDialog : Form
    {
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(
            IntPtr hWnd,
            int msg,
            int wParam,
            int lParam);

        private readonly string sourceDirectory;
        private readonly CheckBox checkBoxEnabled =
            new CheckBox();
        private readonly DataGridView dataGridViewRules =
            new DataGridView();
        private readonly Button buttonAddRule =
            new Button();
        private readonly Button buttonRemoveRule =
            new Button();
        private readonly ComboBox comboBoxKeepMode =
            new ComboBox();
        private readonly NumericUpDown numericUpDownKeepLast =
            new NumericUpDown();
        private readonly DateTimePicker dateTimePickerKeepAfter =
            new DateTimePicker();
        private readonly Button buttonOk;
        private readonly Button buttonCancel;
        private readonly ToolTip toolTipRules =
            new ToolTip();

        public bool ResultEnabled { get; private set; }
        public string ResultRelativeDirectory { get; private set; }
        public List<string> ResultFileExtensions { get; private set; }
        public string ResultMode { get; private set; }
        public int ResultKeepLastCount { get; private set; }
        public string ResultKeepAfterDate { get; private set; }

        public SourceCleanupSettingsDialog(
            Form owner,
            BackupPathPair pair)
        {
            sourceDirectory =
                pair.SourceDirectory;

            ResultEnabled =
                pair.SourceCleanupEnabled;
            ResultRelativeDirectory =
                string.Empty;
            ResultFileExtensions =
                SourceCleanupService.GetNormalizedPatterns(
                    pair);
            ResultMode =
                SourceCleanupService.NormalizeMode(
                    pair.SourceCleanupMode);
            ResultKeepLastCount =
                pair.SourceCleanupKeepLastCount < 1
                    ? 10
                    : pair.SourceCleanupKeepLastCount;
            ResultKeepAfterDate =
                pair.SourceCleanupKeepAfterDate ??
                string.Empty;

            Icon = owner.Icon;
            Text = "Source Cleanup";
            StartPosition =
                FormStartPosition.CenterParent;
            ClientSize = new Size(620, 470);
            MinimumSize = new Size(620, 470);
            MaximumSize = new Size(620, 470);
            FormBorderStyle =
                FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor =
                ModernTheme.WindowBackColor;
            ForeColor =
                ModernTheme.TextColor;
            Font = new Font(
                ModernTheme.FontFamilyName,
                ModernTheme.DefaultFontSize);
            DoubleBuffered = true;

            ModernWindowFrame.Apply(this);
            InitializeModernTitleBar();
            InitializeDescription();
            InitializeEnabledCheckBox();
            InitializeToolbarButtons();
            InitializeRulesGrid();
            InitializeKeepControls();

            buttonOk =
                ModernTheme.CreateDialogPrimaryButton(
                    "buttonOk",
                    "OK");
            buttonCancel =
                ModernTheme.CreateDialogSecondaryButton(
                    "buttonCancel",
                    "Cancel",
                    DialogResult.Cancel);

            ModernTheme.PositionDialogButtons(
                this,
                buttonOk,
                buttonCancel,
                true);

            buttonOk.Click += buttonOk_Click;

            Controls.Add(buttonAddRule);
            Controls.Add(buttonRemoveRule);
            Controls.Add(dataGridViewRules);
            Controls.Add(comboBoxKeepMode);
            Controls.Add(numericUpDownKeepLast);
            Controls.Add(dateTimePickerKeepAfter);
            Controls.Add(buttonOk);
            Controls.Add(buttonCancel);

            foreach (string pattern in
                     ResultFileExtensions)
            {
                AddRuleRow(pattern);
            }

            AcceptButton = buttonOk;
            CancelButton = buttonCancel;

            checkBoxEnabled.CheckedChanged +=
                checkBoxEnabled_CheckedChanged;

            UpdateEnabledState();
            UpdateKeepModeControls();
        }

        private void InitializeModernTitleBar()
        {
            Panel panelModernTitleBar = new Panel
            {
                Name = "panelModernTitleBar",
                Dock = DockStyle.Top,
                Height = ModernTheme.TitleBarHeight,
                BackColor =
                    ModernTheme.TitleBarBackColor
            };

            PictureBox pictureBoxModernTitleIcon =
                new PictureBox
                {
                    Name = "pictureBoxModernTitleIcon",
                    Location = new Point(
                        ModernTheme.TitleBarIconLeft,
                        ModernTheme.TitleBarIconTop),
                    Size = new Size(
                        ModernTheme.TitleBarIconSize,
                        ModernTheme.TitleBarIconSize),
                    SizeMode =
                        PictureBoxSizeMode.StretchImage,
                    Image = Icon?.ToBitmap(),
                    BackColor = Color.Transparent
                };

            Label labelModernTitle = new Label
            {
                Name = "labelModernTitle",
                Text = Text,
                AutoSize = false,
                Location = new Point(
                    ModernTheme.TitleBarTextLeft,
                    0),
                Size = new Size(
                    ClientSize.Width - 66,
                    ModernTheme.TitleBarHeight),
                Anchor =
                    AnchorStyles.Top |
                    AnchorStyles.Left |
                    AnchorStyles.Right,
                TextAlign =
                    ContentAlignment.MiddleLeft,
                ForeColor =
                    ModernTheme.TextColor,
                Font = new Font(
                    ModernTheme.FontFamilyName,
                    ModernTheme.TitleFontSize,
                    FontStyle.Regular),
                BackColor = Color.Transparent
            };

            Button buttonModernClose =
                CreateModernTitleBarButton(
                    "buttonModernClose",
                    new Point(
                        ClientSize.Width -
                        ModernTheme.TitleBarButtonSize.Width,
                        0));

            buttonModernClose.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Right;
            buttonModernClose.MouseEnter +=
                (sender, e) =>
                    buttonModernClose.BackColor =
                        ModernTheme.CloseButtonHoverColor;
            buttonModernClose.MouseLeave +=
                (sender, e) =>
                    buttonModernClose.BackColor =
                        ModernTheme.TitleBarBackColor;
            buttonModernClose.Click +=
                (sender, e) => Close();

            panelModernTitleBar.MouseDown +=
                ModernTitleBar_MouseDown;
            pictureBoxModernTitleIcon.MouseDown +=
                ModernTitleBar_MouseDown;
            labelModernTitle.MouseDown +=
                ModernTitleBar_MouseDown;

            panelModernTitleBar.Controls.Add(
                pictureBoxModernTitleIcon);
            panelModernTitleBar.Controls.Add(
                labelModernTitle);
            panelModernTitleBar.Controls.Add(
                buttonModernClose);

            Controls.Add(panelModernTitleBar);
            panelModernTitleBar.BringToFront();
        }

        private Button CreateModernTitleBarButton(
            string name,
            Point location)
        {
            Button button = new Button
            {
                Name = name,
                Text = string.Empty,
                Size =
                    ModernTheme.TitleBarButtonSize,
                Location = location,
                FlatStyle = FlatStyle.Flat,
                BackColor =
                    ModernTheme.TitleBarBackColor,
                ForeColor =
                    ModernTheme.TextColor,
                Cursor = Cursors.Hand,
                TextAlign =
                    ContentAlignment.MiddleCenter,
                Padding = Padding.Empty,
                UseVisualStyleBackColor = false
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor =
                ModernTheme.ControlBackColor;
            button.FlatAppearance.MouseDownBackColor =
                ModernTheme.AccentColor;

            button.Paint += (sender, e) =>
            {
                e.Graphics.SmoothingMode =
                    System.Drawing.Drawing2D
                        .SmoothingMode.AntiAlias;

                using Pen pen = new Pen(
                    ModernTheme.TextColor,
                    1.4F)
                {
                    StartCap =
                        System.Drawing.Drawing2D
                            .LineCap.Square,
                    EndCap =
                        System.Drawing.Drawing2D
                            .LineCap.Square
                };

                e.Graphics.DrawLine(
                    pen,
                    13,
                    11,
                    23,
                    21);
                e.Graphics.DrawLine(
                    pen,
                    23,
                    11,
                    13,
                    21);
            };

            return button;
        }

        private void ModernTitleBar_MouseDown(
            object? sender,
            MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            ReleaseCapture();
            SendMessage(
                Handle,
                ModernWindowFrame.WmNclButtonDown,
                ModernWindowFrame.HtCaption,
                0);
        }

        private void InitializeDescription()
        {
            Label labelDescription = new Label
            {
                Name = "labelDescription",
                Text =
                    "Only files matching the rules below are deleted after a successful backup.",
                Location = new Point(16, 45),
                Size = new Size(588, 22),
                ForeColor =
                    ModernTheme.TextColor,
                TextAlign =
                    ContentAlignment.MiddleLeft
            };

            Controls.Add(labelDescription);
        }

        private void InitializeEnabledCheckBox()
        {
            checkBoxEnabled.Name =
                "checkBoxEnabled";
            checkBoxEnabled.Text =
                "Clean up source files after successful backup";
            checkBoxEnabled.Location =
                new Point(16, 72);
            checkBoxEnabled.Size =
                new Size(380, 24);
            checkBoxEnabled.Checked =
                ResultEnabled;
            checkBoxEnabled.ForeColor =
                Color.Red;
            checkBoxEnabled.BackColor =
                Color.Transparent;

            PictureBox pictureBoxSourceCleanupHint =
                CreateHintIcon(
                    "pictureBoxSourceCleanupHint",
                    "Warning: Source Cleanup is an experimental feature and its use is expressly discouraged. It may permanently and unintentionally delete source files from the original folder after a successful backup. Deleted source data cannot be restored by EasyVersionBackup. Enable it only if you fully understand the risk and have verified an independent, complete, and readable backup.",
                    new Point(
                        checkBoxEnabled.Left +
                        20 +
                        TextRenderer.MeasureText(
                            checkBoxEnabled.Text,
                            checkBoxEnabled.Font,
                            Size.Empty,
                            TextFormatFlags.NoPadding).Width +
                        6,
                        checkBoxEnabled.Top + 3));

            Controls.Add(checkBoxEnabled);
            Controls.Add(pictureBoxSourceCleanupHint);
            pictureBoxSourceCleanupHint.BringToFront();
        }

        private void InitializeToolbarButtons()
        {
            buttonAddRule.Name =
                "buttonAddRule";
            buttonAddRule.Text = "+";
            buttonAddRule.Size =
                ModernTheme.ToolbarButtonSize;
            buttonAddRule.Location =
                new Point(16, 105);
            buttonAddRule.FlatStyle =
                FlatStyle.Flat;
            buttonAddRule.BackColor =
                ModernTheme.ControlBackColor;
            buttonAddRule.ForeColor =
                ModernTheme.TextColor;
            buttonAddRule.Cursor =
                Cursors.Hand;
            buttonAddRule.TextAlign =
                ContentAlignment.MiddleCenter;
            buttonAddRule.Padding =
                ModernTheme.ToolbarPlusButtonTextPadding;
            buttonAddRule.UseCompatibleTextRendering =
                true;
            buttonAddRule.UseVisualStyleBackColor =
                false;
            buttonAddRule.FlatAppearance.BorderColor =
                ModernTheme.AccentColor;
            buttonAddRule.FlatAppearance.BorderSize = 1;
            buttonAddRule.FlatAppearance.MouseOverBackColor =
                ModernTheme.ControlHoverBackColor;
            buttonAddRule.FlatAppearance.MouseDownBackColor =
                ModernTheme.AccentColor;
            buttonAddRule.Click +=
                buttonAddRule_Click;

            buttonRemoveRule.Name =
                "buttonRemoveRule";
            buttonRemoveRule.Text = "−";
            buttonRemoveRule.Size =
                ModernTheme.ToolbarButtonSize;
            buttonRemoveRule.Location =
                new Point(
                    buttonAddRule.Right +
                    ModernTheme.ToolbarButtonSpacing,
                    105);
            buttonRemoveRule.FlatStyle =
                FlatStyle.Flat;
            buttonRemoveRule.BackColor =
                ModernTheme.ControlBackColor;
            buttonRemoveRule.ForeColor =
                ModernTheme.TextColor;
            buttonRemoveRule.Cursor =
                Cursors.Hand;
            buttonRemoveRule.TextAlign =
                ContentAlignment.MiddleCenter;
            buttonRemoveRule.Padding =
                ModernTheme.ToolbarMinusButtonTextPadding;
            buttonRemoveRule.UseCompatibleTextRendering =
                true;
            buttonRemoveRule.UseVisualStyleBackColor =
                false;
            buttonRemoveRule.FlatAppearance.BorderColor =
                ModernTheme.AccentColor;
            buttonRemoveRule.FlatAppearance.BorderSize = 1;
            buttonRemoveRule.FlatAppearance.MouseOverBackColor =
                ModernTheme.ControlHoverBackColor;
            buttonRemoveRule.FlatAppearance.MouseDownBackColor =
                ModernTheme.AccentColor;
            buttonRemoveRule.Click +=
                buttonRemoveRule_Click;

            toolTipRules.SetToolTip(
                buttonAddRule,
                "Add cleanup rule");
            toolTipRules.SetToolTip(
                buttonRemoveRule,
                "Remove selected cleanup rule");
        }

        private void InitializeRulesGrid()
        {
            dataGridViewRules.Name =
                "dataGridViewRules";
            dataGridViewRules.AllowUserToAddRows =
                false;
            dataGridViewRules.AllowUserToDeleteRows =
                false;
            dataGridViewRules.AllowUserToResizeRows =
                false;
            dataGridViewRules.AutoSizeRowsMode =
                DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewRules.RowHeadersVisible =
                false;
            dataGridViewRules.MultiSelect = false;
            dataGridViewRules.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            dataGridViewRules.EditMode =
                DataGridViewEditMode.EditOnKeystrokeOrF2;
            dataGridViewRules.Location =
                new Point(16, 140);
            dataGridViewRules.Size =
                new Size(588, 188);
            dataGridViewRules.BackgroundColor =
                ModernTheme.WindowBackColor;
            dataGridViewRules.BorderStyle =
                BorderStyle.None;
            dataGridViewRules.GridColor =
                ModernTheme.ControlBackColor;
            dataGridViewRules.EnableHeadersVisualStyles =
                false;
            dataGridViewRules.ColumnHeadersDefaultCellStyle.BackColor =
                ModernTheme.ControlBackColor;
            dataGridViewRules.ColumnHeadersDefaultCellStyle.ForeColor =
                ModernTheme.TextColor;
            dataGridViewRules.ColumnHeadersDefaultCellStyle.SelectionBackColor =
                ModernTheme.ControlBackColor;
            dataGridViewRules.ColumnHeadersDefaultCellStyle.SelectionForeColor =
                ModernTheme.TextColor;
            dataGridViewRules.ColumnHeadersDefaultCellStyle.Font =
                new Font(
                    ModernTheme.FontFamilyName,
                    ModernTheme.HeaderFontSize,
                    FontStyle.Bold);
            dataGridViewRules.DefaultCellStyle.BackColor =
                ModernTheme.TitleBarBackColor;
            dataGridViewRules.DefaultCellStyle.ForeColor =
                ModernTheme.TextColor;
            dataGridViewRules.DefaultCellStyle.SelectionBackColor =
                ModernTheme.AccentColor;
            dataGridViewRules.DefaultCellStyle.SelectionForeColor =
                ModernTheme.DarkTextColor;
            dataGridViewRules.DefaultCellStyle.WrapMode =
                DataGridViewTriState.True;
            dataGridViewRules.AlternatingRowsDefaultCellStyle.BackColor =
                ModernTheme.WindowBackColor;
            dataGridViewRules.AlternatingRowsDefaultCellStyle.ForeColor =
                ModernTheme.TextColor;
            dataGridViewRules.CellValidating +=
                dataGridViewRules_CellValidating;
            dataGridViewRules.CellEndEdit +=
                dataGridViewRules_CellEndEdit;

            DataGridViewTextBoxColumn columnRule =
                new DataGridViewTextBoxColumn
                {
                    HeaderText = "Files to clean up",
                    Name = "ColumnRule",
                    Width = 180,
                    ToolTipText =
                        "Enter a file type such as .sav, or a source-relative rule such as Saved\\SaveGames\\.sav."
                };

            DataGridViewTextBoxColumn columnAffectedFiles =
                new DataGridViewTextBoxColumn
                {
                    HeaderText = "Files that can be deleted",
                    Name = "ColumnAffectedFiles",
                    AutoSizeMode =
                        DataGridViewAutoSizeColumnMode.Fill,
                    ReadOnly = true,
                    DefaultCellStyle =
                    {
                        WrapMode =
                            DataGridViewTriState.True
                    }
                };

            dataGridViewRules.Columns.Add(
                columnRule);
            dataGridViewRules.Columns.Add(
                columnAffectedFiles);

            Label labelExamples = new Label
            {
                Name = "labelExamples",
                Text =
                    "Examples: .sav = source folder only    |    Saved\\SaveGames\\.sav = this subfolder only",
                Location = new Point(16, 332),
                Size = new Size(588, 22),
                ForeColor =
                    ModernTheme.DisabledTextColor,
                TextAlign =
                    ContentAlignment.MiddleLeft
            };

            Controls.Add(labelExamples);
        }

        private void InitializeKeepControls()
        {
            Label labelKeep = new Label
            {
                Name = "labelKeep",
                Text = "Keep:",
                Location = new Point(16, 366),
                Size = new Size(80, 26),
                ForeColor =
                    ModernTheme.TextColor,
                TextAlign =
                    ContentAlignment.MiddleLeft
            };

            comboBoxKeepMode.Name =
                "comboBoxKeepMode";
            comboBoxKeepMode.Location =
                new Point(96, 366);
            comboBoxKeepMode.Size =
                new Size(180, 26);
            comboBoxKeepMode.DropDownStyle =
                ComboBoxStyle.DropDownList;
            comboBoxKeepMode.FlatStyle =
                FlatStyle.Flat;
            comboBoxKeepMode.BackColor =
                ModernTheme.TitleBarBackColor;
            comboBoxKeepMode.ForeColor =
                ModernTheme.TextColor;
            comboBoxKeepMode.Items.Add(
                "Newest files");
            comboBoxKeepMode.Items.Add(
                "Files before");
            comboBoxKeepMode.SelectedIndex =
                ResultMode ==
                SourceCleanupService.ModeKeepAfterDate
                    ? 1
                    : 0;
            comboBoxKeepMode.SelectedIndexChanged +=
                comboBoxKeepMode_SelectedIndexChanged;

            toolTipRules.SetToolTip(
                comboBoxKeepMode,
                "Files modified before the selected date are deleted. Files modified on or after the selected date are kept.");

            numericUpDownKeepLast.Name =
                "numericUpDownKeepLast";
            numericUpDownKeepLast.Location =
                new Point(286, 366);
            numericUpDownKeepLast.Size =
                new Size(120, 26);
            numericUpDownKeepLast.Minimum = 1;
            numericUpDownKeepLast.Maximum = 100000;
            numericUpDownKeepLast.Value =
                ResultKeepLastCount;
            numericUpDownKeepLast.BackColor =
                ModernTheme.TitleBarBackColor;
            numericUpDownKeepLast.ForeColor =
                ModernTheme.TextColor;

            dateTimePickerKeepAfter.Name =
                "dateTimePickerKeepAfter";
            dateTimePickerKeepAfter.Location =
                new Point(286, 366);
            dateTimePickerKeepAfter.Size =
                new Size(120, 26);
            dateTimePickerKeepAfter.Format =
                DateTimePickerFormat.Custom;
            dateTimePickerKeepAfter.CustomFormat =
                "yyyy-MM-dd";
            dateTimePickerKeepAfter.Value =
                SourceCleanupService.TryParseKeepAfterDate(
                    ResultKeepAfterDate,
                    out DateTime keepAfterDate)
                    ? keepAfterDate
                    : DateTime.Today;

            Controls.Add(labelKeep);
        }

        private void AddRuleRow(
            string pattern)
        {
            int rowIndex =
                dataGridViewRules.Rows.Add(
                    pattern,
                    string.Empty);

            UpdateAffectedPath(
                dataGridViewRules.Rows[rowIndex]);
        }

        private void checkBoxEnabled_CheckedChanged(
            object? sender,
            EventArgs e)
        {
            if (checkBoxEnabled.Checked)
            {
                DialogResult confirmationResult =
                    ModernConfirmationDialog.ShowExperimentalDataLossConfirmation(
                        this,
                        "Source Cleanup",
                        "permanently deletes source files from the original source folder after a successful backup.");

                if (confirmationResult != DialogResult.OK)
                {
                    checkBoxEnabled.Checked = false;
                    return;
                }
            }

            UpdateEnabledState();
        }

        private void comboBoxKeepMode_SelectedIndexChanged(
            object? sender,
            EventArgs e)
        {
            UpdateKeepModeControls();
        }

        private void UpdateEnabledState()
        {
            bool enabled =
                checkBoxEnabled.Checked;

            buttonAddRule.Enabled = enabled;
            buttonRemoveRule.Enabled = enabled;
            dataGridViewRules.Enabled = enabled;
            comboBoxKeepMode.Enabled = enabled;
            numericUpDownKeepLast.Enabled = enabled;
            dateTimePickerKeepAfter.Enabled = enabled;
        }

        private void UpdateKeepModeControls()
        {
            bool keepAfterDate =
                comboBoxKeepMode.SelectedIndex == 1;

            numericUpDownKeepLast.Visible =
                !keepAfterDate;
            dateTimePickerKeepAfter.Visible =
                keepAfterDate;
        }

        private void buttonAddRule_Click(
            object? sender,
            EventArgs e)
        {
            int rowIndex =
                dataGridViewRules.Rows.Add(
                    string.Empty,
                    string.Empty);

            dataGridViewRules.ClearSelection();
            dataGridViewRules.Rows[rowIndex].Selected =
                true;
            dataGridViewRules.CurrentCell =
                dataGridViewRules.Rows[rowIndex]
                    .Cells["ColumnRule"];
            dataGridViewRules.BeginEdit(true);
        }

        private void buttonRemoveRule_Click(
            object? sender,
            EventArgs e)
        {
            if (dataGridViewRules.CurrentRow == null ||
                dataGridViewRules.CurrentRow.IsNewRow)
            {
                return;
            }

            string rule =
                dataGridViewRules.CurrentRow
                    .Cells["ColumnRule"]
                    .Value?.ToString()?.Trim() ??
                string.Empty;

            if (!string.IsNullOrWhiteSpace(rule))
            {
                DialogResult result =
                    ModernConfirmationDialog.Show(
                        this,
                        "Remove cleanup rule",
                        "Remove the selected cleanup rule?");

                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            dataGridViewRules.Rows.Remove(
                dataGridViewRules.CurrentRow);
        }

        private void dataGridViewRules_CellValidating(
            object? sender,
            DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex < 0 ||
                dataGridViewRules.Columns[e.ColumnIndex]
                    .Name != "ColumnRule")
            {
                return;
            }

            string value =
                e.FormattedValue?.ToString()?.Trim() ??
                string.Empty;

            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (!SourceCleanupService.TryGetAffectedPath(
                    sourceDirectory,
                    value,
                    out _,
                    out string errorMessage))
            {
                e.Cancel = true;

                ModernMessageDialog.Show(
                    this,
                    "Invalid cleanup rule",
                    errorMessage);
            }
        }

        private void dataGridViewRules_CellEndEdit(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            UpdateAffectedPath(
                dataGridViewRules.Rows[e.RowIndex]);
        }

        private void UpdateAffectedPath(
            DataGridViewRow row)
        {
            string rule =
                row.Cells["ColumnRule"]
                    .Value?.ToString()?.Trim() ??
                string.Empty;

            if (SourceCleanupService.TryGetAffectedPath(
                    sourceDirectory,
                    rule,
                    out string affectedPath,
                    out _))
            {
                string directoryPath =
                    Path.GetDirectoryName(affectedPath) ??
                    affectedPath;
                string filePattern =
                    Path.GetFileName(affectedPath);

                row.Cells["ColumnAffectedFiles"].Value =
                    directoryPath +
                    Environment.NewLine +
                    filePattern;
            }
            else
            {
                row.Cells["ColumnAffectedFiles"].Value =
                    string.Empty;
            }
        }

        private void buttonOk_Click(
            object? sender,
            EventArgs e)
        {
            if (!Validate())
            {
                return;
            }

            List<string> patterns =
                dataGridViewRules.Rows
                    .Cast<DataGridViewRow>()
                    .Where(row => !row.IsNewRow)
                    .Select(row =>
                        row.Cells["ColumnRule"]
                            .Value?.ToString()?.Trim() ??
                        string.Empty)
                    .Where(value =>
                        !string.IsNullOrWhiteSpace(value))
                    .ToList();

            BackupPathPair validationPair =
                new BackupPathPair
                {
                    SourceDirectory =
                        sourceDirectory,
                    SourceCleanupEnabled =
                        checkBoxEnabled.Checked,
                    SourceCleanupRelativeDirectory =
                        string.Empty,
                    SourceCleanupFileExtensions =
                        patterns,
                    SourceCleanupMode =
                        comboBoxKeepMode.SelectedIndex == 1
                            ? SourceCleanupService.ModeKeepAfterDate
                            : SourceCleanupService.ModeKeepLast,
                    SourceCleanupKeepLastCount =
                        Decimal.ToInt32(
                            numericUpDownKeepLast.Value),
                    SourceCleanupKeepAfterDate =
                        dateTimePickerKeepAfter.Value
                            .ToString(
                                "yyyy-MM-dd",
                                CultureInfo.InvariantCulture)
                };

            if (!SourceCleanupService.TryValidateSettings(
                    validationPair,
                    out _,
                    out string errorMessage))
            {
                ModernMessageDialog.Show(
                    this,
                    "Error",
                    errorMessage);
                return;
            }

            ResultEnabled =
                validationPair.SourceCleanupEnabled;
            ResultRelativeDirectory =
                string.Empty;
            ResultFileExtensions =
                SourceCleanupService.GetNormalizedPatterns(
                    validationPair);
            ResultMode =
                validationPair.SourceCleanupMode;
            ResultKeepLastCount =
                validationPair.SourceCleanupKeepLastCount;
            ResultKeepAfterDate =
                validationPair.SourceCleanupKeepAfterDate;

            DialogResult = DialogResult.OK;
            Close();
        }

        private PictureBox CreateHintIcon(
            string name,
            string hintText,
            Point location)
        {
            PictureBox pictureBox = new PictureBox
            {
                Name = name,
                Location = location,
                Size = new Size(18, 18),
                Image = CreateHintIconBitmap(),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Cursor = Cursors.Help,
                BackColor = Color.Transparent
            };

            string wrappedHintText =
                WrapHintText(
                    hintText,
                    300);

            toolTipRules.SetToolTip(
                pictureBox,
                wrappedHintText);

            pictureBox.Click += (sender, e) =>
            {
                toolTipRules.Hide(
                    pictureBox);
                toolTipRules.Show(
                    wrappedHintText,
                    pictureBox,
                    pictureBox.Width + 5,
                    0,
                    8000);
            };

            return pictureBox;
        }

        private string WrapHintText(
            string hintText,
            int maximumWidth)
        {
            string[] words =
                hintText.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);

            System.Text.StringBuilder result =
                new System.Text.StringBuilder();
            System.Text.StringBuilder currentLine =
                new System.Text.StringBuilder();

            foreach (string word in words)
            {
                string candidate =
                    currentLine.Length == 0
                        ? word
                        : currentLine + " " + word;

                int candidateWidth =
                    TextRenderer.MeasureText(
                        candidate,
                        Font,
                        Size.Empty,
                        TextFormatFlags.NoPadding).Width;

                if (candidateWidth > maximumWidth &&
                    currentLine.Length > 0)
                {
                    if (result.Length > 0)
                    {
                        result.AppendLine();
                    }

                    result.Append(
                        currentLine);
                    currentLine.Clear();
                    currentLine.Append(
                        word);
                }
                else
                {
                    if (currentLine.Length > 0)
                    {
                        currentLine.Append(
                            ' ');
                    }

                    currentLine.Append(
                        word);
                }
            }

            if (currentLine.Length > 0)
            {
                if (result.Length > 0)
                {
                    result.AppendLine();
                }

                result.Append(
                    currentLine);
            }

            return result.ToString();
        }

        private Bitmap CreateHintIconBitmap()
        {
            Bitmap bitmap =
                new Bitmap(
                    18,
                    18);

            using Graphics graphics =
                Graphics.FromImage(
                    bitmap);
            graphics.Clear(
                Color.Transparent);

            using SolidBrush brush =
                new SolidBrush(
                    ModernTheme.AccentColor);
            graphics.FillEllipse(
                brush,
                1,
                1,
                16,
                16);

            using Font font =
                new Font(
                    ModernTheme.FontFamilyName,
                    ModernTheme.SettingsHintIconFontSize,
                    FontStyle.Regular);
            Size textSize =
                TextRenderer.MeasureText(
                    "?",
                    font);

            int x =
                (18 - textSize.Width) / 2 +
                ModernTheme.SettingsHintIconTextOffsetX;
            int y =
                (18 - textSize.Height) / 2 +
                ModernTheme.SettingsHintIconTextOffsetY;

            TextRenderer.DrawText(
                graphics,
                "?",
                font,
                new Point(
                    x,
                    y),
                ModernTheme.DarkTextColor,
                TextFormatFlags.NoPadding);

            return bitmap;
        }

        protected override void OnFormClosing(
            FormClosingEventArgs e)
        {
            toolTipRules.Active = false;
            toolTipRules.RemoveAll();

            base.OnFormClosing(e);
        }
    }
}
