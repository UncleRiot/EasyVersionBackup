// Design-Rule / UI consistency:
// Keep layout, spacing, colors, sizes, and fonts aligned with ModernTheme.
// Add new shared visual values to ModernTheme instead of hardcoding local exceptions here.
// 03.05.2026 /dc


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

        public static DialogResult Show(
            Form owner,
            string title,
            string message,
            Size? clientSize = null)
        {
            using Form form = new Form();

            form.Text = title;
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.None;
            form.ClientSize =
                clientSize ??
                new Size(420, 200);
            form.BackColor = ModernTheme.WindowBackColor;
            form.Font = new Font(ModernTheme.FontFamilyName, ModernTheme.DefaultFontSize);
            form.ShowInTaskbar = false;
            form.Icon = owner.Icon;
            ModernWindowFrame.Apply(form);

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
                Image = owner.Icon?.ToBitmap(),
                BackColor = Color.Transparent
            };

            Label labelModernTitle = new Label
            {
                Name = "labelModernTitle",
                Text = title,
                AutoSize = false,
                Location = new Point(ModernTheme.TitleBarTextLeft, 0),
                Size = new Size(form.ClientSize.Width - 66, ModernTheme.TitleBarHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = ModernTheme.TextColor,
                Font = new Font(ModernTheme.FontFamilyName, ModernTheme.TitleFontSize, FontStyle.Regular),
                BackColor = Color.Transparent
            };

            Button buttonModernClose = CreateModernTitleBarButton(
                form,
                "buttonModernClose",
                new Point(form.ClientSize.Width - ModernTheme.TitleBarButtonSize.Width, 0));

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

            int buttonTop =
                form.ClientSize.Height -
                ModernTheme.DialogButtonSize.Height -
                21;

            Label labelMessage = new Label
            {
                Text = message,
                AutoSize = false,
                Location = new Point(18, 52),
                Size = new Size(
                    form.ClientSize.Width - 36,
                    buttonTop - 64),
                ForeColor = ModernTheme.TextColor,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Button buttonYes = new Button
            {
                Text = "Yes",
                Size = ModernTheme.DialogButtonSize,
                Location = new Point(
                    form.ClientSize.Width - 168,
                    buttonTop),
                DialogResult = DialogResult.Yes,
                FlatStyle = FlatStyle.Flat,
                BackColor = ModernTheme.AccentColor,
                ForeColor = ModernTheme.DarkTextColor,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter, // Button-Text positioning
                Padding = ModernTheme.DialogPrimaryButtonTextPadding, // Button-Text positioning
                UseCompatibleTextRendering = true, // Button-Text positioning
                UseVisualStyleBackColor = false
            };

            buttonYes.FlatAppearance.BorderSize = 0;
            buttonYes.FlatAppearance.MouseOverBackColor = ModernTheme.AccentHoverColor;
            buttonYes.FlatAppearance.MouseDownBackColor = ModernTheme.ControlBackColor;

            Button buttonNo = new Button
            {
                Text = "No",
                Size = ModernTheme.DialogButtonSize,
                Location = new Point(
                    form.ClientSize.Width - 87,
                    buttonTop),
                DialogResult = DialogResult.No,
                FlatStyle = FlatStyle.Flat,
                BackColor = ModernTheme.ControlBackColor,
                ForeColor = ModernTheme.TextColor,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter, // Button-Text positioning
                Padding = ModernTheme.DialogSecondaryButtonTextPadding, // Button-Text positioning
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

        public static DialogResult ShowExperimentalDataLossConfirmation(
            Form owner,
            string featureName,
            string deletionDescription)
        {
            const string requiredText =
                "I understand the risk of data loss!";

            DialogResult result =
                ShowTextConfirmation(
                    owner,
                    "Enable experimental feature?",
                    featureName + " is an experimental feature that " +
                    deletionDescription +
                    " By enabling this feature, you are explicitly warned that data can be deleted permanently, unintentionally, and irreversibly. " +
                    "Deleted data cannot be restored by EasyVersionBackup. The use of this feature is expressly discouraged. " +
                    "Before enabling it, verify that all settings and affected paths are correct and that an independent, complete, and readable backup exists. " +
                    "You are solely responsible for reviewing the configuration and affected paths. " +
                    "NO WARRANTY OR GUARANTEE OF ANY KIND IS PROVIDED. THE AUTHOR ASSUMES NO RESPONSIBILITY OR LIABILITY FOR DATA LOSS, " +
                    "CORRUPTION, DELETION, LOSS OF BACKUPS, CONSEQUENTIAL DAMAGE, OR ANY OTHER DAMAGE RESULTING FROM ENABLING OR USING THIS FEATURE.",
                    requiredText,
                    new Size(620, 420));

            if (result == DialogResult.OK)
            {
                SystemLogger.WriteExperimentalConfirmation(
                    featureName,
                    requiredText);
            }

            return result;
        }

        public static DialogResult ShowTextConfirmation(
            Form owner,
            string title,
            string message,
            string requiredText,
            Size clientSize)
        {
            using Form form = new Form();

            form.Text = title;
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.None;
            form.ClientSize = clientSize;
            form.BackColor = ModernTheme.WindowBackColor;
            form.Font = new Font(ModernTheme.FontFamilyName, ModernTheme.DefaultFontSize);
            form.ShowInTaskbar = false;
            form.Icon = owner.Icon;
            ModernWindowFrame.Apply(form);

            int contentWidth =
                form.ClientSize.Width -
                36;

            Size measuredMessageSize =
                TextRenderer.MeasureText(
                    message,
                    form.Font,
                    new Size(
                        contentWidth,
                        int.MaxValue),
                    TextFormatFlags.WordBreak |
                    TextFormatFlags.NoPadding);

            int messageTop = 52;
            int messageHeight =
                measuredMessageSize.Height +
                24;
            int promptTop =
                messageTop +
                messageHeight +
                14;
            int requiredTextTop =
                promptTop +
                24;
            int textBoxTop =
                requiredTextTop +
                28;
            int minimumClientHeight =
                textBoxTop +
                27 +
                82;

            if (form.ClientSize.Height < minimumClientHeight)
            {
                form.ClientSize =
                    new Size(
                        form.ClientSize.Width,
                        minimumClientHeight);
            }

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
                Image = owner.Icon?.ToBitmap(),
                BackColor = Color.Transparent
            };

            Label labelModernTitle = new Label
            {
                Name = "labelModernTitle",
                Text = title,
                AutoSize = false,
                Location = new Point(ModernTheme.TitleBarTextLeft, 0),
                Size = new Size(form.ClientSize.Width - 66, ModernTheme.TitleBarHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = ModernTheme.TextColor,
                Font = new Font(ModernTheme.FontFamilyName, ModernTheme.TitleFontSize, FontStyle.Regular),
                BackColor = Color.Transparent
            };

            Button buttonModernClose = CreateModernTitleBarButton(
                form,
                "buttonModernClose",
                new Point(form.ClientSize.Width - ModernTheme.TitleBarButtonSize.Width, 0));

            buttonModernClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonModernClose.MouseEnter += (sender, e) => buttonModernClose.BackColor = ModernTheme.CloseButtonHoverColor;
            buttonModernClose.MouseLeave += (sender, e) => buttonModernClose.BackColor = ModernTheme.TitleBarBackColor;
            buttonModernClose.Click += (sender, e) =>
            {
                form.DialogResult = DialogResult.Cancel;
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
                Location = new Point(18, messageTop),
                Size = new Size(contentWidth, messageHeight),
                ForeColor = ModernTheme.TextColor,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.TopLeft
            };

            Label labelRequiredPrompt = new Label
            {
                Text = "Type the following text to continue:",
                AutoSize = false,
                Location = new Point(18, promptTop),
                Size = new Size(contentWidth, 24),
                ForeColor = ModernTheme.TextColor,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label labelRequiredText = new Label
            {
                Text = requiredText,
                AutoSize = false,
                Location = new Point(18, requiredTextTop),
                Size = new Size(contentWidth, 28),
                ForeColor = ModernTheme.TextColor,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font(
                    ModernTheme.FontFamilyName,
                    ModernTheme.DefaultFontSize,
                    FontStyle.Bold)
            };

            TextBox textBoxConfirmation = new TextBox
            {
                Name = "textBoxConfirmation",
                Location = new Point(18, textBoxTop),
                Size = new Size(contentWidth, 27),
                BackColor = ModernTheme.ControlBackColor,
                ForeColor = ModernTheme.TextColor,
                BorderStyle = BorderStyle.FixedSingle
            };

            Button buttonConfirm = new Button
            {
                Text = "Enable",
                Size = ModernTheme.DialogButtonSize,
                Location = new Point(form.ClientSize.Width - 168, form.ClientSize.Height - 49),
                DialogResult = DialogResult.OK,
                FlatStyle = FlatStyle.Flat,
                BackColor = ModernTheme.AccentColor,
                ForeColor = ModernTheme.DarkTextColor,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = ModernTheme.DialogPrimaryButtonTextPadding,
                UseCompatibleTextRendering = true,
                UseVisualStyleBackColor = false,
                Enabled = false
            };

            buttonConfirm.FlatAppearance.BorderSize = 0;
            buttonConfirm.FlatAppearance.MouseOverBackColor = ModernTheme.AccentHoverColor;
            buttonConfirm.FlatAppearance.MouseDownBackColor = ModernTheme.ControlBackColor;

            Button buttonCancel = new Button
            {
                Text = "Cancel",
                Size = ModernTheme.DialogButtonSize,
                Location = new Point(form.ClientSize.Width - 87, form.ClientSize.Height - 49),
                DialogResult = DialogResult.Cancel,
                FlatStyle = FlatStyle.Flat,
                BackColor = ModernTheme.ControlBackColor,
                ForeColor = ModernTheme.TextColor,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = ModernTheme.DialogSecondaryButtonTextPadding,
                UseCompatibleTextRendering = true,
                UseVisualStyleBackColor = false
            };

            buttonCancel.FlatAppearance.BorderColor = ModernTheme.AccentColor;
            buttonCancel.FlatAppearance.BorderSize = 1;
            buttonCancel.FlatAppearance.MouseOverBackColor = ModernTheme.ControlHoverBackColor;
            buttonCancel.FlatAppearance.MouseDownBackColor = ModernTheme.AccentColor;

            textBoxConfirmation.TextChanged += (sender, e) =>
            {
                buttonConfirm.Enabled =
                    string.Equals(
                        textBoxConfirmation.Text,
                        requiredText,
                        StringComparison.Ordinal);
            };

            form.Controls.Add(panelModernTitleBar);
            form.Controls.Add(labelMessage);
            form.Controls.Add(labelRequiredPrompt);
            form.Controls.Add(labelRequiredText);
            form.Controls.Add(textBoxConfirmation);
            form.Controls.Add(buttonConfirm);
            form.Controls.Add(buttonCancel);

            form.AcceptButton = buttonConfirm;
            form.CancelButton = buttonCancel;

            return form.ShowDialog(owner);
        }

        private static Button CreateModernTitleBarButton(Form form, string name, Point location)
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