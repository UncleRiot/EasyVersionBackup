using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EasyVersionBackup
{
    public sealed class ModernBackupInfoDialog : Form
    {
        private readonly Label labelBackupInfo = new Label();
        private readonly DataGridView dataGridViewBackupLog = new DataGridView();
        private readonly Label labelLogFilePath = new Label();
        private readonly Button buttonOk = new Button();
        private readonly ModernTheme.ModernScrollBar verticalScrollBarBackupLog;
        private readonly ModernTheme.ModernScrollBar horizontalScrollBarBackupLog;
        private bool isUpdatingScrollBars;

        public Size DialogSize { get; private set; }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        public ModernBackupInfoDialog(Form owner, string backupInfoText)
            : this(owner, backupInfoText, new List<BackupLogEntry>(), ModernTheme.BackupInfoDialogDefaultSize)
        {
        }

        public ModernBackupInfoDialog(Form owner, string backupInfoText, List<BackupLogEntry> logEntries)
            : this(owner, backupInfoText, logEntries, ModernTheme.BackupInfoDialogDefaultSize)
        {
        }

        public ModernBackupInfoDialog(Form owner, string backupInfoText, List<BackupLogEntry> logEntries, Size savedDialogSize)
        {
            verticalScrollBarBackupLog = new ModernTheme.ModernScrollBar
            {
                Name = "verticalScrollBarBackupLog",
                Orientation = Orientation.Vertical,
                Width = ModernTheme.DataGridViewScrollBarSize,
                Visible = false
            };

            horizontalScrollBarBackupLog = new ModernTheme.ModernScrollBar
            {
                Name = "horizontalScrollBarBackupLog",
                Orientation = Orientation.Horizontal,
                Height = ModernTheme.DataGridViewScrollBarSize,
                Visible = false
            };

            Text = "Backup Info";
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.None;
            MinimumSize = SizeFromClientSize(ModernTheme.BackupInfoDialogMinimumClientSize);
            ClientSize = GetInitialClientSize(savedDialogSize);
            SizeGripStyle = SizeGripStyle.Show;
            BackColor = ModernTheme.WindowBackColor;
            Font = new Font(ModernTheme.FontFamilyName, ModernTheme.DefaultFontSize);
            Icon = owner.Icon;
            DoubleBuffered = true;
            ModernWindowFrame.Apply(this);

            InitializeModernTitleBar();
            InitializeBackupInfoLabel(backupInfoText);
            InitializeBackupLogGrid(logEntries);
            InitializeLogFilePathLabel();
            InitializeDialogButtons();

            Controls.Add(labelBackupInfo);
            Controls.Add(dataGridViewBackupLog);
            Controls.Add(verticalScrollBarBackupLog);
            Controls.Add(horizontalScrollBarBackupLog);
            Controls.Add(labelLogFilePath);
            Controls.Add(buttonOk);

            verticalScrollBarBackupLog.ScrollValueChanged += verticalScrollBarBackupLog_ScrollValueChanged;
            horizontalScrollBarBackupLog.ScrollValueChanged += horizontalScrollBarBackupLog_ScrollValueChanged;
            dataGridViewBackupLog.Scroll += dataGridViewBackupLog_Scroll;
            dataGridViewBackupLog.RowsAdded += dataGridViewBackupLog_ContentChanged;
            dataGridViewBackupLog.RowsRemoved += dataGridViewBackupLog_ContentChanged;
            dataGridViewBackupLog.ColumnWidthChanged += dataGridViewBackupLog_ContentChanged;
            dataGridViewBackupLog.Sorted += dataGridViewBackupLog_ContentChanged;
            dataGridViewBackupLog.MouseWheel += dataGridViewBackupLog_MouseWheel;
            dataGridViewBackupLog.SortCompare += dataGridViewBackupLog_SortCompare;
            Resize += ModernBackupInfoDialog_Resize;
            FormClosing += ModernBackupInfoDialog_FormClosing;

            AcceptButton = buttonOk;

            ApplyDialogLayout();
        }

        private Size GetInitialClientSize(Size savedDialogSize)
        {
            if (savedDialogSize.Width >= ModernTheme.BackupInfoDialogMinimumClientSize.Width &&
                savedDialogSize.Height >= ModernTheme.BackupInfoDialogMinimumClientSize.Height)
            {
                return savedDialogSize;
            }

            return ModernTheme.BackupInfoDialogDefaultSize;
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

            panelModernTitleBar.MouseMove += ModernTitleBar_MouseMove;
            pictureBoxModernTitleIcon.MouseMove += ModernTitleBar_MouseMove;
            labelModernTitle.MouseMove += ModernTitleBar_MouseMove;

            panelModernTitleBar.MouseLeave += ModernTitleBar_MouseLeave;
            pictureBoxModernTitleIcon.MouseLeave += ModernTitleBar_MouseLeave;
            labelModernTitle.MouseLeave += ModernTitleBar_MouseLeave;

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

        private void InitializeBackupInfoLabel(string backupInfoText)
        {
            labelBackupInfo.AutoSize = false;
            labelBackupInfo.BorderStyle = BorderStyle.FixedSingle;
            labelBackupInfo.BackColor = ModernTheme.TitleBarBackColor;
            labelBackupInfo.ForeColor = ModernTheme.TextColor;
            labelBackupInfo.TextAlign = ContentAlignment.MiddleLeft;
            labelBackupInfo.Padding = new Padding(6, 0, 6, 0);
            labelBackupInfo.Text = backupInfoText;
        }

        private void InitializeBackupLogGrid(List<BackupLogEntry> logEntries)
        {
            dataGridViewBackupLog.AllowUserToAddRows = false;
            dataGridViewBackupLog.AllowUserToDeleteRows = false;
            dataGridViewBackupLog.AllowUserToResizeRows = false;
            dataGridViewBackupLog.RowHeadersVisible = false;
            dataGridViewBackupLog.MultiSelect = false;
            dataGridViewBackupLog.ReadOnly = true;
            dataGridViewBackupLog.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewBackupLog.BorderStyle = BorderStyle.FixedSingle;
            dataGridViewBackupLog.BackgroundColor = ModernTheme.TitleBarBackColor;
            dataGridViewBackupLog.GridColor = ModernTheme.ControlBackColor;
            dataGridViewBackupLog.EnableHeadersVisualStyles = false;
            dataGridViewBackupLog.ColumnHeadersDefaultCellStyle.BackColor = ModernTheme.ControlBackColor;
            dataGridViewBackupLog.ColumnHeadersDefaultCellStyle.ForeColor = ModernTheme.TextColor;
            dataGridViewBackupLog.ColumnHeadersDefaultCellStyle.SelectionBackColor = ModernTheme.ControlBackColor;
            dataGridViewBackupLog.ColumnHeadersDefaultCellStyle.SelectionForeColor = ModernTheme.TextColor;
            dataGridViewBackupLog.ColumnHeadersDefaultCellStyle.Font = new Font(ModernTheme.FontFamilyName, ModernTheme.HeaderFontSize, FontStyle.Bold);
            dataGridViewBackupLog.DefaultCellStyle.BackColor = ModernTheme.TitleBarBackColor;
            dataGridViewBackupLog.DefaultCellStyle.ForeColor = ModernTheme.TextColor;
            dataGridViewBackupLog.DefaultCellStyle.SelectionBackColor = ModernTheme.ControlBackColor;
            dataGridViewBackupLog.DefaultCellStyle.SelectionForeColor = ModernTheme.TextColor;
            dataGridViewBackupLog.AlternatingRowsDefaultCellStyle.BackColor = ModernTheme.WindowBackColor;
            dataGridViewBackupLog.AlternatingRowsDefaultCellStyle.ForeColor = ModernTheme.TextColor;
            dataGridViewBackupLog.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewBackupLog.ScrollBars = ScrollBars.None;
            dataGridViewBackupLog.Columns.Clear();

            DataGridViewImageColumn columnStatus = new DataGridViewImageColumn
            {
                Name = "ColumnLogStatus",
                HeaderText = "Status",
                Width = ModernTheme.BackupInfoDialogStatusColumnWidth,
                ImageLayout = DataGridViewImageCellLayout.Normal,
                SortMode = DataGridViewColumnSortMode.Automatic
            };

            DataGridViewTextBoxColumn columnDate = new DataGridViewTextBoxColumn
            {
                Name = "ColumnLogDate",
                HeaderText = "Datum",
                Width = ModernTheme.BackupInfoDialogDateColumnWidth,
                SortMode = DataGridViewColumnSortMode.Automatic
            };

            DataGridViewTextBoxColumn columnText = new DataGridViewTextBoxColumn
            {
                Name = "ColumnLogText",
                HeaderText = "Logtext",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = ModernTheme.BackupInfoDialogTextColumnMinimumWidth,
                SortMode = DataGridViewColumnSortMode.Automatic
            };

            dataGridViewBackupLog.Columns.Add(columnStatus);
            dataGridViewBackupLog.Columns.Add(columnDate);
            dataGridViewBackupLog.Columns.Add(columnText);

            if (logEntries.Count == 0)
            {
                int rowIndex = dataGridViewBackupLog.Rows.Add(CreateStatusIcon(BackupLogger.LogSeverityInfo), string.Empty, "No log entries found for this backup pair.");
                dataGridViewBackupLog.Rows[rowIndex].Cells[0].ToolTipText = BackupLogger.LogSeverityInfo;
                dataGridViewBackupLog.Rows[rowIndex].Cells[0].Tag = BackupLogger.LogSeverityInfo;
                dataGridViewBackupLog.Rows[rowIndex].Cells[1].Tag = DateTime.MinValue;
                return;
            }

            foreach (BackupLogEntry logEntry in logEntries)
            {
                int rowIndex = dataGridViewBackupLog.Rows.Add(
                    CreateStatusIcon(logEntry.Severity),
                    logEntry.Timestamp.ToString("dd.MM.yyyy HH:mm:ss"),
                    logEntry.Text);

                dataGridViewBackupLog.Rows[rowIndex].Cells[0].ToolTipText = logEntry.Severity;
                dataGridViewBackupLog.Rows[rowIndex].Cells[0].Tag = logEntry.Severity;
                dataGridViewBackupLog.Rows[rowIndex].Cells[1].Tag = logEntry.Timestamp;
            }
        }

        private Bitmap CreateStatusIcon(string severity)
        {
            Color color = GetStatusColor(severity);
            Bitmap bitmap = new Bitmap(16, 16);

            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Transparent);
            ModernTheme.DrawBackupInfoStatusIcon(graphics, new Rectangle(0, 0, 16, 16), color);

            return bitmap;
        }

        private Color GetStatusColor(string severity)
        {
            if (string.Equals(severity, BackupLogger.LogSeverityError, StringComparison.OrdinalIgnoreCase))
            {
                return ModernTheme.BackupInfoErrorColor;
            }

            if (string.Equals(severity, BackupLogger.LogSeverityWarning, StringComparison.OrdinalIgnoreCase))
            {
                return ModernTheme.BackupInfoWarningColor;
            }

            return ModernTheme.BackupInfoDefaultColor;
        }

        private void InitializeLogFilePathLabel()
        {
            labelLogFilePath.AutoSize = false;
            labelLogFilePath.BackColor = Color.Transparent;
            labelLogFilePath.ForeColor = ModernTheme.TextColor;
            labelLogFilePath.TextAlign = ContentAlignment.MiddleLeft;
            labelLogFilePath.Font = new Font(ModernTheme.FontFamilyName, ModernTheme.DefaultFontSize);
            labelLogFilePath.Text = BackupLogger.GetCurrentLogFilePath();
        }

        private void InitializeDialogButtons()
        {
            buttonOk.Text = "OK";
            buttonOk.DialogResult = DialogResult.OK;
            buttonOk.Size = ModernTheme.DialogButtonSize;
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

        private void ApplyDialogLayout()
        {
            int margin = ModernTheme.BackupInfoDialogMargin;
            int scrollBarSize = ModernTheme.DataGridViewScrollBarSize;

            labelBackupInfo.Location = new Point(margin, ModernTheme.BackupInfoDialogInfoTop);
            labelBackupInfo.Size = new Size(ClientSize.Width - (margin * 2), ModernTheme.BackupInfoDialogInfoHeight);

            ModernTheme.PositionSingleDialogButton(this, buttonOk, true);

            labelLogFilePath.Location = new Point(margin, buttonOk.Top);
            labelLogFilePath.Size = new Size(
                Math.Max(0, buttonOk.Left - margin - ModernTheme.BackupInfoDialogSpacing),
                buttonOk.Height);

            int gridTop = labelBackupInfo.Bottom + ModernTheme.BackupInfoDialogSpacing;
            int gridBottom = labelLogFilePath.Top - ModernTheme.BackupInfoDialogSpacing;
            int gridWidth = Math.Max(
                ModernTheme.BackupInfoDialogTextColumnMinimumWidth,
                ClientSize.Width - (margin * 2) - scrollBarSize);
            int gridHeight = Math.Max(
                ModernTheme.BackupInfoDialogMinimumGridHeight,
                gridBottom - gridTop - scrollBarSize);

            dataGridViewBackupLog.Location = new Point(margin, gridTop);
            dataGridViewBackupLog.Size = new Size(gridWidth, gridHeight);

            verticalScrollBarBackupLog.Location = new Point(dataGridViewBackupLog.Right, dataGridViewBackupLog.Top);
            verticalScrollBarBackupLog.Size = new Size(scrollBarSize, dataGridViewBackupLog.Height);

            horizontalScrollBarBackupLog.Location = new Point(dataGridViewBackupLog.Left, dataGridViewBackupLog.Bottom);
            horizontalScrollBarBackupLog.Size = new Size(dataGridViewBackupLog.Width, scrollBarSize);

            UpdateBackupLogScrollBars();
        }

        private void UpdateBackupLogScrollBars()
        {
            if (isUpdatingScrollBars)
            {
                return;
            }

            isUpdatingScrollBars = true;

            try
            {
                int visibleRowCount = Math.Max(1, (dataGridViewBackupLog.ClientSize.Height - dataGridViewBackupLog.ColumnHeadersHeight) / Math.Max(1, dataGridViewBackupLog.RowTemplate.Height));
                int maxFirstDisplayedRowIndex = Math.Max(0, dataGridViewBackupLog.Rows.Count - visibleRowCount);

                verticalScrollBarBackupLog.Minimum = 0;
                verticalScrollBarBackupLog.Maximum = maxFirstDisplayedRowIndex;
                verticalScrollBarBackupLog.LargeChange = visibleRowCount;
                verticalScrollBarBackupLog.Visible = maxFirstDisplayedRowIndex > 0;

                int firstDisplayedRowIndex = GetFirstDisplayedScrollingRowIndex();
                verticalScrollBarBackupLog.Value = Math.Min(maxFirstDisplayedRowIndex, Math.Max(0, firstDisplayedRowIndex));

                int totalColumnWidth = dataGridViewBackupLog.Columns
                    .Cast<DataGridViewColumn>()
                    .Where(column => column.Visible)
                    .Sum(column => column.Width);

                int maxHorizontalOffset = Math.Max(0, totalColumnWidth - dataGridViewBackupLog.ClientSize.Width);

                horizontalScrollBarBackupLog.Minimum = 0;
                horizontalScrollBarBackupLog.Maximum = maxHorizontalOffset;
                horizontalScrollBarBackupLog.LargeChange = dataGridViewBackupLog.ClientSize.Width;
                horizontalScrollBarBackupLog.Visible = maxHorizontalOffset > 0;
                horizontalScrollBarBackupLog.Value = Math.Min(maxHorizontalOffset, Math.Max(0, dataGridViewBackupLog.HorizontalScrollingOffset));
            }
            finally
            {
                isUpdatingScrollBars = false;
            }
        }

        private int GetFirstDisplayedScrollingRowIndex()
        {
            try
            {
                return dataGridViewBackupLog.FirstDisplayedScrollingRowIndex;
            }
            catch (InvalidOperationException)
            {
                return 0;
            }
        }

        private void verticalScrollBarBackupLog_ScrollValueChanged(object? sender, EventArgs e)
        {
            if (isUpdatingScrollBars || dataGridViewBackupLog.Rows.Count == 0)
            {
                return;
            }

            try
            {
                int value = Math.Min(verticalScrollBarBackupLog.Value, dataGridViewBackupLog.Rows.Count - 1);
                dataGridViewBackupLog.FirstDisplayedScrollingRowIndex = value;
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void horizontalScrollBarBackupLog_ScrollValueChanged(object? sender, EventArgs e)
        {
            if (isUpdatingScrollBars)
            {
                return;
            }

            dataGridViewBackupLog.HorizontalScrollingOffset = Math.Max(0, horizontalScrollBarBackupLog.Value);
        }

        private void dataGridViewBackupLog_Scroll(object? sender, ScrollEventArgs e)
        {
            UpdateBackupLogScrollBars();
        }

        private void dataGridViewBackupLog_ContentChanged(object? sender, EventArgs e)
        {
            UpdateBackupLogScrollBars();
        }

        private void dataGridViewBackupLog_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (verticalScrollBarBackupLog.Visible)
            {
                verticalScrollBarBackupLog.Value += e.Delta > 0 ? -1 : 1;
            }
        }

        private void dataGridViewBackupLog_SortCompare(object? sender, DataGridViewSortCompareEventArgs e)
        {
            if (e.Column.Name == "ColumnLogDate")
            {
                DateTime leftTimestamp = e.CellValue1 is string && dataGridViewBackupLog.Rows[e.RowIndex1].Cells[e.Column.Index].Tag is DateTime leftDate
                    ? leftDate
                    : DateTime.MinValue;
                DateTime rightTimestamp = e.CellValue2 is string && dataGridViewBackupLog.Rows[e.RowIndex2].Cells[e.Column.Index].Tag is DateTime rightDate
                    ? rightDate
                    : DateTime.MinValue;

                e.SortResult = leftTimestamp.CompareTo(rightTimestamp);
                e.Handled = true;
                return;
            }

            if (e.Column.Name == "ColumnLogStatus")
            {
                string leftSeverity = dataGridViewBackupLog.Rows[e.RowIndex1].Cells[e.Column.Index].Tag?.ToString() ?? string.Empty;
                string rightSeverity = dataGridViewBackupLog.Rows[e.RowIndex2].Cells[e.Column.Index].Tag?.ToString() ?? string.Empty;

                e.SortResult = GetSeveritySortValue(leftSeverity).CompareTo(GetSeveritySortValue(rightSeverity));
                e.Handled = true;
                return;
            }

            e.SortResult = string.Compare(e.CellValue1?.ToString(), e.CellValue2?.ToString(), StringComparison.OrdinalIgnoreCase);
            e.Handled = true;
        }

        private int GetSeveritySortValue(string severity)
        {
            if (string.Equals(severity, BackupLogger.LogSeverityError, StringComparison.OrdinalIgnoreCase))
            {
                return 3;
            }

            if (string.Equals(severity, BackupLogger.LogSeverityWarning, StringComparison.OrdinalIgnoreCase))
            {
                return 2;
            }

            return 1;
        }

        private void ModernBackupInfoDialog_Resize(object? sender, EventArgs e)
        {
            ApplyDialogLayout();
        }

        private void ModernBackupInfoDialog_FormClosing(object? sender, FormClosingEventArgs e)
        {
            DialogSize = ClientSize;
        }

        private void ModernTitleBar_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (sender is Control control)
            {
                Point cursorPosition = PointToClient(control.PointToScreen(e.Location));

                if (ModernWindowFrame.TryGetResizeHitTestResult(ClientSize, cursorPosition, out int resizeHitTestResult))
                {
                    ReleaseCapture();
                    SendMessage(Handle, ModernWindowFrame.WmNclButtonDown, resizeHitTestResult, 0);
                    return;
                }
            }

            ReleaseCapture();
            SendMessage(Handle, ModernWindowFrame.WmNclButtonDown, ModernWindowFrame.HtCaption, 0);
        }

        private void ModernTitleBar_MouseMove(object? sender, MouseEventArgs e)
        {
            if (sender is not Control control)
            {
                Cursor = Cursors.Default;
                return;
            }

            Point cursorPosition = PointToClient(control.PointToScreen(e.Location));

            Cursor = ModernWindowFrame.TryGetResizeHitTestResult(ClientSize, cursorPosition, out int resizeHitTestResult)
                ? ModernWindowFrame.GetResizeCursor(resizeHitTestResult)
                : Cursors.Default;
        }

        private void ModernTitleBar_MouseLeave(object? sender, EventArgs e)
        {
            Cursor = Cursors.Default;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            ModernTheme.DrawResizeGrip(e.Graphics, ClientSize);
        }

        protected override void WndProc(ref Message m)
        {
            if (ModernWindowFrame.HandleResizeHitTest(this, ref m))
            {
                return;
            }

            base.WndProc(ref m);
        }
    }
}
