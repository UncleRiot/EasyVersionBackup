// Design-Rule / UI standards:
// This file defines the shared visual values for the application.
// Forms should reuse these values to keep layout, spacing, colors, sizes, and fonts consistent.
// 03.05.2026 /dc


using System;
using System.Drawing;

// Semantic Anchor [ModernTheme.cs]: Single source of truth for shared colors, spacing, sizing and reusable UI rendering; keep deliberate local exceptions unchanged unless explicitly requested.
namespace EasyVersionBackup
{
    public static class ModernTheme
    {
        // Semantic Anchor: Shared drag row preview appearance; existing theme values remain authoritative.
        public const double DragRowPreviewOpacity = 0.72;

        // Semantic Anchor: All popup menus use the shared dark palette, including their image margin and selection.
        public static System.Windows.Forms.ToolStripRenderer CreateContextMenuRenderer()
        {
            return new System.Windows.Forms.ToolStripProfessionalRenderer(new DarkContextMenuColorTable());
        }

        private sealed class DarkContextMenuColorTable : System.Windows.Forms.ProfessionalColorTable
        {
            public override Color ToolStripDropDownBackground => ControlBackColor;
            public override Color ImageMarginGradientBegin => WindowBackColor;
            public override Color ImageMarginGradientMiddle => WindowBackColor;
            public override Color ImageMarginGradientEnd => WindowBackColor;
            public override Color MenuItemSelected => ControlHoverBackColor;
            public override Color MenuItemSelectedGradientBegin => ControlHoverBackColor;
            public override Color MenuItemSelectedGradientEnd => ControlHoverBackColor;
            public override Color MenuItemBorder => ControlHoverBackColor;
            public override Color MenuBorder => WindowBorderColor;
            public override Color SeparatorDark => WindowBackColor;
            public override Color SeparatorLight => WindowBorderColor;
        }

        // window color settings start
        public static Color WindowBackColor { get; private set; } = Color.FromArgb(27, 40, 56);
        public static Color TitleBarBackColor { get; private set; } = Color.FromArgb(23, 26, 33);
        public static Color ControlBackColor { get; private set; } = Color.FromArgb(42, 71, 94);
        public static Color ControlHoverBackColor { get; private set; } = Color.FromArgb(55, 90, 120);
        public static Color AccentColor { get; private set; } = Color.FromArgb(102, 192, 244);
        public static Color AccentHoverColor { get; private set; } = Color.FromArgb(143, 212, 255);
        public static Color TextColor { get; private set; } = Color.FromArgb(199, 213, 224);
        public static Color DarkTextColor { get; private set; } = Color.FromArgb(23, 26, 33);
        public static readonly Color CloseButtonHoverColor = Color.FromArgb(196, 43, 28);
        public static readonly Color DestructiveActionColor = Color.FromArgb(196, 43, 28);
        public static readonly Color DestructiveActionHoverColor = Color.FromArgb(220, 58, 42);
        public static readonly Color DestructiveActionPressedColor = Color.FromArgb(154, 32, 22);
        // window color settings end

        // Semantic Anchor [Theme backdrop opacity]: Central style values for compositing
        // the animated window effect behind existing WinForms surfaces. The Default theme
        // does not use these values; keep text/selection contrast and hit targets unchanged.
        public const int ThemeTitleBackdropOpacity = 75;
        public const int ThemeWarningBackdropOpacity = 95;
        public const int ThemeGridHeaderBackdropOpacity = 155;
        public const int ThemeGridCellBackdropOpacity = 110;
        public const int ThemeGridSelectionBackdropOpacity = 220;
        public const int ThemeGridEmptyBackdropOpacity = 75;

        // Semantic Anchor [Native checklist limitation]: CheckedListBox is not owner-drawable;
        // keep native checked state and use the same readable control color across all themes.
        internal static Color GetRetentionListBackColor() =>
            VisualThemeRuntime.ActiveId == "Default" ? TitleBarBackColor : ControlBackColor;

        // Semantic Anchor [Runtime palette]: ModernTheme is the only WinForms color SSOT;
        // the original values above remain the Default baseline, and VisualThemeRuntime
        // alone changes these nine theme-dependent properties on the UI thread.
        internal static void SetVisualThemeColors(Color[] colors)
        {
            if (colors.Length != 9) throw new ArgumentException("Invalid theme palette length.", nameof(colors));
            WindowBackColor = colors[0];
            TitleBarBackColor = colors[1];
            ControlBackColor = colors[2];
            ControlHoverBackColor = colors[3];
            AccentColor = colors[4];
            AccentHoverColor = colors[5];
            TextColor = colors[6];
            DarkTextColor = colors[7];
            WindowBorderColor = colors[8];
        }

