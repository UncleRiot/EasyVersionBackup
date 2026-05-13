// Design-Rule / UI consistency:
// Keep layout, spacing, colors, sizes, and fonts aligned with ModernTheme.
// Add new shared visual values to ModernTheme instead of hardcoding local exceptions here.
// 03.05.2026 /dc

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EasyVersionBackup
{
    public partial class VersionInputForm : Form
    {
        private const int WmNclButtonDown = 0xA1;
        private const int HtCaption = 0x2;
        private const int WmNcHitTest = 0x84;
        private const int HtBottomRight = 17;

        private readonly ToolTip _versionInputToolTip = new ToolTip();
        private readonly List<string> _availableTags;
        private readonly ModernTheme.ModernScrollBar _verticalScrollBar;
        private readonly ModernTheme.ModernScrollBar _horizontalScrollBar;
        private bool _isUpdatingScrollBars;

        public List<BackupVersionItem> ResultItems { get; private set; }
        public bool IgnoreCopyErrors { get; private set; }
        public Size DialogSize { get; private set; }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        public VersionInputForm(List<BackupVersionItem> items, bool ignoreCopyErrors, List<string> availableTags, Size savedDialogSize)
        {
            InitializeComponent();

            _availableTags = availableTags
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(tag => tag, StringComparer.OrdinalIgnoreCase)
                .ToList();

            _verticalScrollBar = new ModernTheme.ModernScrollBar
            {
                Name = "verticalScrollBarVersions",
                Orientation = Orientation.Vertical,
                Width = ModernTheme.DataGridViewScrollBarSize,
                Visible = false
            };

            _horizontalScrollBar = new ModernTheme.ModernScrollBar
            {
                Name = "horizontalScrollBarVersions",
                Orientation = Orientation.Horizontal,
                Height = ModernTheme.DataGridViewScrollBarSize,
                Visible = false
            };

            Controls.Add(_verticalScrollBar);
            Controls.Add(_horizontalScrollBar);

            Text = "Backup Version";
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.None;
            MinimumSize = new Size(514, 290);
            SizeGripStyle = SizeGripStyle.Show;
            BackColor = ModernTheme.WindowBackColor;
            Font = new Font(ModernTheme.FontFamilyName, ModernTheme.DefaultFontSize);
            DoubleBuffered = true;
            ModernWindowFrame.Apply(this);

            OffsetExistingControlsForModernTitleBar(ModernTheme.TitleBarHeight);
            InitializeModernTitleBar();

            ApplySavedDialogSize(savedDialogSize);

            foreach (Control control in Controls)
            {
                if (control is Label label)
                {
                    label.ForeColor = ModernTheme.TextColor;
                    label.BackColor = Color.Transparent;
                }
            }

            _versionInputToolTip.SetToolTip(checkBoxIgnoreCopyErrors, "Skip files that cannot be copied");

            if (dataGridViewVersions.Columns.Count > 0)
            {
                dataGridViewVersions.Columns[0].ToolTipText = "Source folder";
            }

            if (dataGridViewVersions.Columns.Count > 1)
            {
                dataGridViewVersions.Columns[1].ToolTipText = "Backup version";
            }

            if (dataGridViewVersions.Columns.Count > 2)
            {
                dataGridViewVersions.Columns[2].ToolTipText = "Optional backup tag";
            }

            ColumnSourceName.Width = 260;
            ColumnVersion.Width = 125;
            ColumnTag.Width = 95;

            checkBoxIgnoreCopyErrors.BackColor = ModernTheme.WindowBackColor;
            checkBoxIgnoreCopyErrors.ForeColor = ModernTheme.TextColor;
            checkBoxIgnoreCopyErrors.UseVisualStyleBackColor = false;

            dataGridViewVersions.BorderStyle = BorderStyle.None;
            dataGridViewVersions.BackgroundColor = ModernTheme.WindowBackColor;
            dataGridViewVersions.GridColor = ModernTheme.ControlBackColor;
            dataGridViewVersions.EnableHeadersVisualStyles = false;
            dataGridViewVersions.ColumnHeadersDefaultCellStyle.BackColor = ModernTheme.ControlBackColor;
            dataGridViewVersions.ColumnHeadersDefaultCellStyle.ForeColor = ModernTheme.TextColor;
            dataGridViewVersions.ColumnHeadersDefaultCellStyle.SelectionBackColor = ModernTheme.ControlBackColor;
            dataGridViewVersions.ColumnHeadersDefaultCellStyle.SelectionForeColor = ModernTheme.TextColor;
            dataGridViewVersions.ColumnHeadersDefaultCellStyle.Font = new Font(ModernTheme.FontFamilyName, ModernTheme.HeaderFontSize, FontStyle.Bold);
            dataGridViewVersions.DefaultCellStyle.BackColor = ModernTheme.TitleBarBackColor;
            dataGridViewVersions.DefaultCellStyle.ForeColor = ModernTheme.TextColor;
            dataGridViewVersions.DefaultCellStyle.SelectionBackColor = ModernTheme.AccentColor;
            dataGridViewVersions.DefaultCellStyle.SelectionForeColor = ModernTheme.DarkTextColor;
            dataGridViewVersions.AlternatingRowsDefaultCellStyle.BackColor = ModernTheme.WindowBackColor;
            dataGridViewVersions.AlternatingRowsDefaultCellStyle.ForeColor = ModernTheme.TextColor;
            dataGridViewVersions.ScrollBars = ScrollBars.None;

            if (ColumnTag.Items.Count == 0)
            {
                ColumnTag.Items.Add("None");

                foreach (string tag in _availableTags)
                {
                    ColumnTag.Items.Add(tag);
                }
            }

            StyleModernButton(buttonOk, ModernTheme.AccentColor, ModernTheme.DarkTextColor, false, ModernTheme.DialogPrimaryButtonTextPadding);
            StyleModernButton(buttonCancel, ModernTheme.ControlBackColor, ModernTheme.TextColor, true, ModernTheme.DialogSecondaryButtonTextPadding);

            ResultItems = new List<BackupVersionItem>();
            checkBoxIgnoreCopyErrors.Checked = ignoreCopyErrors;

            dataGridViewVersions.Rows.Clear();

            foreach (BackupVersionItem item in items)
            {
                string tag = string.IsNullOrWhiteSpace(item.Tag) ? "None" : item.Tag.Trim();

                if (!ColumnTag.Items.Contains(tag))
                {
                    tag = "None";
                }

                dataGridViewVersions.Rows.Add(item.SourceName, item.Version, tag);
            }

            _verticalScrollBar.ScrollValueChanged += verticalScrollBarVersions_ScrollValueChanged;
            _horizontalScrollBar.ScrollValueChanged += horizontalScrollBarVersions_ScrollValueChanged;
            dataGridViewVersions.Scroll += dataGridViewVersions_Scroll;
            dataGridViewVersions.RowsAdded += dataGridViewVersions_ContentChanged;
            dataGridViewVersions.RowsRemoved += dataGridViewVersions_ContentChanged;
            dataGridViewVersions.ColumnWidthChanged += dataGridViewVersions_ContentChanged;
            dataGridViewVersions.MouseWheel += dataGridViewVersions_MouseWheel;
            Resize += VersionInputForm_Resize;
            FormClosing += VersionInputForm_FormClosing;

            ApplyDialogLayout();
            PositionBottomRight();
        }

        private void ApplySavedDialogSize(Size savedDialogSize)
        {
            Size defaultDialogSize = new Size(514, 330 + ModernTheme.TitleBarHeight);

            Size requestedDialogSize = savedDialogSize.Width >= MinimumSize.Width && savedDialogSize.Height >= MinimumSize.Height
                ? savedDialogSize
                : defaultDialogSize;

            Size = new Size(
                Math.Max(MinimumSize.Width, requestedDialogSize.Width),
                Math.Max(MinimumSize.Height, requestedDialogSize.Height));

            DialogSize = Size;
        }

        private void OffsetExistingControlsForModernTitleBar(int offsetY)
        {
            ClientSize = new Size(ClientSize.Width, ClientSize.Height + offsetY);

            foreach (Control control in Controls)
            {
                control.Top += offsetY;
            }
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

        private void ModernTitleBar_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            ReleaseCapture();
            SendMessage(Handle, WmNclButtonDown, HtCaption, 0);
        }

        private void StyleModernButton(Button button, Color backColor, Color foreColor, bool showBorder, Padding textPadding)
        {
            button.Size = ModernTheme.DialogButtonSize;
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

        private void ApplyDialogLayout()
        {
            int margin = 12;
            int bottomMargin = 14;
            int buttonGap = 8;
            int scrollBarSize = ModernTheme.DataGridViewScrollBarSize;

            labelInfo.Location = new Point(margin, ModernTheme.TitleBarHeight + 12);

            int buttonTop = ClientSize.Height - bottomMargin - buttonCancel.Height;
            checkBoxIgnoreCopyErrors.Location = new Point(margin, buttonTop - 40);

            int gridTop = labelInfo.Bottom + 8;
            int gridBottom = checkBoxIgnoreCopyErrors.Top - 12;
            int gridHeight = Math.Max(105, gridBottom - gridTop - scrollBarSize);
            int availableGridWidth = Math.Max(220, ClientSize.Width - (margin * 2) - scrollBarSize);
            int preferredGridWidth = ColumnSourceName.Width + ColumnVersion.Width + ColumnTag.Width;
            int gridWidth = Math.Max(220, Math.Min(availableGridWidth, preferredGridWidth));

            dataGridViewVersions.Location = new Point(margin, gridTop);
            dataGridViewVersions.Size = new Size(gridWidth, gridHeight);

            _verticalScrollBar.Location = new Point(dataGridViewVersions.Right, dataGridViewVersions.Top);
            _verticalScrollBar.Size = new Size(scrollBarSize, dataGridViewVersions.Height);

            _horizontalScrollBar.Location = new Point(dataGridViewVersions.Left, dataGridViewVersions.Bottom);
            _horizontalScrollBar.Size = new Size(dataGridViewVersions.Width, scrollBarSize);

            UpdateVersionScrollBars();

            int tableRight = _verticalScrollBar.Visible
                ? _verticalScrollBar.Right
                : dataGridViewVersions.Right;

            buttonCancel.Location = new Point(tableRight - buttonCancel.Width, buttonTop);
            buttonOk.Location = new Point(buttonCancel.Left - buttonGap - buttonOk.Width, buttonTop);
        }

        private void UpdateVersionScrollBars()
        {
            if (_isUpdatingScrollBars)
            {
                return;
            }

            _isUpdatingScrollBars = true;

            try
            {
                int visibleRowCount = Math.Max(1, (dataGridViewVersions.ClientSize.Height - dataGridViewVersions.ColumnHeadersHeight) / Math.Max(1, dataGridViewVersions.RowTemplate.Height));
                int maxFirstDisplayedRowIndex = Math.Max(0, dataGridViewVersions.Rows.Count - visibleRowCount);

                _verticalScrollBar.Minimum = 0;
                _verticalScrollBar.Maximum = maxFirstDisplayedRowIndex;
                _verticalScrollBar.LargeChange = visibleRowCount;
                _verticalScrollBar.Visible = maxFirstDisplayedRowIndex > 0;

                int firstDisplayedRowIndex = GetFirstDisplayedScrollingRowIndex();
                _verticalScrollBar.Value = Math.Min(maxFirstDisplayedRowIndex, Math.Max(0, firstDisplayedRowIndex));

                int totalColumnWidth = dataGridViewVersions.Columns
                    .Cast<DataGridViewColumn>()
                    .Where(column => column.Visible)
                    .Sum(column => column.Width);

                int maxHorizontalOffset = Math.Max(0, totalColumnWidth - dataGridViewVersions.ClientSize.Width);

                _horizontalScrollBar.Minimum = 0;
                _horizontalScrollBar.Maximum = maxHorizontalOffset;
                _horizontalScrollBar.LargeChange = dataGridViewVersions.ClientSize.Width;
                _horizontalScrollBar.Visible = maxHorizontalOffset > 0;
                _horizontalScrollBar.Value = Math.Min(maxHorizontalOffset, Math.Max(0, dataGridViewVersions.HorizontalScrollingOffset));
            }
            finally
            {
                _isUpdatingScrollBars = false;
            }
        }

        private int GetFirstDisplayedScrollingRowIndex()
        {
            try
            {
                return dataGridViewVersions.FirstDisplayedScrollingRowIndex;
            }
            catch (InvalidOperationException)
            {
                return 0;
            }
        }

        private void verticalScrollBarVersions_ScrollValueChanged(object? sender, EventArgs e)
        {
            if (_isUpdatingScrollBars || dataGridViewVersions.Rows.Count == 0)
            {
                return;
            }

            try
            {
                int value = Math.Min(_verticalScrollBar.Value, dataGridViewVersions.Rows.Count - 1);
                dataGridViewVersions.FirstDisplayedScrollingRowIndex = value;
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void horizontalScrollBarVersions_ScrollValueChanged(object? sender, EventArgs e)
        {
            if (_isUpdatingScrollBars)
            {
                return;
            }

            dataGridViewVersions.HorizontalScrollingOffset = Math.Max(0, _horizontalScrollBar.Value);
        }

        private void dataGridViewVersions_Scroll(object? sender, ScrollEventArgs e)
        {
            UpdateVersionScrollBars();
        }

        private void dataGridViewVersions_ContentChanged(object? sender, EventArgs e)
        {
            UpdateVersionScrollBars();
        }

        private void dataGridViewVersions_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (_verticalScrollBar.Visible)
            {
                _verticalScrollBar.Value += e.Delta > 0 ? -1 : 1;
            }
        }

        private void VersionInputForm_Resize(object? sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                DialogSize = Size;
            }

            ApplyDialogLayout();
        }

        private void VersionInputForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            DialogSize = WindowState == FormWindowState.Normal
                ? Size
                : RestoreBounds.Size;
        }

        private void PositionBottomRight()
        {
            Screen? screen = Screen.PrimaryScreen ?? Screen.AllScreens.FirstOrDefault();

            if (screen == null)
            {
                StartPosition = FormStartPosition.CenterScreen;
                return;
            }

            Rectangle workingArea = screen.WorkingArea;

            int x = workingArea.Right - Width - 10;
            int y = workingArea.Bottom - Height - 10;

            Location = new Point(x, y);
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
                Point cursorPosition = PointToClient(new Point(
                    unchecked((short)(long)m.LParam),
                    unchecked((short)((long)m.LParam >> 16))));

                if (ModernWindowFrame.TryGetResizeHitTestResult(ClientSize, cursorPosition, out int resizeHitTestResult))
                {
                    m.Result = resizeHitTestResult;
                    return;
                }
            }

            base.WndProc(ref m);
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            ResultItems.Clear();

            IgnoreCopyErrors = checkBoxIgnoreCopyErrors.Checked;

            foreach (DataGridViewRow row in dataGridViewVersions.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                string sourceName = row.Cells[0].Value?.ToString() ?? string.Empty;
                string version = row.Cells[1].Value?.ToString()?.Trim() ?? string.Empty;
                string tag = row.Cells[2].Value?.ToString()?.Trim() ?? string.Empty;

                if (string.Equals(tag, "None", StringComparison.OrdinalIgnoreCase))
                {
                    tag = string.Empty;
                }

                if (!string.IsNullOrWhiteSpace(version) && !VersionHelper.IsValidVersion(version))
                {
                    ModernMessageDialog.Show(
                        this,
                        "Error",
                        $"Invalid version for '{sourceName}'. The value must be usable as part of a file name.");

                    return;
                }

                if (!string.IsNullOrWhiteSpace(tag) && !VersionPatternHelper.IsValidVersionValue(tag))
                {
                    ModernMessageDialog.Show(
                        this,
                        "Error",
                        $"Invalid tag for '{sourceName}'. The value must be usable as part of a file name.");

                    return;
                }

                ResultItems.Add(new BackupVersionItem
                {
                    SourceName = sourceName,
                    Version = version,
                    Tag = tag
                });
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
