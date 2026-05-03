// Design-Rule / UI standards:
// This file defines the shared visual values for the application.
// Forms should reuse these values to keep layout, spacing, colors, sizes, and fonts consistent.
// 03.05.2026 /dc


using System.Drawing;

namespace EasyVersionBackup
{
    public static class ModernTheme
    {
        // window color settings start
        public static readonly Color WindowBackColor = Color.FromArgb(27, 40, 56);
        public static readonly Color TitleBarBackColor = Color.FromArgb(23, 26, 33);
        public static readonly Color ControlBackColor = Color.FromArgb(42, 71, 94);
        public static readonly Color ControlHoverBackColor = Color.FromArgb(55, 90, 120);
        public static readonly Color AccentColor = Color.FromArgb(102, 192, 244);
        public static readonly Color AccentHoverColor = Color.FromArgb(143, 212, 255);
        public static readonly Color TextColor = Color.FromArgb(199, 213, 224);
        public static readonly Color DarkTextColor = Color.FromArgb(23, 26, 33);
        public static readonly Color CloseButtonHoverColor = Color.FromArgb(196, 43, 28);
        // window color settings end

        // status color settings start
        public static readonly Color BackupInfoDefaultColor = Color.FromArgb(0, 120, 215);
        public static readonly Color BackupInfoOkColor = Color.FromArgb(0, 160, 80);
        public static readonly Color BackupInfoWarningColor = Color.FromArgb(230, 180, 0);
        public static readonly Color BackupInfoErrorColor = Color.FromArgb(200, 0, 0);
        public static readonly Color BackupInfoTextColor = Color.White;
        public static readonly Color ActiveExclusionColor = Color.LimeGreen;
        // public static readonly Color DisabledTextColor = WindowBorderColor;
        public static readonly Color DisabledTextColor = Color.FromArgb(92, 104, 112);
        public static readonly Color DisabledControlBackColor = Color.FromArgb(32, 43, 56);
        // status color settings end

        // window border settings start
        public static readonly Color WindowBorderColor = Color.FromArgb(92, 104, 112);
        public const float WindowBorderWidth = 1F;
        // window border settings end

        // window font settings start
        public const string FontFamilyName = "Segoe UI";
        public const float DefaultFontSize = 9F;
        public const float TitleFontSize = 9F;
        public const float HeaderFontSize = 9F;
        // window font settings end

        // title bar settings start
        public const int TitleBarHeight = 32;
        public static readonly Size TitleBarButtonSize = new Size(36, 32);
        public const int TitleBarIconSize = 16;
        public const int TitleBarIconLeft = 8;
        public const int TitleBarIconTop = 8;
        public const int TitleBarTextLeft = 30;
        // title bar settings end

        // settings layout settings start
        public const int SettingsLabelLeft = 26;
        public const int SettingsLabelWidth = 145;
        public const int SettingsControlLeft = 177;
        public const int SettingsInputLeft = 209;
        public const int SettingsContentTop = 20;
        public const int SettingsRowHeight = 32;
        public const int SettingsLabelTopOffset = 3;
        public const int SettingsControlHeight = 23;
        public const int SettingsLabelHeight = 20;
        public const int SettingsComboBoxWidth = 180;
        public const int SettingsTimerInputWidth = 70;
        public const int SettingsCheckBoxWidth = 18;
        public const int SettingsCheckBoxTopOffset = -3;
        public const int SettingsHintSpacing = 6;
        public static readonly Size SettingsTabButtonSize = new Size(90, 30);
        public static readonly Size SettingsHintIconSize = new Size(18, 18);
        public const int SettingsHintIconInnerPadding = 1;
        public const float SettingsHintIconFontSize = 8F;
        public const int SettingsHintIconTextOffsetX = 5;
        public const int SettingsHintIconTextOffsetY = 0;

        public static int SettingsRowTop(int rowIndex)
        {
            return SettingsContentTop + SettingsRowHeight * rowIndex;
        }

        public static int SettingsLabelTop(int rowIndex)
        {
            return SettingsRowTop(rowIndex) + SettingsLabelTopOffset;
        }


        public static int SettingsCheckBoxTop(int rowTop)
        {
            return rowTop + SettingsCheckBoxTopOffset;
        }

        public static System.Windows.Forms.CheckBox CreateSettingsCheckBox(string name, Point location)
        {
            return new System.Windows.Forms.CheckBox
            {
                Name = name,
                Text = string.Empty,
                Location = location,
                Size = new Size(SettingsCheckBoxWidth, SettingsRowHeight),
                AutoSize = false,
                CheckAlign = System.Drawing.ContentAlignment.MiddleLeft,
                BackColor = WindowBackColor,
                ForeColor = TextColor,
                UseVisualStyleBackColor = false
            };
        }

        // settings layout settings end