        // status color settings start
        public static readonly Color BackupInfoDefaultColor = Color.FromArgb(0, 120, 215);
        public static readonly Color BackupInfoOkColor = Color.FromArgb(0, 160, 80);
        public static readonly Color BackupInfoWarningColor = Color.FromArgb(230, 180, 0);
        public static readonly Color BackupInfoCleanupColor = Color.FromArgb(128, 0, 160);
        public static readonly Color BackupInfoErrorColor = Color.FromArgb(200, 0, 0);
        public static readonly Color BackupInfoTextColor = Color.White;
        public static readonly Color ActiveExclusionColor = Color.LimeGreen;
        public static readonly Color ActiveRetentionColor = Color.FromArgb(196, 43, 28);
        public static readonly Color ActiveSourceCleanupColor = Color.FromArgb(40, 225, 255);
        public static readonly Color BackupProgressFillColor = Color.FromArgb(72, 160, 170, 178);
        public static readonly Color BackupProgressPulseColor = Color.FromArgb(96, 190, 200, 208);
        // public static readonly Color DisabledTextColor = WindowBorderColor;
        public static readonly Color DisabledTextColor = Color.FromArgb(92, 104, 112);
        public static readonly Color DisabledControlBackColor = Color.FromArgb(32, 43, 56);
        // status color settings end

        // window border settings start
        public static Color WindowBorderColor { get; private set; } = Color.FromArgb(92, 104, 112);
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

        // backup info dialog layout settings start
        public static readonly Size BackupInfoDialogDefaultSize = new Size(760, 430);
        public static readonly Size BackupInfoDialogMinimumClientSize = new Size(620, 330);
        public const int BackupInfoDialogMargin = 12;
        public const int BackupInfoDialogInfoTop = 44;
        public const int BackupInfoDialogInfoHeight = 54;
        public const int BackupInfoDialogSpacing = 8;
        public const int BackupInfoDialogButtonBottomMargin = 12;
        public const int BackupInfoDialogResizeGripReservedWidth = 22;
        public const int BackupInfoDialogStatusColumnWidth = 56;
        public const int BackupInfoDialogDateColumnWidth = 145;
        public const int BackupInfoDialogTextColumnMinimumWidth = 260;
        public const int BackupInfoDialogMinimumGridHeight = 120;
        // backup info dialog layout settings end

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
            // Semantic Anchor [Dialog checkbox backdrop SSOT]: Style only, not state.
            return new ThemeBackdropCheckBox
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

        // themed scroll bar start
        public static readonly Color DataGridViewScrollBarTrackColor = ControlBackColor;
        public static readonly Color DataGridViewScrollBarThumbColor = ControlBackColor;
        public const int DataGridViewScrollBarSize = 14;
        public const int DataGridViewScrollBarMinimumThumbSize = 24;

        public sealed class ModernScrollBar : System.Windows.Forms.Control
        {
            private bool _isDragging;
            private int _dragOffset;
            private int _value;

            public event EventHandler? ScrollValueChanged;

