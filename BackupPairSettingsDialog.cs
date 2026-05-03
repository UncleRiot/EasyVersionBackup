using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EasyVersionBackup
{
    public sealed class BackupPairSettingsDialog : Form
    {
        private const int WmNclButtonDown = 0xA1;
        private const int HtCaption = 0x2;
        private const int WmNcHitTest = 0x84;
        private const int HtBottomRight = 17;

        private readonly Panel panelTitleBar;
        private readonly PictureBox pictureBoxTitleIcon;
        private readonly Label labelTitle;
        private readonly Label labelDefaultVersioning;
        private readonly Label labelRetentionHeader;
        private readonly Label labelKeepLast;
        private readonly Label labelKeepDays;
        private readonly Label labelKeepDaysUnit;
        private readonly Label labelRetentionMode;
        private readonly ComboBox comboBoxDefaultVersioning;
        private readonly CheckBox checkBoxKeepLast;
        private readonly TextBox textBoxKeepLast;
        private readonly CheckBox checkBoxKeepDays;
        private readonly TextBox textBoxKeepDays;
        private readonly ComboBox comboBoxRetentionMode;
        private readonly Button buttonOk;
        private readonly Button buttonCancel;


        public string ResultVersioning { get; private set; }
        public bool ResultIgnoreCopyErrors { get; private set; }
        public bool ResultSkipDialogs { get; private set; }
        public bool ResultRetentionKeepLastEnabled { get; private set; }
        public int ResultRetentionKeepLastCount { get; private set; }
        public bool ResultRetentionKeepDaysEnabled { get; private set; }
        public int ResultRetentionKeepDaysCount { get; private set; }
        public string ResultRetentionMode { get; private set; }

        public BackupPairSettingsDialog(Form owner, BackupPathPair pair, string defaultVersioning, bool zipRetentionAvailable)
        {
            ResultVersioning = string.IsNullOrWhiteSpace(pair.Versioning)
                ? defaultVersioning
                : pair.Versioning;

            ResultIgnoreCopyErrors = pair.IgnoreCopyErrors;
            ResultSkipDialogs = pair.SkipDialogs;
            ResultRetentionKeepLastEnabled = zipRetentionAvailable && pair.RetentionKeepLastEnabled;
            ResultRetentionKeepLastCount = pair.RetentionKeepLastCount <= 0 ? 10 : pair.RetentionKeepLastCount;
            ResultRetentionKeepDaysEnabled = zipRetentionAvailable && pair.RetentionKeepDaysEnabled;
            ResultRetentionKeepDaysCount = pair.RetentionKeepDaysCount <= 0 ? 14 : pair.RetentionKeepDaysCount;
            ResultRetentionMode = BackupHelper.NormalizeRetentionMode(pair.RetentionMode);

            Color retentionTextColor = zipRetentionAvailable
                ? ModernTheme.TextColor
                : ModernTheme.DisabledTextColor;

            Color retentionBackColor = zipRetentionAvailable
                ? ModernTheme.TitleBarBackColor
                : ModernTheme.DisabledControlBackColor;

            Icon = owner.Icon;
            Text = "Backup Pair Settings";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(420, 355);
            MinimumSize = new Size(420, 355);
            FormBorderStyle = FormBorderStyle.None;
            BackColor = ModernTheme.WindowBackColor;
            Font = new Font(ModernTheme.FontFamilyName, ModernTheme.DefaultFontSize);
            ShowInTaskbar = false;
            DoubleBuffered = true;

            ModernWindowFrame.Apply(this);

            panelTitleBar = new Panel
            {
                Name = "panelTitleBar",
                Height = ModernTheme.TitleBarHeight,
                Dock = DockStyle.Top,
                BackColor = ModernTheme.TitleBarBackColor,
                Cursor = Cursors.SizeAll
            };

            pictureBoxTitleIcon = new PictureBox
            {
                Name = "pictureBoxTitleIcon",
                Location = new Point(ModernTheme.TitleBarIconLeft, ModernTheme.TitleBarIconTop),
                Size = new Size(ModernTheme.TitleBarIconSize, ModernTheme.TitleBarIconSize),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = Icon?.ToBitmap(),
                BackColor = Color.Transparent
            };

            labelTitle = new Label
            {
                Name = "labelTitle",
                Text = "Backup Pair Settings",
                AutoSize = false,
                Location = new Point(ModernTheme.TitleBarTextLeft, 0),
                Size = new Size(ClientSize.Width - ModernTheme.TitleBarTextLeft - 12, ModernTheme.TitleBarHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = ModernTheme.TextColor,
                Font = new Font(ModernTheme.FontFamilyName, ModernTheme.TitleFontSize, FontStyle.Bold),
                BackColor = Color.Transparent,
                Cursor = Cursors.SizeAll
            };

            labelDefaultVersioning = CreateLabel("labelDefaultVersioning", "Default Versioning", 0);

            Label labelIgnoreCopyErrors = CreateLabel("labelIgnoreCopyErrors", "Ignore copy errors", 1);
            CheckBox checkBoxIgnoreCopyErrors = CreateCheckBox("checkBoxIgnoreCopyErrors", 1);
            checkBoxIgnoreCopyErrors.Checked = ResultIgnoreCopyErrors;

            PictureBox pictureBoxIgnoreCopyErrorsHint = CreateHintIcon(
                "pictureBoxIgnoreCopyErrorsHint",
                "Override general settings",
                new Point(
                    checkBoxIgnoreCopyErrors.Right + ModernTheme.SettingsHintSpacing,
                    GetDialogLabelTop(1) + 1));

            Label labelSkipDialogs = CreateLabel("labelSkipDialogs", "Skip dialogs", 2);
            CheckBox checkBoxSkipDialogs = CreateCheckBox("checkBoxSkipDialogs", 2);
            checkBoxSkipDialogs.Checked = ResultSkipDialogs;

            PictureBox pictureBoxSkipDialogsHint = CreateHintIcon(
                "pictureBoxSkipDialogsHint",
                "Override general settings",
                new Point(
                    checkBoxSkipDialogs.Right + ModernTheme.SettingsHintSpacing,
                    GetDialogLabelTop(2) + 1));

            labelRetentionHeader = new Label
            {
                Name = "labelRetentionHeader",
                Text = "Retention:",
                Location = new Point(ModernTheme.SettingsLabelLeft, GetDialogLabelTop(3)),
                Size = new Size(ModernTheme.SettingsLabelWidth, ModernTheme.SettingsLabelHeight),
                ForeColor = retentionTextColor,
                BackColor = Color.Transparent,
                Font = new Font(ModernTheme.FontFamilyName, ModernTheme.DefaultFontSize, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            labelKeepLast = CreateLabel("labelKeepLast", "Keep last backups", 4);
            labelKeepLast.ForeColor = retentionTextColor;

            labelKeepDays = CreateLabel("labelKeepDays", "Keep backups for", 5);
            labelKeepDays.ForeColor = retentionTextColor;

            labelRetentionMode = CreateLabel("labelRetentionMode", "Retention mode", 6);
            labelRetentionMode.ForeColor = retentionTextColor;

            comboBoxDefaultVersioning = new ComboBox
            {
                Name = "comboBoxDefaultVersioning",
                Location = new Point(ModernTheme.SettingsControlLeft, GetDialogRowTop(0)),
                Size = new Size(ModernTheme.SettingsComboBoxWidth, ModernTheme.SettingsControlHeight),
                DropDownStyle = ComboBoxStyle.DropDown,
                FlatStyle = FlatStyle.Flat,
                BackColor = ModernTheme.TitleBarBackColor,
                ForeColor = ModernTheme.TextColor
            };

            comboBoxDefaultVersioning.Items.Add("none");
            comboBoxDefaultVersioning.Items.Add("v1.0");
            comboBoxDefaultVersioning.Items.Add("1.0");
            comboBoxDefaultVersioning.Items.Add("yyyy-MM-dd");
            comboBoxDefaultVersioning.Items.Add("yyyyMMdd");
            comboBoxDefaultVersioning.Items.Add("yyyy-MM-dd-HH-mm");
            comboBoxDefaultVersioning.Items.Add("yyyyMMddHHmm");
            comboBoxDefaultVersioning.Text = string.IsNullOrWhiteSpace(ResultVersioning) ? "v1.0" : ResultVersioning;

            checkBoxKeepLast = CreateCheckBox("checkBoxKeepLast", 4);
            checkBoxKeepLast.Checked = ResultRetentionKeepLastEnabled;
            checkBoxKeepLast.AutoCheck = zipRetentionAvailable;
            checkBoxKeepLast.TabStop = zipRetentionAvailable;
            checkBoxKeepLast.Cursor = zipRetentionAvailable ? Cursors.Hand : Cursors.Default;
            checkBoxKeepLast.ForeColor = retentionTextColor;

            textBoxKeepLast = CreateTextBox("textBoxKeepLast", 4);
            textBoxKeepLast.Text = ResultRetentionKeepLastCount.ToString();
            textBoxKeepLast.ReadOnly = !zipRetentionAvailable;
            textBoxKeepLast.TabStop = zipRetentionAvailable;
            textBoxKeepLast.BackColor = retentionBackColor;
            textBoxKeepLast.ForeColor = retentionTextColor;

            checkBoxKeepDays = CreateCheckBox("checkBoxKeepDays", 5);
            checkBoxKeepDays.Checked = ResultRetentionKeepDaysEnabled;
            checkBoxKeepDays.AutoCheck = zipRetentionAvailable;
            checkBoxKeepDays.TabStop = zipRetentionAvailable;
            checkBoxKeepDays.Cursor = zipRetentionAvailable ? Cursors.Hand : Cursors.Default;
            checkBoxKeepDays.ForeColor = retentionTextColor;

            textBoxKeepDays = CreateTextBox("textBoxKeepDays", 5);
            textBoxKeepDays.Text = ResultRetentionKeepDaysCount.ToString();
            textBoxKeepDays.ReadOnly = !zipRetentionAvailable;
            textBoxKeepDays.TabStop = zipRetentionAvailable;
            textBoxKeepDays.BackColor = retentionBackColor;
            textBoxKeepDays.ForeColor = retentionTextColor;

            labelKeepDaysUnit = new Label
            {
                Name = "labelKeepDaysUnit",
                Text = "days",
                Location = new Point(textBoxKeepDays.Right + ModernTheme.SettingsHintSpacing, GetDialogLabelTop(5)),
                Size = new Size(50, ModernTheme.SettingsLabelHeight),
                ForeColor = retentionTextColor,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };

            comboBoxRetentionMode = new ComboBox
            {
                Name = "comboBoxRetentionMode",
                Location = new Point(ModernTheme.SettingsControlLeft, GetDialogRowTop(6)),
                Size = new Size(ModernTheme.SettingsTimerInputWidth, ModernTheme.SettingsControlHeight),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = retentionBackColor,
                ForeColor = retentionTextColor
            };

            comboBoxRetentionMode.Items.Add("OR");
            comboBoxRetentionMode.Items.Add("AND");
            comboBoxRetentionMode.SelectedIndex = ResultRetentionMode == BackupHelper.RetentionModeAll ? 1 : 0;

            if (!zipRetentionAvailable)
            {
                ModernTheme.ApplyInactiveComboBoxStyle(comboBoxRetentionMode);
            }

            buttonOk = ModernTheme.CreateDialogPrimaryButton("buttonOk", "OK");
            buttonCancel = ModernTheme.CreateDialogSecondaryButton("buttonCancel", "Cancel", DialogResult.Cancel);
            ModernTheme.PositionDialogButtons(this, buttonOk, buttonCancel, true);

            buttonOk.Click += buttonOk_Click;

            panelTitleBar.MouseDown += titleBar_MouseDown;
            labelTitle.MouseDown += titleBar_MouseDown;
            pictureBoxTitleIcon.MouseDown += titleBar_MouseDown;

            panelTitleBar.Controls.Add(pictureBoxTitleIcon);
            panelTitleBar.Controls.Add(labelTitle);

            Controls.Add(panelTitleBar);
            Controls.Add(labelDefaultVersioning);
            Controls.Add(comboBoxDefaultVersioning);
            Controls.Add(labelIgnoreCopyErrors);
            Controls.Add(checkBoxIgnoreCopyErrors);
            Controls.Add(pictureBoxIgnoreCopyErrorsHint);
            Controls.Add(labelSkipDialogs);
            Controls.Add(checkBoxSkipDialogs);
            Controls.Add(pictureBoxSkipDialogsHint);
            Controls.Add(labelRetentionHeader);
            Controls.Add(labelKeepLast);
            Controls.Add(checkBoxKeepLast);
            Controls.Add(textBoxKeepLast);
            Controls.Add(labelKeepDays);
            Controls.Add(checkBoxKeepDays);
            Controls.Add(textBoxKeepDays);
            Controls.Add(labelKeepDaysUnit);
            Controls.Add(labelRetentionMode);
            Controls.Add(comboBoxRetentionMode);
            Controls.Add(buttonOk);
            Controls.Add(buttonCancel);

            AcceptButton = buttonOk;
            CancelButton = buttonCancel;
        }

        private int GetDialogRowTop(int rowIndex)
        {
            return ModernTheme.BackupDialogContentTop + ModernTheme.SettingsRowHeight * rowIndex;
        }

        private int GetDialogLabelTop(int rowIndex)
        {
            return GetDialogRowTop(rowIndex) + ModernTheme.SettingsLabelTopOffset;
        }

        private Label CreateLabel(string name, string text, int rowIndex)
        {
            return new Label
            {
                Name = name,
                Text = text,
                Location = new Point(ModernTheme.SettingsLabelLeft, GetDialogLabelTop(rowIndex)),
                Size = new Size(ModernTheme.SettingsLabelWidth, ModernTheme.SettingsLabelHeight),
                ForeColor = ModernTheme.TextColor,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private CheckBox CreateCheckBox(string name, int rowIndex)
        {
            return ModernTheme.CreateSettingsCheckBox(
                name,
                new Point(
                    ModernTheme.SettingsControlLeft,
                    ModernTheme.SettingsCheckBoxTop(GetDialogRowTop(rowIndex))));
        }

        private TextBox CreateTextBox(string name, int rowIndex)
        {
            return new TextBox
            {
                Name = name,
                Location = new Point(ModernTheme.SettingsInputLeft, GetDialogRowTop(rowIndex)),
                Size = new Size(ModernTheme.SettingsTimerInputWidth, ModernTheme.SettingsControlHeight),
                BackColor = ModernTheme.TitleBarBackColor,
                ForeColor = ModernTheme.TextColor,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            ModernTheme.DrawResizeGrip(e.Graphics, ClientSize);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmNcHitTest)
            {
                base.WndProc(ref m);

                Point cursorPosition = PointToClient(Cursor.Position);

                if (ModernTheme.IsInResizeGripArea(ClientSize, cursorPosition))
                {
                    m.Result = HtBottomRight;
                }

                return;
            }

            base.WndProc(ref m);
        }

        private void buttonOk_Click(object? sender, EventArgs e)
        {
            string versioning = comboBoxDefaultVersioning.Text.Trim();

            if (!string.Equals(versioning, "none", StringComparison.OrdinalIgnoreCase) &&
                !VersionPatternHelper.IsDatePattern(versioning) &&
                !VersionPatternHelper.IsValidVersionValue(versioning))
            {
                ModernMessageDialog.Show(this, "Error", "Default Versioning contains invalid filename characters.");
                return;
            }

            if (checkBoxKeepLast.Checked &&
                (!int.TryParse(textBoxKeepLast.Text.Trim(), out int keepLastCount) || keepLastCount < 1))
            {
                ModernMessageDialog.Show(this, "Error", "Keep last backups must be a number greater than 0.");
                return;
            }

            if (checkBoxKeepDays.Checked &&
                (!int.TryParse(textBoxKeepDays.Text.Trim(), out int keepDaysCount) || keepDaysCount < 1))
            {
                ModernMessageDialog.Show(this, "Error", "Keep backups for days must be a number greater than 0.");
                return;
            }

            ResultVersioning = versioning;
            ResultIgnoreCopyErrors = Controls["checkBoxIgnoreCopyErrors"] is CheckBox checkBoxIgnoreCopyErrors && checkBoxIgnoreCopyErrors.Checked;
            ResultSkipDialogs = Controls["checkBoxSkipDialogs"] is CheckBox checkBoxSkipDialogs && checkBoxSkipDialogs.Checked;
            ResultRetentionKeepLastEnabled = checkBoxKeepLast.Checked;
            ResultRetentionKeepLastCount = int.TryParse(textBoxKeepLast.Text.Trim(), out int resultKeepLastCount) ? resultKeepLastCount : 10;
            ResultRetentionKeepDaysEnabled = checkBoxKeepDays.Checked;
            ResultRetentionKeepDaysCount = int.TryParse(textBoxKeepDays.Text.Trim(), out int resultKeepDaysCount) ? resultKeepDaysCount : 14;
            ResultRetentionMode = comboBoxRetentionMode.SelectedIndex == 1
                ? BackupHelper.RetentionModeAll
                : BackupHelper.RetentionModeAny;

            DialogResult = DialogResult.OK;
            Close();
        }
        private PictureBox CreateHintIcon(string name, string hintText, Point location)
        {
            ToolTip toolTip = new ToolTip();

            PictureBox pictureBox = new PictureBox
            {
                Name = name,
                Location = location,
                Size = ModernTheme.SettingsHintIconSize,
                Image = CreateHintIconBitmap(),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Cursor = Cursors.Help,
                BackColor = Color.Transparent
            };

            toolTip.SetToolTip(pictureBox, hintText);

            pictureBox.Click += (sender, e) =>
            {
                toolTip.Hide(pictureBox);
                toolTip.Show(hintText, pictureBox, pictureBox.Width + 5, 0, 8000);
            };

            return pictureBox;
        }

        private Bitmap CreateHintIconBitmap()
        {
            Bitmap bitmap = new Bitmap(ModernTheme.SettingsHintIconSize.Width, ModernTheme.SettingsHintIconSize.Height);

            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Transparent);

            using SolidBrush brush = new SolidBrush(ModernTheme.AccentColor);
            graphics.FillEllipse(brush, 1, 1, 16, 16);

            using Font font = new Font(ModernTheme.FontFamilyName, ModernTheme.SettingsHintIconFontSize, FontStyle.Regular);
            Size textSize = TextRenderer.MeasureText("?", font);

            int x = (ModernTheme.SettingsHintIconSize.Width - textSize.Width) / 2 + ModernTheme.SettingsHintIconTextOffsetX;
            int y = (ModernTheme.SettingsHintIconSize.Height - textSize.Height) / 2 + ModernTheme.SettingsHintIconTextOffsetY;

            TextRenderer.DrawText(
                graphics,
                "?",
                font,
                new Point(x, y),
                ModernTheme.DarkTextColor,
                TextFormatFlags.NoPadding);

            return bitmap;
        }

        private void titleBar_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WmNclButtonDown, HtCaption, 0);
            }
        }

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
    }
}