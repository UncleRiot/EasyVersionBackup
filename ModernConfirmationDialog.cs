using System;
using System.Drawing;
using System.Windows.Forms;

namespace EasyVersionBackup
{
    public static class ModernConfirmationDialog
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        public static DialogResult Show(Form owner, string title, string message)
        {
            using Form form = new Form();

            form.Text = title;
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.None;
            form.ClientSize = new Size(420, 170);
            form.BackColor = ModernTheme.WindowBackColor;
            form.Font = new Font(ModernTheme.FontFamilyName, ModernTheme.DefaultFontSize);
            form.ShowInTaskbar = false;
            form.Icon = owner.Icon;
            ModernWindowFrame.Apply(form);

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
                Image = owner.Icon?.ToBitmap(),
                BackColor = Color.Transparent
            };

            Label labelModernTitle = new Label
            {
                Name = "labelModernTitle",
                Text = title,
                AutoSize = false,
                Location = new Point(30, 0),
                Size = new Size(form.ClientSize.Width - 66, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = ModernTheme.TextColor,
                Font = new Font(ModernTheme.FontFamilyName, ModernTheme.TitleFontSize, FontStyle.Regular),
                BackColor = Color.Transparent
            };

            Button buttonModernClose = CreateModernTitleBarButton(form, "buttonModernClose", new Point(form.ClientSize.Width - 36, 0));
            buttonModernClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonModernClose.MouseEnter += (sender, e) => buttonModernClose.BackColor = ModernTheme.CloseButtonHoverColor;
            buttonModernClose.MouseLeave += (sender, e) => buttonModernClose.BackColor = ModernTheme.TitleBarBackColor;
            buttonModernClose.Click += (sender, e) =>
            {
                form.DialogResult = DialogResult.No;
                form.Close();
            };

            panelModernTitleBar.MouseDown += (sender, e) => MoveDialogWindow(form, e);
            pictureBoxModernTitleIcon.MouseDown += (sender, e) => MoveDialogWindow(form, e);
            labelModernTitle.MouseDown += (sender, e) => MoveDialogWindow(form, e);

            panelModernTitleBar.Controls.Add(pictureBoxModernTitleIcon);
            panelModernTitleBar.Controls.Add(labelModernTitle);
            panelModernTitleBar.Controls.Add(buttonModernClose);

            Label labelMessage = new Label
            {
                Text = message,
                AutoSize = false,
                Location = new Point(18, 52),
                Size = new Size(form.ClientSize.Width - 36, 52),
                ForeColor = ModernTheme.TextColor,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Button buttonYes = new Button
            {
                Text = "Yes",
                Size = new Size(75, 27),
                Location = new Point(form.ClientSize.Width - 168, 121),
                DialogResult = DialogResult.Yes,
                FlatStyle = FlatStyle.Flat,
                BackColor = ModernTheme.AccentColor,
                ForeColor = ModernTheme.DarkTextColor,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter, // Button-Text positioning
                Padding = new Padding(0, 0, 0, 1), // Button-Text positioning
                UseCompatibleTextRendering = true, // Button-Text positioning
                UseVisualStyleBackColor = false
            };

            buttonYes.FlatAppearance.BorderSize = 0;
            buttonYes.FlatAppearance.MouseOverBackColor = ModernTheme.AccentHoverColor;
            buttonYes.FlatAppearance.MouseDownBackColor = ModernTheme.ControlBackColor;

            Button buttonNo = new Button
            {
                Text = "No",
                Size = new Size(75, 27),
                Location = new Point(form.ClientSize.Width - 87, 121),
                DialogResult = DialogResult.No,
                FlatStyle = FlatStyle.Flat,
                BackColor = ModernTheme.ControlBackColor,
                ForeColor = ModernTheme.TextColor,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter, // Button-Text positioning
                Padding = new Padding(0, 0, 0, 2), // Button-Text positioning
                UseCompatibleTextRendering = true, // Button-Text positioning
                UseVisualStyleBackColor = false
            };

            buttonNo.FlatAppearance.BorderColor = ModernTheme.AccentColor;
            buttonNo.FlatAppearance.BorderSize = 1;
            buttonNo.FlatAppearance.MouseOverBackColor = ModernTheme.ControlHoverBackColor;
            buttonNo.FlatAppearance.MouseDownBackColor = ModernTheme.AccentColor;

            form.Controls.Add(panelModernTitleBar);
            form.Controls.Add(labelMessage);
            form.Controls.Add(buttonYes);
            form.Controls.Add(buttonNo);

            form.AcceptButton = buttonYes;
            form.CancelButton = buttonNo;

            return form.ShowDialog(owner);
        }

        private static Button CreateModernTitleBarButton(Form form, string name, Point location)
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

        private static void MoveDialogWindow(Form form, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            const int wmNclbuttondown = 0xA1;
            const int htCaption = 0x2;

            ReleaseCapture();
            SendMessage(form.Handle, wmNclbuttondown, htCaption, 0);
        }
    }
}