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

        public static DialogResult ShowInitialDisclaimer(
            Form owner)
        {
            const string requiredText =
                "I ACCEPT";

            const string disclaimerText =
                "Important Information and Disclaimer\r\n" +
                "\r\n" +
                "Experimental Software – Please Read Before Use\r\n" +
                "\r\n" +
                "EasyVersionBackup is an independently developed, experimental software project intended for personal use, testing, and evaluation.\r\n" +
                "\r\n" +
                "The software is designed to create and manage file backups. However, like any backup solution, it may contain defects or behave unexpectedly under certain conditions.\r\n" +
                "\r\n" +
                "Important Considerations\r\n" +
                "\r\n" +
                "Although the software is developed with care, it has not undergone formal certification, independent security auditing, or comprehensive validation for business-critical or regulated environments.\r\n" +
                "\r\n" +
                "Certain functions, including backup synchronization, retention, cleanup, and file replacement, may modify or delete existing files as part of their intended operation.\r\n" +
                "\r\n" +
                "Incorrect settings, software defects, interrupted operations, or unexpected system conditions could potentially result in incomplete backups or unintended data loss.\r\n" +
                "\r\n" +
                "To minimize these risks, we recommend:\r\n" +
                "\r\n" +
                "    Verifying backup configurations before relying on them.\r\n" +
                "    Testing backup and restore operations with non-critical data.\r\n" +
                "    Maintaining an additional, independent backup of important files.\r\n" +
                "    Reviewing retention and cleanup settings carefully.\r\n" +
                "\r\n" +
                "Warranty and Liability\r\n" +
                "\r\n" +
                "This software is provided \"AS IS\", without warranties of any kind, express or implied, to the extent permitted by applicable law.\r\n" +
                "\r\n" +
                "The author does not guarantee uninterrupted operation, error-free functionality, successful backups, or the recoverability of stored data.\r\n" +
                "\r\n" +
                "To the extent permitted by applicable law, the author shall not be liable for losses or damages resulting from the use or inability to use this software, including loss of data.\r\n" +
                "\r\n" +
                "Nothing in this disclaimer excludes or limits liability where such exclusion or limitation is prohibited by applicable law.\r\n" +
                "\r\n" +
                "Acknowledgment\r\n" +
                "\r\n" +
                "By continuing, you confirm that you understand the experimental nature of this software, the potential risks associated with backup and file-management operations, and the importance of maintaining independent backups of valuable data.\r\n" +
                "\r\n" +
                "Required confirmation:\r\n" +
                "\r\n" +
                "I ACCEPT";

            using Form form = new Form();

            form.Text = "Important Information and Disclaimer";
            form.StartPosition = FormStartPosition.CenterParent;
            form.FormBorderStyle = FormBorderStyle.None;
            form.ClientSize = new Size(820, 700);
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
                Text = "Important Information and Disclaimer",
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

            TextBox textBoxDisclaimer = new TextBox
            {
                Name = "textBoxDisclaimer",
                Location = new Point(18, 52),
                Size = new Size(form.ClientSize.Width - 36, 520),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                WordWrap = true,
                Text = disclaimerText,
                BackColor = ModernTheme.ControlBackColor,
                ForeColor = ModernTheme.TextColor,
                BorderStyle = BorderStyle.FixedSingle,
                TabStop = false
            };

            Label labelRequiredPrompt = new Label
            {
                Text = "Type the following text in UPPERCASE to accept:",
                AutoSize = false,
                Location = new Point(18, 582),
                Size = new Size(form.ClientSize.Width - 36, 22),
                ForeColor = ModernTheme.TextColor,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label labelRequiredText = new Label
            {
                Text = requiredText,
                AutoSize = false,
                Location = new Point(18, 604),
                Size = new Size(form.ClientSize.Width - 36, 26),
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
                Location = new Point(18, 632),
                Size = new Size(form.ClientSize.Width - 204, 27),
                BackColor = ModernTheme.ControlBackColor,
                ForeColor = ModernTheme.TextColor,
                BorderStyle = BorderStyle.FixedSingle
            };

            Button buttonAccept = new Button
            {
                Text = "Accept",
                Size = ModernTheme.DialogButtonSize,
                Location = new Point(form.ClientSize.Width - 168, 631),
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

            buttonAccept.FlatAppearance.BorderSize = 0;
            buttonAccept.FlatAppearance.MouseOverBackColor = ModernTheme.AccentHoverColor;
            buttonAccept.FlatAppearance.MouseDownBackColor = ModernTheme.ControlBackColor;

            Button buttonDecline = new Button
            {
                Text = "Decline",
                Size = ModernTheme.DialogButtonSize,
                Location = new Point(form.ClientSize.Width - 87, 631),
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

            buttonDecline.FlatAppearance.BorderColor = ModernTheme.AccentColor;
            buttonDecline.FlatAppearance.BorderSize = 1;
            buttonDecline.FlatAppearance.MouseOverBackColor = ModernTheme.ControlHoverBackColor;
            buttonDecline.FlatAppearance.MouseDownBackColor = ModernTheme.AccentColor;

            textBoxConfirmation.TextChanged += (sender, e) =>
            {
                buttonAccept.Enabled =
                    string.Equals(
                        textBoxConfirmation.Text,
                        requiredText,
                        StringComparison.Ordinal);
            };

            form.Controls.Add(panelModernTitleBar);
            form.Controls.Add(textBoxDisclaimer);
            form.Controls.Add(labelRequiredPrompt);
            form.Controls.Add(labelRequiredText);
            form.Controls.Add(textBoxConfirmation);
            form.Controls.Add(buttonAccept);
            form.Controls.Add(buttonDecline);

            form.AcceptButton = buttonAccept;
            form.CancelButton = buttonDecline;

            DialogResult result =
                form.ShowDialog(owner);

            if (result == DialogResult.OK)
            {
                SystemLogger.WriteInitialDisclaimerConfirmation(
                    textBoxConfirmation.Text);
            }

            return result;
        }

        public static DialogResult ShowRecurringDataLossWarning(
            Form owner,
            bool retentionEnabled,
            bool sourceCleanupEnabled)
        {
            const string requiredText =
                "I understand the risk of data loss!";

            string enabledFeatures =
                retentionEnabled &&
                sourceCleanupEnabled
                    ? "Retention and Source Cleanup are enabled."
                    : retentionEnabled
                        ? "Retention is enabled."
                        : "Source Cleanup is enabled.";

            DialogResult result =
                ShowTextConfirmation(
                    owner,
                    "Data-loss warning",
                    enabledFeatures + " These experimental features can permanently, unintentionally, and irreversibly delete backup or source data. " +
                    "Deleted data cannot be restored by EasyVersionBackup. Their use is expressly discouraged. " +
                    "Verify all configured paths, rules, exclusions, and retention settings. Keep an independent, complete, readable, and recoverable backup that is not accessible to EasyVersionBackup. " +
                    "DO NOT RELY ON EASYVERSIONBACKUP AS YOUR ONLY BACKUP. NO WARRANTY OR GUARANTEE OF ANY KIND IS PROVIDED. " +
                    "THE AUTHOR ASSUMES NO RESPONSIBILITY OR LIABILITY FOR DATA LOSS, CORRUPTION, DELETION, LOSS OF BACKUPS, CONSEQUENTIAL DAMAGE, OR ANY OTHER DAMAGE RESULTING FROM USING THESE FEATURES.",
                    requiredText,
                    new Size(620, 470));

            SystemLogger.WriteRecurringDataLossWarning(
                retentionEnabled,
                sourceCleanupEnabled,
                result == DialogResult.OK,
                result == DialogResult.OK
                    ? requiredText
                    : string.Empty);

            return result;
        }

        // Semantic Anchor: Experimental feature activation is explicit, themed, and independent of Debug Mode.
        public static DialogResult ShowExperimentalDataLossConfirmation(
            Form owner,
            string featureName,
            string deletionDescription)
        {
            string message = featureName == "Retention"
                ? "Retention automatically removes older backup versions according to your configured retention policy. " +
                  "Deleted versions may no longer be recoverable. Review your retention settings and keep an independent backup of important files."
                : "Source Cleanup removes files from the original source directory after a successful backup, according to your configuration. " +
                  "Deleted source files may no longer be recoverable from their original location. Check your source and destination paths and make sure the backup is accessible.";

            message += "\n\nThis is an experimental feature. By enabling it, you acknowledge the possible consequences of deletion " +
                       "and accept responsibility for your configuration. The software is provided AS IS, without warranties, " +
                       "to the extent permitted by applicable law. Nothing excludes liability where exclusion is prohibited by law.";

            using Form form = new Form
            {
                Text = "Enable " + featureName + "?",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.None,
                ClientSize = new Size(620, 340),
                BackColor = ModernTheme.WindowBackColor,
                Font = new Font(ModernTheme.FontFamilyName, ModernTheme.DefaultFontSize),
                ShowInTaskbar = false,
                Icon = owner.Icon
            };
            ModernWindowFrame.Apply(form);

            Panel titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = ModernTheme.TitleBarHeight,
                BackColor = ModernTheme.TitleBarBackColor
            };
            Label title = new Label
            {
                Text = form.Text,
                ForeColor = ModernTheme.TextColor,
                BackColor = Color.Transparent,
                Font = new Font(ModernTheme.FontFamilyName, ModernTheme.TitleFontSize),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(ModernTheme.TitleBarTextLeft, 0, 0, 0)
            };
            titleBar.Controls.Add(title);
            form.Controls.Add(titleBar);

            Label description = new Label
            {
                Text = message,
                ForeColor = ModernTheme.TextColor,
                BackColor = Color.Transparent,
                Location = new Point(18, ModernTheme.TitleBarHeight + 18),
                Size = new Size(form.ClientSize.Width - 36, 168),
                TextAlign = ContentAlignment.TopLeft
            };
            const string requiredText = "I ACCEPT";
            Label acknowledgmentPrompt = new Label
            {
                Text = "Type I ACCEPT (UPPERCASE) to enable this feature:",
                ForeColor = ModernTheme.TextColor,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(18, form.ClientSize.Height - 119)
            };
            TextBox acknowledgment = new TextBox
            {
                BackColor = ModernTheme.ControlBackColor,
                ForeColor = ModernTheme.TextColor,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(18, form.ClientSize.Height - 91),
                Size = new Size(form.ClientSize.Width - 36, 27)
            };
            Button enable = ModernTheme.CreateDialogPrimaryButton("buttonEnable", "Enable");
            Button cancel = ModernTheme.CreateDialogSecondaryButton("buttonCancel", "Cancel", DialogResult.Cancel);
            enable.Enabled = false;
            acknowledgment.TextChanged += (_, _) =>
                enable.Enabled = string.Equals(acknowledgment.Text, requiredText, StringComparison.Ordinal);
            enable.Click += (_, _) =>
            {
                if (string.Equals(acknowledgment.Text, requiredText, StringComparison.Ordinal))
                {
                    form.DialogResult = DialogResult.OK;
                    form.Close();
                }
            };
            cancel.Click += (_, _) =>
            {
                form.DialogResult = DialogResult.Cancel;
                form.Close();
            };
            form.Controls.Add(description);
            form.Controls.Add(acknowledgmentPrompt);
            form.Controls.Add(acknowledgment);
            form.Controls.Add(enable);
            form.Controls.Add(cancel);
            ModernTheme.PositionDialogButtons(form, enable, cancel);
            DialogResult result = form.ShowDialog(owner);
            if (result == DialogResult.OK)
            {
                SystemLogger.WriteExperimentalConfirmation(featureName, requiredText);
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