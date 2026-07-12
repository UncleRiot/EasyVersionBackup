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
                "I accept the risk of data loss.";

            const string disclaimerText =
                "IMPORTANT DATA-LOSS WARNING AND DISCLAIMER\r\n" +
                "Experimental Software\r\n\r\n" +
                "This software is an experimental hobby project and proof of concept.\r\n\r\n" +
                "It is not a professional, certified, audited, supported, or production-ready backup solution. It has not been validated for business-critical, commercial, regulated, or other professional use.\r\n\r\n" +
                "Use of this software for production systems, business purposes, or the protection of important, valuable, confidential, regulated, or irreplaceable data is strongly discouraged.\r\n\r\n" +
                "Risk of Permanent Data Loss\r\n\r\n" +
                "This software includes functions that may delete, purge, overwrite, move, replace, or otherwise modify files, directories, backups, snapshots, and other data at source and destination locations.\r\n\r\n" +
                "Retention, cleanup, synchronization, and purging operations may result in partial or complete, permanent, and irreversible data loss.\r\n\r\n" +
                "Data loss may occur due to, among other things:\r\n\r\n" +
                "    software defects or programming errors;\r\n" +
                "    design flaws or undocumented behavior;\r\n" +
                "    incorrect configuration or operation;\r\n" +
                "    incorrect source or destination paths;\r\n" +
                "    retention, cleanup, filtering, or exclusion rules;\r\n" +
                "    unexpected interactions with operating systems, storage devices, network services, filesystems, permissions, or third-party software;\r\n" +
                "    hardware, storage, network, or service failures;\r\n" +
                "    interrupted, incomplete, corrupted, or unverified backup operations.\r\n\r\n" +
                "Warnings, previews, dry-run functions, confirmation dialogs, logs, and other protective measures may themselves contain defects or may fail to identify every destructive operation.\r\n\r\n" +
                "DO NOT RELY ON THIS SOFTWARE AS YOUR ONLY BACKUP.\r\n\r\n" +
                "Before using it, create independent and verified backups that are not accessible to this software. Test all functionality, including retention, purging, deletion, backup verification, and restoration, in an isolated non-production environment.\r\n\r\n" +
                "The user is solely responsible for reviewing all settings, paths, rules, filters, exclusions, retention policies, deletion options, backup results, and restore procedures.\r\n\r\n" +
                "No Warranty\r\n\r\n" +
                "TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THIS SOFTWARE IS PROVIDED “AS IS” AND “AS AVAILABLE”, WITH ALL FAULTS AND WITHOUT WARRANTY OF ANY KIND, WHETHER EXPRESS, IMPLIED, STATUTORY, OR OTHERWISE.\r\n\r\n" +
                "ALL WARRANTIES ARE DISCLAIMED, INCLUDING, WITHOUT LIMITATION, WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, TITLE, NON-INFRINGEMENT, ACCURACY, RELIABILITY, AVAILABILITY, SECURITY, COMPATIBILITY, DATA INTEGRITY, ERROR-FREE OPERATION, AND FITNESS FOR BACKUP, RESTORATION, RETENTION, ARCHIVING, OR DISASTER-RECOVERY PURPOSES.\r\n\r\n" +
                "NO REPRESENTATION OR WARRANTY IS MADE THAT:\r\n\r\n" +
                "    backups will be complete, correct, recoverable, or available;\r\n" +
                "    restoration will succeed;\r\n" +
                "    retention or deletion rules will operate as expected;\r\n" +
                "    data will not be deleted, overwritten, corrupted, lost, or disclosed;\r\n" +
                "    defects will be identified, corrected, or supported;\r\n" +
                "    the software is suitable for any particular environment or purpose.\r\n\r\n" +
                "Limitation of Liability\r\n\r\n" +
                "TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THE AUTHOR, MAINTAINERS, CONTRIBUTORS, COPYRIGHT HOLDERS, AND DISTRIBUTORS SHALL NOT BE LIABLE FOR ANY CLAIM, LOSS, DAMAGE, COST, OR OTHER LIABILITY ARISING FROM OR RELATED TO THIS SOFTWARE OR ITS USE, MISUSE, CONFIGURATION, MODIFICATION, DISTRIBUTION, OR INABILITY TO USE.\r\n\r\n" +
                "THIS EXCLUSION INCLUDES, WITHOUT LIMITATION:\r\n\r\n" +
                "    deletion, loss, corruption, overwriting, alteration, disclosure, or unavailability of data;\r\n" +
                "    failed, incomplete, unusable, or unrecoverable backups;\r\n" +
                "    failed restoration or disaster recovery;\r\n" +
                "    loss of revenue, profits, contracts, business, goodwill, or expected savings;\r\n" +
                "    business interruption or system downtime;\r\n" +
                "    costs of data recovery, restoration, replacement systems, services, or substitute software;\r\n" +
                "    direct, indirect, incidental, special, exemplary, punitive, or consequential damages.\r\n\r\n" +
                "THIS LIMITATION APPLIES REGARDLESS OF THE LEGAL THEORY OF LIABILITY, INCLUDING CONTRACT, TORT, NEGLIGENCE, STRICT LIABILITY, OR OTHERWISE, AND EVEN IF THE POSSIBILITY OF SUCH DAMAGE WAS KNOWN OR REASONABLY FORESEEABLE.\r\n\r\n" +
                "Nothing in this disclaimer excludes or limits liability that cannot legally be excluded or limited under applicable law.\r\n\r\n" +
                "Assumption of Risk\r\n\r\n" +
                "USE OF THIS SOFTWARE IS ENTIRELY AT YOUR OWN RISK.\r\n\r\n" +
                "By installing, starting, configuring, or using the software, you acknowledge that:\r\n\r\n" +
                "    you have read and understood this warning;\r\n" +
                "    the software can permanently delete or damage source and destination data;\r\n" +
                "    no backup or restoration result is guaranteed;\r\n" +
                "    you have created independent, verified, and recoverable backups;\r\n" +
                "    you have tested the software in a safe, isolated environment;\r\n" +
                "    you accept full responsibility for deciding whether and how to use it;\r\n" +
                "    you assume all risks and consequences arising from its use.\r\n\r\n" +
                "If you do not understand or accept these risks, do not install, start, configure, or use the software.";

            using Form form = new Form();

            form.Text = "Important data-loss warning";
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
                Text = "Important data-loss warning",
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
                Text = "Type the following text to accept:",
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