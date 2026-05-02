using System;
using System.Drawing;
using System.Windows.Forms;

namespace EasyVersionBackup
{
    public sealed class ModernBackupInfoDialog : Form
    {
        private readonly TextBox textBoxBackupInfo = new TextBox();
        private readonly Button buttonOk = new Button();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        public ModernBackupInfoDialog(Form owner, string backupInfoText)
        {
            Text = "Backup Info";
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.None;
            ClientSize = new Size(620, 392);
            MinimumSize = SizeFromClientSize(new Size(420, 272));
            BackColor = ModernTheme.WindowBackColor;
            Font = new Font(ModernTheme.FontFamilyName, ModernTheme.DefaultFontSize);
            Icon = owner.Icon;
            DoubleBuffered = true;
            ModernWindowFrame.Apply(this);

            InitializeModernTitleBar();
            InitializeBackupInfoTextBox(backupInfoText);
            InitializeDialogButtons();

            Controls.Add(textBoxBackupInfo);
            Controls.Add(buttonOk);

            AcceptButton = buttonOk;
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
                Text = Text,
                AutoSize = false,
                Location = new Point(30, 0),
                Size = new Size(ClientSize.Width - 66, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = ModernTheme.TextColor,
                Font = new Font(ModernTheme.FontFamilyName, ModernTheme.TitleFontSize, FontStyle.Regular),
                BackColor = Color.Transparent
            };

            Button buttonModernClose = CreateModernTitleBarButton("buttonModernClose", new Point(ClientSize.Width - 36, 0));
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

        private Button CreateModernTitleBarButton(string name, Point location)
        {
            Button button = new Button
            {
                Name = name,
                Text = string.Empty,
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

                e.Graphics.DrawLine(pen, 13, 11, 23, 21);
                e.Graphics.DrawLine(pen, 23, 11, 13, 21);
            };

            return button;
        }

        private void InitializeBackupInfoTextBox(string backupInfoText)
        {
            textBoxBackupInfo.Multiline = true;
            textBoxBackupInfo.ReadOnly = true;
            textBoxBackupInfo.ScrollBars = ScrollBars.Both;
            textBoxBackupInfo.WordWrap = false;
            textBoxBackupInfo.BorderStyle = BorderStyle.FixedSingle;
            textBoxBackupInfo.BackColor = ModernTheme.TitleBarBackColor;
            textBoxBackupInfo.ForeColor = ModernTheme.TextColor;
            textBoxBackupInfo.Location = new Point(12, 44);
            textBoxBackupInfo.Size = new Size(ClientSize.Width - 24, ClientSize.Height - 91);
            textBoxBackupInfo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBoxBackupInfo.Text = backupInfoText;
        }

        private void InitializeDialogButtons()
        {
            buttonOk.Text = "OK";
            buttonOk.DialogResult = DialogResult.OK;
            buttonOk.Size = ModernTheme.DialogButtonSize;
            buttonOk.Location = new Point(ClientSize.Width - buttonOk.Width - 12, ClientSize.Height - buttonOk.Height - 12);
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonOk.FlatStyle = FlatStyle.Flat;
            buttonOk.BackColor = ModernTheme.AccentColor;
            buttonOk.ForeColor = ModernTheme.DarkTextColor;
            buttonOk.Cursor = Cursors.Hand;
            buttonOk.TextAlign = ContentAlignment.MiddleCenter;
            buttonOk.Padding = ModernTheme.DialogPrimaryButtonTextPadding;
            buttonOk.UseCompatibleTextRendering = true;
            buttonOk.UseVisualStyleBackColor = false;
            buttonOk.FlatAppearance.BorderSize = 0;
            buttonOk.FlatAppearance.MouseOverBackColor = ModernTheme.AccentHoverColor;
            buttonOk.FlatAppearance.MouseDownBackColor = ModernTheme.ControlBackColor;
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
    }
}