            // Semantic Anchor [ModernScrollBar runtime state]: These values are set by the
            // hosting view at runtime; the WinForms Designer must never persist them.
            // The control's existing painting, scrolling and event contracts remain unchanged.
            [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
            public System.Windows.Forms.Orientation Orientation { get; set; }

            [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
            public int Minimum { get; set; }

            [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
            public int Maximum { get; set; }

            [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
            public int LargeChange { get; set; } = 1;

            [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
            public int Value
            {
                get => _value;
                set
                {
                    int normalizedValue = Math.Max(Minimum, Math.Min(Maximum, value));

                    if (_value == normalizedValue)
                    {
                        return;
                    }

                    _value = normalizedValue;
                    Invalidate();
                    ScrollValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            public ModernScrollBar()
            {
                SetStyle(
                    System.Windows.Forms.ControlStyles.AllPaintingInWmPaint |
                    System.Windows.Forms.ControlStyles.UserPaint |
                    System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer |
                    System.Windows.Forms.ControlStyles.ResizeRedraw,
                    true);

                BackColor = DataGridViewScrollBarTrackColor;
                ForeColor = DataGridViewScrollBarThumbColor;
                Cursor = System.Windows.Forms.Cursors.Hand;
            }

            protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
            {
                base.OnPaint(e);

                using SolidBrush trackBrush = new SolidBrush(WindowBackColor);
                e.Graphics.FillRectangle(trackBrush, ClientRectangle);

                if (Maximum <= Minimum)
                {
                    return;
                }

                using SolidBrush thumbBrush = new SolidBrush(ControlBackColor);
                e.Graphics.FillRectangle(thumbBrush, GetThumbRectangle());
            }

            protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
            {
                base.OnMouseDown(e);

                if (e.Button != System.Windows.Forms.MouseButtons.Left || Maximum <= Minimum)
                {
                    return;
                }

                Rectangle thumbRectangle = GetThumbRectangle();
                int mousePosition = Orientation == System.Windows.Forms.Orientation.Vertical ? e.Y : e.X;

                if (thumbRectangle.Contains(e.Location))
                {
                    _isDragging = true;
                    _dragOffset = mousePosition - (Orientation == System.Windows.Forms.Orientation.Vertical ? thumbRectangle.Top : thumbRectangle.Left);
                    Capture = true;
                    return;
                }

                Value += mousePosition < (Orientation == System.Windows.Forms.Orientation.Vertical ? thumbRectangle.Top : thumbRectangle.Left)
                    ? -LargeChange
                    : LargeChange;
            }

            protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
            {
                base.OnMouseMove(e);

                if (!_isDragging || Maximum <= Minimum)
                {
                    return;
                }

                int length = Orientation == System.Windows.Forms.Orientation.Vertical ? Height : Width;
                Rectangle thumbRectangle = GetThumbRectangle();
                int availableTrack = Math.Max(1, length - (Orientation == System.Windows.Forms.Orientation.Vertical ? thumbRectangle.Height : thumbRectangle.Width));
                int mousePosition = Orientation == System.Windows.Forms.Orientation.Vertical ? e.Y : e.X;
                int thumbStart = Math.Max(0, Math.Min(availableTrack, mousePosition - _dragOffset));

                Value = Minimum + (int)Math.Round((double)thumbStart / availableTrack * (Maximum - Minimum));
            }

            protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
            {
                base.OnMouseUp(e);

                _isDragging = false;
                Capture = false;
            }

            protected override void OnMouseWheel(System.Windows.Forms.MouseEventArgs e)
            {
                base.OnMouseWheel(e);

                if (Maximum <= Minimum)
                {
                    return;
                }

                Value += e.Delta > 0 ? -1 : 1;
            }

            private Rectangle GetThumbRectangle()
            {
                int length = Orientation == System.Windows.Forms.Orientation.Vertical ? Height : Width;
                int thickness = Orientation == System.Windows.Forms.Orientation.Vertical ? Width : Height;

                if (length <= 0 || thickness <= 0)
                {
                    return Rectangle.Empty;
                }

                int scrollableRange = Math.Max(0, Maximum - Minimum);
                int largeChange = Math.Max(1, LargeChange);
                int virtualItemCount = Math.Max(1, scrollableRange + largeChange);

                int thumbLength = scrollableRange == 0
                    ? length
                    : (int)Math.Round(length * ((double)largeChange / virtualItemCount));

                thumbLength = Math.Max(DataGridViewScrollBarMinimumThumbSize, thumbLength);
                thumbLength = Math.Min(length, thumbLength);

                int availableTrack = Math.Max(1, length - thumbLength);

                int thumbStart = scrollableRange == 0
                    ? 0
                    : (int)Math.Round((double)(Value - Minimum) / scrollableRange * availableTrack);

                thumbStart = Math.Max(0, Math.Min(availableTrack, thumbStart));

                return Orientation == System.Windows.Forms.Orientation.Vertical
                    ? new Rectangle(0, thumbStart, thickness, thumbLength)
                    : new Rectangle(thumbStart, 0, thumbLength, thickness);
            }
        }
        // themed scroll bar end

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
        public static void DrawSettingsIcon(
            Graphics graphics,
            Rectangle bounds,
            Color outlineColor,
            float penWidth,
            Color fillColor)
        {
            System.Drawing.Drawing2D.SmoothingMode previousSmoothingMode =
                graphics.SmoothingMode;

            graphics.SmoothingMode =
                System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

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

            using SolidBrush fillBrush = new SolidBrush(fillColor);
            graphics.FillPolygon(fillBrush, gearPoints);

            using Pen pen = new Pen(outlineColor, penWidth)
            {
                StartCap = System.Drawing.Drawing2D.LineCap.Square,
                EndCap = System.Drawing.Drawing2D.LineCap.Square,
                LineJoin = System.Drawing.Drawing2D.LineJoin.Miter
            };

            graphics.DrawPolygon(pen, gearPoints);

            using SolidBrush centerBrush =
                new SolidBrush(WindowBackColor);

            graphics.FillEllipse(
                centerBrush,
                left + 6,
                top + 6,
                6,
                6);

            graphics.DrawEllipse(
                pen,
                left + 6,
                top + 6,
                6,
                6);

            graphics.SmoothingMode = previousSmoothingMode;
        }

        public static void DrawSettingsIcon(
            Graphics graphics,
            Rectangle bounds,
            Color outlineColor,
            float penWidth,
            Color leftFillColor,
            Color rightFillColor)
        {
            System.Drawing.Drawing2D.SmoothingMode previousSmoothingMode =
                graphics.SmoothingMode;

            graphics.SmoothingMode =
                System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

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

            System.Drawing.Drawing2D.GraphicsState graphicsState =
                graphics.Save();

            using System.Drawing.Drawing2D.GraphicsPath gearPath =
                new System.Drawing.Drawing2D.GraphicsPath();

            gearPath.AddPolygon(gearPoints);
            graphics.SetClip(gearPath);

            using SolidBrush leftFillBrush =
                new SolidBrush(leftFillColor);
            using SolidBrush rightFillBrush =
                new SolidBrush(rightFillColor);

            graphics.FillRectangle(
                leftFillBrush,
                left,
                top,
                9,
                19);

            graphics.FillRectangle(
                rightFillBrush,
                left + 9,
                top,
                9,
                19);

            graphics.Restore(graphicsState);

            using Pen pen = new Pen(outlineColor, penWidth)
            {
                StartCap =
                    System.Drawing.Drawing2D.LineCap.Square,
                EndCap =
                    System.Drawing.Drawing2D.LineCap.Square,
                LineJoin =
                    System.Drawing.Drawing2D.LineJoin.Miter
            };

            graphics.DrawPolygon(pen, gearPoints);

            using SolidBrush centerBrush =
                new SolidBrush(WindowBackColor);

            graphics.FillEllipse(
                centerBrush,
                left + 6,
                top + 6,
                6,
                6);

            graphics.DrawEllipse(
                pen,
                left + 6,
                top + 6,
                6,
                6);

            graphics.SmoothingMode = previousSmoothingMode;
        }

        public static void DrawSettingsStatusMarker(
            Graphics graphics,
            Rectangle bounds,
            string markerText,
            Color markerColor,
            Color markerTextColor,
            bool topLeft)
        {
            System.Drawing.Drawing2D.SmoothingMode previousSmoothingMode =
                graphics.SmoothingMode;

            graphics.SmoothingMode =
                System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            const int markerSize = 11;

            int gearLeft =
                bounds.Left +
                (bounds.Width - 18) / 2;

            int gearTop =
                bounds.Top +
                (bounds.Height - 19) / 2;

            int markerLeft =
                topLeft
                    ? gearLeft - 4
                    : gearLeft + 11;

            int markerTop =
                topLeft
                    ? gearTop - 3
                    : gearTop + 12;

            Rectangle markerBounds =
                new Rectangle(
                    markerLeft,
                    markerTop,
                    markerSize,
                    markerSize);

            using SolidBrush markerBrush =
                new SolidBrush(markerColor);

            graphics.FillEllipse(
                markerBrush,
                markerBounds);

            using Pen markerOutlinePen =
                new Pen(WindowBackColor, 1F);

            graphics.DrawEllipse(
                markerOutlinePen,
                markerBounds);

            using Font markerFont =
                new Font(
                    FontFamilyName,
                    6.5F,
                    FontStyle.Bold,
                    GraphicsUnit.Point);

            TextRenderer.DrawText(
                graphics,
                markerText,
                markerFont,
                markerBounds,
                markerTextColor,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding |
                TextFormatFlags.SingleLine);

            graphics.SmoothingMode =
                previousSmoothingMode;
        }

        public static void DrawBackupInfoStatusIcon(Graphics graphics, Rectangle bounds, Color color)
        {
            System.Drawing.Drawing2D.SmoothingMode previousSmoothingMode = graphics.SmoothingMode;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using SolidBrush circleBrush = new SolidBrush(color);
            graphics.FillEllipse(circleBrush, bounds);

            using SolidBrush iconBrush = new SolidBrush(BackupInfoTextColor);

            int centerX = bounds.Left + bounds.Width / 2;
            graphics.FillEllipse(iconBrush, centerX - 1, bounds.Top + 3, 2, 2);
            graphics.FillRectangle(iconBrush, centerX - 1, bounds.Top + 7, 2, bounds.Height - 10);

            graphics.SmoothingMode = previousSmoothingMode;
        }

        public static void DrawBackupInfoCleanupStatusIcon(Graphics graphics, Rectangle bounds)
        {
            System.Drawing.Drawing2D.SmoothingMode previousSmoothingMode = graphics.SmoothingMode;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using SolidBrush circleBrush = new SolidBrush(BackupInfoCleanupColor);
            graphics.FillEllipse(circleBrush, bounds);

            using Font iconFont = new Font(
                FontFamilyName,
                8F,
                FontStyle.Bold,
                GraphicsUnit.Point);

            using SolidBrush iconBrush = new SolidBrush(BackupInfoTextColor);

            SizeF textSize = graphics.MeasureString("C", iconFont);
            float textX = bounds.Left + (bounds.Width - textSize.Width) / 2F;
            float textY = bounds.Top + (bounds.Height - textSize.Height) / 2F - 0.5F;

            graphics.DrawString("C", iconFont, iconBrush, textX, textY);

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
            System.Windows.Forms.Button button = new ThemeContrastButton
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

        // Semantic Anchor [Theme button contrast SSOT]: Select the more readable
        // of white and near-black using WCAG relative luminance; applies to imported
        // palettes without changing the Default theme or button actions.
        internal static Color GetReadableButtonTextColor(Color surface)
        {
            static double Channel(byte value)
            {
                double scaled = value / 255.0;
                return scaled <= 0.04045 ? scaled / 12.92 : Math.Pow((scaled + 0.055) / 1.055, 2.4);
            }

            double luminance = 0.2126 * Channel(surface.R) +
                0.7152 * Channel(surface.G) + 0.0722 * Channel(surface.B);
            const double whiteLuminance = 1.0;
            const double darkLuminance = 0.0;
            double whiteContrast = (whiteLuminance + 0.05) / (luminance + 0.05);
            double darkContrast = (luminance + 0.05) / (darkLuminance + 0.05);
            return whiteContrast >= darkContrast ? Color.White : Color.Black;
        }

        private static void DialogButton_Paint(object? sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (sender is not System.Windows.Forms.Button button)
            {
                return;
            }

            string text = button.Tag as string ?? string.Empty;

            // Semantic Anchor [Dialog primary contrast]: The accent button must remain an
            // opaque, legible control over animated backgrounds; Default remains unchanged.
            Color textColor = button.ForeColor;
            if (VisualThemeRuntime.ActiveId != "Default")
            {
                bool primary = button.FlatAppearance.BorderSize == 0;
                bool hovering = button.Enabled && button.ClientRectangle.Contains(
                    button.PointToClient(System.Windows.Forms.Cursor.Position));
                Color surface = !button.Enabled ? ControlBackColor :
                    hovering ? (primary ? AccentHoverColor : ControlHoverBackColor) :
                    (primary ? AccentColor : ControlBackColor);
                using (SolidBrush background = new(surface))
                    e.Graphics.FillRectangle(background, button.ClientRectangle);
                if (!primary)
                {
                    using Pen border = new(WindowBorderColor);
                    e.Graphics.DrawRectangle(border, 0, 0,
                        button.ClientRectangle.Width - 1, button.ClientRectangle.Height - 1);
                }
                textColor = GetReadableButtonTextColor(surface);
            }

            System.Windows.Forms.TextRenderer.DrawText(
                e.Graphics,
                text,
                button.Font,
                button.ClientRectangle,
                textColor,
                System.Windows.Forms.TextFormatFlags.HorizontalCenter |
                System.Windows.Forms.TextFormatFlags.VerticalCenter |
                System.Windows.Forms.TextFormatFlags.NoPadding |
                System.Windows.Forms.TextFormatFlags.EndEllipsis);
        }
        // dialog button factory end
    }
}