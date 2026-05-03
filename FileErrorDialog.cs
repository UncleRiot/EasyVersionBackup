// Design-Rule / UI consistency:
// Keep layout, spacing, colors, sizes, and fonts aligned with ModernTheme.
// Add new shared visual values to ModernTheme instead of hardcoding local exceptions here.
// 03.05.2026 /dc

using System;
using System.Drawing;
using System.Windows.Forms;

namespace EasyVersionBackup
{
    public class FileErrorDialog : Form
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        public FileErrorDialog(string filePath, string errorMessage)
        {
            Text = "File Error";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(600, 232);
            BackColor = ModernTheme.WindowBackColor;
            Font = new Font(ModernTheme.FontFamilyName, ModernTheme.DefaultFontSize);
            DoubleBuffered = true;
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            ModernWindowFrame.Apply(this);

            InitializeModernTitleBar();

            PictureBox pictureBoxIcon = new PictureBox
            {
                Location = new Point(20, 56),
                Size = new Size(32, 32),
                Image = SystemIcons.Warning.ToBitmap(),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent
            };

            TextBox textBoxMessage = new TextBox
            {
                Location = new Point(70, 48),
                Size = new Size(500, 104),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = ModernTheme.TitleBarBackColor,
                ForeColor = ModernTheme.TextColor,
                Text = $"File:{Environment.NewLine}{filePath}{Environment.NewLine}{Environment.NewLine}Error:{Environment.NewLine}{errorMessage}"
            };

            Button buttonAbort = new Button
            {
                Text = "Abort",
                DialogResult = DialogResult.Abort,
                Size = new Size(100, 27),
                Location = new Point(146, 176)
            };

            Button buttonRetry = new Button
            {
                Text = "Retry",
                DialogResult = DialogResult.Retry,
                Size = new Size(100, 27),
                Location = new Point(256, 176)
            };

            Button buttonIgnore = new Button
            {
                Text = "Ignore",
                DialogResult = DialogResult.Ignore,
                Size = new Size(100, 27),
                Location = new Point(366, 176)
            };

            Button buttonIgnoreAll = new Button
            {
                Text = "Ignore All",
                DialogResult = DialogResult.Yes,
                Size = new Size(100, 27),
                Location = new Point(476, 176)
            };

            StyleModernButton(buttonAbort, ModernTheme.ControlBackColor, ModernTheme.TextColor, true, ModernTheme.DialogSecondaryButtonTextPadding);
            StyleModernButton(buttonRetry, ModernTheme.AccentColor, ModernTheme.DarkTextColor, false, ModernTheme.DialogPrimaryButtonTextPadding);
            StyleModernButton(buttonIgnore, ModernTheme.ControlBackColor, ModernTheme.TextColor, true, ModernTheme.DialogSecondaryButtonTextPadding);
            StyleModernButton(buttonIgnoreAll, ModernTheme.ControlBackColor, ModernTheme.TextColor, true, ModernTheme.DialogSecondaryButtonTextPadding);

            Controls.Add(pictureBoxIcon);
            Controls.Add(textBoxMessage);
            Controls.Add(buttonAbort);
            Controls.Add(buttonRetry);
            Controls.Add(buttonIgnore);
            Controls.Add(buttonIgnoreAll);

            AcceptButton = buttonRetry;
            CancelButton = buttonAbort;
        }

        private void InitializeModernTitleBar()
        {
            Panel panelModernTitleBar = new Panel
            {
                Name = "panelModernTitleBar",
                Dock = DockStyle.Top,
                Height = ModernTheme.TitleBarHeight,
                BackColor = ModernTheme.TitleBarBackColor
            };

            PictureBox pictureBoxModernTitleIcon = new PictureBox
            {
                Name = "pictureBoxModernTitleIcon",
                Location = new Point(ModernTheme.TitleBarIconLeft, ModernTheme.TitleBarIconTop),
                Size = new Size(ModernTheme.TitleBarIconSize, ModernTheme.TitleBarIconSize),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = Icon?.ToBitmap(),
                BackColor = Color.Transparent
            };

            Label labelModernTitle = new Label
            {
                Name = "labelModernTitle",
                Text = Text,
                AutoSize = false,
                Location = new Point(ModernTheme.TitleBarTextLeft, 0),
                Size = new Size(ClientSize.Width - 66, ModernTheme.TitleBarHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = ModernTheme.TextColor,
                Font = new Font(ModernTheme.FontFamilyName, ModernTheme.TitleFontSize, FontStyle.Regular),
                BackColor = Color.Transparent
            };

            Button buttonModernClose = CreateModernTitleBarButton(
                "buttonModernClose",
                new Point(ClientSize.Width - ModernTheme.TitleBarButtonSize.Width, 0));

            buttonModernClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonModernClose.MouseEnter += (sender, e) => buttonModernClose.BackColor = ModernTheme.CloseButtonHoverColor;
            buttonModernClose.MouseLeave += (sender, e) => buttonModernClose.BackColor = ModernTheme.TitleBarBackColor;
            buttonModernClose.Click += (sender, e) =>
            {
                DialogResult = DialogResult.Abort;
                Close();
            };

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
                Size = ModernTheme.TitleBarButtonSize,
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

        private void StyleModernButton(Button button, Color backColor, Color foreColor, bool showBorder, Padding textPadding)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.Cursor = Cursors.Hand;
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.Padding = textPadding;
            button.UseCompatibleTextRendering = true;
            button.UseVisualStyleBackColor = false;

            button.FlatAppearance.BorderColor = ModernTheme.AccentColor;
            button.FlatAppearance.BorderSize = showBorder ? 1 : 0;
            button.FlatAppearance.MouseOverBackColor = showBorder
                ? ModernTheme.ControlHoverBackColor
                : ModernTheme.AccentHoverColor;
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
    }

}