        // message dialog layout settings start
        public static readonly Size MessageDialogDefaultSize = new Size(460, 170);
        public static readonly Size MessageDialogMinimumSize = new Size(460, 170);
        public const int MessageDialogContentLeft = 18;
        public const int MessageDialogContentTop = 50;
        public const int MessageDialogContentRightMargin = 18;
        public const int MessageDialogButtonBottomMargin = 18;
        public const int MessageDialogButtonRightMargin = 18;
        public const int MessageDialogButtonAreaHeight = 58;
        // message dialog layout settings end

        // backup dialog layout settings start
        public static readonly Size BackupDialogDefaultSize = new Size(420, 190);
        public static readonly Size BackupDialogMinimumSize = new Size(420, 190);
        public static readonly Size BackupDialogDestinationConflictMinimumSize = BackupDialogDefaultSize;
        public const int BackupDialogContentLeft = 22;
        public const int BackupDialogContentTop = 58;
        public const int BackupDialogContentRightMargin = 22;
        public const int BackupDialogBottomMargin = 20;
        public const int BackupDialogPathLabelHeight = 20;
        public const int BackupDialogMessageHeight = 82;
        public const int BackupDialogControlSpacing = 10;
        public const int BackupDialogComboBoxWidth = 180;
        public const int BackupDialogResizeGripSize = 16;
        // backup dialog layout settings end

        // button text positioning settings start
        public static readonly System.Windows.Forms.Padding DefaultButtonTextPadding = System.Windows.Forms.Padding.Empty;
        public static readonly System.Windows.Forms.Padding DialogButtonTextPadding = System.Windows.Forms.Padding.Empty;
        public static readonly System.Windows.Forms.Padding DialogPrimaryButtonTextPadding = new System.Windows.Forms.Padding(0, 0, 0, 1);
        public static readonly System.Windows.Forms.Padding DialogSecondaryButtonTextPadding = new System.Windows.Forms.Padding(0, 0, 0, 2);
        public static readonly System.Windows.Forms.Padding ToolbarPlusButtonTextPadding = new System.Windows.Forms.Padding(0, 0, 0, 2);
        public static readonly System.Windows.Forms.Padding ToolbarMinusButtonTextPadding = new System.Windows.Forms.Padding(0, 0, 0, 2);
        public static readonly System.Windows.Forms.Padding MainToolbarButtonTextPadding = new System.Windows.Forms.Padding(0, 0, 0, 2);
        // button text positioning settings end

        // button size settings start
        public static readonly Size DialogButtonSize = new Size(75, 27);
        public static readonly Size ToolbarButtonSize = new Size(32, 32);
        public static readonly Size MainToolbarButtonSize = ToolbarButtonSize;
        public const int ToolbarButtonSpacing = 6;
        // button size settings end

        // resizable dialog helper start
        public static bool IsInResizeGripArea(Size clientSize, Point clientPoint)
        {
            return clientPoint.X >= clientSize.Width - BackupDialogResizeGripSize &&
                   clientPoint.Y >= clientSize.Height - BackupDialogResizeGripSize;
        }

        public static void DrawResizeGrip(Graphics graphics, Size clientSize)
        {
            using Pen pen = new Pen(WindowBorderColor, 1F);

            int right = clientSize.Width - 5;
            int bottom = clientSize.Height - 5;

            graphics.DrawLine(pen, right - 10, bottom, right, bottom - 10);
            graphics.DrawLine(pen, right - 6, bottom, right, bottom - 6);
            graphics.DrawLine(pen, right - 2, bottom, right, bottom - 2);
        }
        // resizable dialog helper end
        public static void ApplyInactiveComboBoxStyle(System.Windows.Forms.ComboBox comboBox)
        {
            comboBox.Enabled = false;
            comboBox.TabStop = false;
            comboBox.Cursor = System.Windows.Forms.Cursors.Default;
            comboBox.BackColor = DisabledControlBackColor;
            comboBox.ForeColor = DisabledTextColor;
        }
        // shared icon drawing start
        public static void DrawSettingsIcon(Graphics graphics, Rectangle bounds, Color outlineColor, float penWidth)
        {
            System.Drawing.Drawing2D.SmoothingMode previousSmoothingMode = graphics.SmoothingMode;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using Pen pen = new Pen(outlineColor, penWidth)
            {
                StartCap = System.Drawing.Drawing2D.LineCap.Square,
                EndCap = System.Drawing.Drawing2D.LineCap.Square,
                LineJoin = System.Drawing.Drawing2D.LineJoin.Miter
            };

            int left = bounds.Left + (bounds.Width - 18) / 2;
            int top = bounds.Top + (bounds.Height - 19) / 2;

            Point[] gearPoints =
            {
            new Point(left + 7, top),
            new Point(left + 11, top),
            new Point(left + 11, top + 3),
            new Point(left + 12, top + 4),
            new Point(left + 15, top + 2),
            new Point(left + 17, top + 4),
            new Point(left + 15, top + 7),
            new Point(left + 16, top + 8),
            new Point(left + 18, top + 8),
            new Point(left + 18, top + 10),
            new Point(left + 16, top + 10),
            new Point(left + 15, top + 12),
            new Point(left + 17, top + 15),
            new Point(left + 15, top + 17),
            new Point(left + 12, top + 15),
            new Point(left + 11, top + 16),
            new Point(left + 11, top + 19),
            new Point(left + 7, top + 19),
            new Point(left + 7, top + 16),
            new Point(left + 6, top + 15),
            new Point(left + 3, top + 17),
            new Point(left + 1, top + 15),
            new Point(left + 3, top + 12),
            new Point(left + 2, top + 10),
            new Point(left, top + 10),
            new Point(left, top + 8),
            new Point(left + 2, top + 8),
            new Point(left + 3, top + 7),
            new Point(left + 1, top + 4),
            new Point(left + 3, top + 2),
            new Point(left + 6, top + 4),
            new Point(left + 7, top + 3)
        };

            graphics.DrawPolygon(pen, gearPoints);
            graphics.DrawEllipse(pen, left + 6, top + 6, 6, 6);

            graphics.SmoothingMode = previousSmoothingMode;
        }
        // shared icon drawing end

