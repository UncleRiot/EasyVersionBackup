// Design-Rule / UI consistency:
// Keep layout, spacing, colors, sizes, and fonts aligned with ModernTheme.
// Add new shared visual values to ModernTheme instead of hardcoding local exceptions here.
// 03.05.2026 /dc

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace EasyVersionBackup
{
    public partial class VersionInputForm : Form
    {
        public List<BackupVersionItem> ResultItems { get; private set; }
        public bool IgnoreCopyErrors { get; private set; }
        private readonly ToolTip _versionInputToolTip = new ToolTip();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        public VersionInputForm(List<BackupVersionItem> items, bool ignoreCopyErrors)
        {
            InitializeComponent();

            Text = "Backup Version";
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            FormBorderStyle = FormBorderStyle.None;
            BackColor = ModernTheme.WindowBackColor;
            Font = new Font(ModernTheme.FontFamilyName, ModernTheme.DefaultFontSize);
            DoubleBuffered = true;
            ModernWindowFrame.Apply(this);

            OffsetExistingControlsForModernTitleBar(ModernTheme.TitleBarHeight);
            InitializeModernTitleBar();

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

            StyleModernButton(buttonOk, ModernTheme.AccentColor, ModernTheme.DarkTextColor, false, ModernTheme.DialogPrimaryButtonTextPadding);
            StyleModernButton(buttonCancel, ModernTheme.ControlBackColor, ModernTheme.TextColor, true, ModernTheme.DialogSecondaryButtonTextPadding);

            ResultItems = new List<BackupVersionItem>();
            checkBoxIgnoreCopyErrors.Checked = ignoreCopyErrors;

            dataGridViewVersions.Rows.Clear();

            foreach (BackupVersionItem item in items)
            {
                dataGridViewVersions.Rows.Add(item.SourceName, item.Version);
            }

            int rowCount = Math.Min(items.Count, 3);
            int rowHeight = dataGridViewVersions.RowTemplate.Height;

            dataGridViewVersions.Height =
                dataGridViewVersions.ColumnHeadersHeight +
                (rowCount * rowHeight) + 2;

            Width = 380;
            dataGridViewVersions.Width = Width - 24;

            buttonCancel.Left = ClientSize.Width - buttonCancel.Width - 12;
            buttonOk.Left = buttonCancel.Left - buttonOk.Width - 8;

            Height = buttonOk.Bottom + 12;

            PositionBottomRight();
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

            const int wmNclbuttondown = 0xA1;
            const int htCaption = 0x2;

            ReleaseCapture();
            SendMessage(Handle, wmNclbuttondown, htCaption, 0);
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

                if (!string.IsNullOrWhiteSpace(version) && !VersionHelper.IsValidVersion(version))
                {
                    ModernMessageDialog.Show(
                        this,
                        "Error",
                        $"Invalid version for '{sourceName}'. The value must be usable as part of a file name.");

                    return;
                }

                ResultItems.Add(new BackupVersionItem
                {
                    SourceName = sourceName,
                    Version = version
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