        // dialog button factory start
        public static System.Windows.Forms.Button CreateDialogPrimaryButton(string name, string text)
        {
            System.Windows.Forms.Button button = CreateDialogButton(name, text, AccentColor, DarkTextColor, System.Windows.Forms.DialogResult.None);
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        public static System.Windows.Forms.Button CreateDialogSecondaryButton(string name, string text, System.Windows.Forms.DialogResult dialogResult)
        {
            System.Windows.Forms.Button button = CreateDialogButton(name, text, ControlBackColor, TextColor, dialogResult);
            button.FlatAppearance.BorderColor = AccentColor;
            button.FlatAppearance.BorderSize = 1;
            return button;
        }

        public static void PositionSingleDialogButton(System.Windows.Forms.Control parent, System.Windows.Forms.Button button, bool reserveResizeGrip)
        {
            int rightMargin = reserveResizeGrip
                ? MessageDialogButtonRightMargin + BackupDialogResizeGripSize
                : MessageDialogButtonRightMargin;

            button.Location = new Point(
                parent.ClientSize.Width - rightMargin - DialogButtonSize.Width,
                parent.ClientSize.Height - MessageDialogButtonBottomMargin - DialogButtonSize.Height);
        }

        public static void PositionDialogButtons(System.Windows.Forms.Control parent, System.Windows.Forms.Button primaryButton, System.Windows.Forms.Button secondaryButton)
        {
            PositionDialogButtons(parent, primaryButton, secondaryButton, false);
        }

        public static void PositionDialogButtons(System.Windows.Forms.Control parent, System.Windows.Forms.Button primaryButton, System.Windows.Forms.Button secondaryButton, bool reserveResizeGrip)
        {
            int rightMargin = reserveResizeGrip
                ? BackupDialogContentRightMargin + BackupDialogResizeGripSize
                : BackupDialogContentRightMargin;

            secondaryButton.Location = new Point(
                parent.ClientSize.Width - rightMargin - DialogButtonSize.Width,
                parent.ClientSize.Height - BackupDialogBottomMargin - DialogButtonSize.Height);

            primaryButton.Location = new Point(
                secondaryButton.Left - ToolbarButtonSpacing - DialogButtonSize.Width,
                secondaryButton.Top);
        }

        private static System.Windows.Forms.Button CreateDialogButton(string name, string text, Color backColor, Color foreColor, System.Windows.Forms.DialogResult dialogResult)
        {
            System.Windows.Forms.Button button = new System.Windows.Forms.Button
            {
                Name = name,
                Text = string.Empty,
                Tag = text,
                Size = DialogButtonSize,
                Anchor = System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom,
                DialogResult = dialogResult,
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = foreColor,
                Cursor = System.Windows.Forms.Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = DialogButtonTextPadding,
                UseVisualStyleBackColor = false
            };

            button.FlatAppearance.MouseOverBackColor = ControlHoverBackColor;
            button.FlatAppearance.MouseDownBackColor = AccentColor;
            button.Paint += DialogButton_Paint;

            return button;
        }

        private static void DialogButton_Paint(object? sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (sender is not System.Windows.Forms.Button button)
            {
                return;
            }

            string text = button.Tag as string ?? string.Empty;

            System.Windows.Forms.TextRenderer.DrawText(
                e.Graphics,
                text,
                button.Font,
                button.ClientRectangle,
                button.ForeColor,
                System.Windows.Forms.TextFormatFlags.HorizontalCenter |
                System.Windows.Forms.TextFormatFlags.VerticalCenter |
                System.Windows.Forms.TextFormatFlags.NoPadding |
                System.Windows.Forms.TextFormatFlags.EndEllipsis);
        }
        // dialog button factory end
    }
}