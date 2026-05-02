using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EasyVersionBackup
{
    public class ExclusionsDialog : Form
    {
        private readonly DataGridView dataGridViewExclusions = new DataGridView();
        private readonly ToolTip toolTipExclusions = new ToolTip();
        private readonly Button buttonAddExclusion = new Button();
        private readonly Button buttonRemoveExclusion = new Button();
        private readonly Button buttonOk = new Button();
        private readonly Button buttonCancel = new Button();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        public List<string> ResultExcludedPaths { get; private set; } = new List<string>();

        public ExclusionsDialog(List<string> excludedPaths)
        {
            Text = "Exclusions";
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.None;
            ClientSize = new Size(520, 353);
            BackColor = ModernTheme.WindowBackColor;
            Font = new Font(ModernTheme.FontFamilyName, ModernTheme.DefaultFontSize);
            DoubleBuffered = true;
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            ModernWindowFrame.Apply(this);

            InitializeModernTitleBar();
            InitializeToolbarButtons();
            InitializeExclusionsGrid();
            InitializeDialogButtons();

            foreach (string excludedPath in excludedPaths)
            {
                dataGridViewExclusions.Rows.Add(excludedPath);
            }

            Controls.Add(buttonAddExclusion);
            Controls.Add(buttonRemoveExclusion);
            Controls.Add(dataGridViewExclusions);
            Controls.Add(buttonOk);
            Controls.Add(buttonCancel);

            AcceptButton = buttonOk;
            CancelButton = buttonCancel;
        }
        private void InitializeModernTitleBar()
        {
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
                Image = Icon?.ToBitmap(),
                BackColor = Color.Transparent
            };

            Label labelModernTitle = new Label
            {
                Name = "labelModernTitle",
                Text = Text,
                AutoSize = false,
                Location = new Point(30, 0),
                Size = new Size(ClientSize.Width - 66, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = ModernTheme.TextColor,
                Font = new Font(ModernTheme.FontFamilyName, ModernTheme.TitleFontSize, FontStyle.Regular),
                BackColor = Color.Transparent
            };

            Button buttonModernClose = CreateModernTitleBarButton("buttonModernClose", new Point(ClientSize.Width - 36, 0));
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
        private void InitializeToolbarButtons()
        {
            buttonAddExclusion.Name = "buttonAddExclusion";
            buttonAddExclusion.Text = "+";
            buttonAddExclusion.Size = ModernTheme.ToolbarButtonSize;
            buttonAddExclusion.Location = new Point(12, 44);
            buttonAddExclusion.FlatStyle = FlatStyle.Flat;
            buttonAddExclusion.BackColor = ModernTheme.ControlBackColor;
            buttonAddExclusion.ForeColor = ModernTheme.TextColor;
            buttonAddExclusion.Cursor = Cursors.Hand;
            buttonAddExclusion.TextAlign = ContentAlignment.MiddleCenter;
            buttonAddExclusion.Padding = ModernTheme.ToolbarPlusButtonTextPadding;
            buttonAddExclusion.UseCompatibleTextRendering = true;
            buttonAddExclusion.UseVisualStyleBackColor = false;
            buttonAddExclusion.FlatAppearance.BorderColor = ModernTheme.AccentColor;
            buttonAddExclusion.FlatAppearance.BorderSize = 1;
            buttonAddExclusion.FlatAppearance.MouseOverBackColor = ModernTheme.ControlHoverBackColor;
            buttonAddExclusion.FlatAppearance.MouseDownBackColor = ModernTheme.AccentColor;
            buttonAddExclusion.Click += buttonAddExclusion_Click;

            buttonRemoveExclusion.Name = "buttonRemoveExclusion";
            buttonRemoveExclusion.Text = "−";
            buttonRemoveExclusion.Size = ModernTheme.ToolbarButtonSize;
            buttonRemoveExclusion.Location = new Point(46, 44);
            buttonRemoveExclusion.FlatStyle = FlatStyle.Flat;
            buttonRemoveExclusion.BackColor = ModernTheme.ControlBackColor;
            buttonRemoveExclusion.ForeColor = ModernTheme.TextColor;
            buttonRemoveExclusion.Cursor = Cursors.Hand;
            buttonRemoveExclusion.TextAlign = ContentAlignment.MiddleCenter;
            buttonRemoveExclusion.Padding = ModernTheme.ToolbarMinusButtonTextPadding;
            buttonRemoveExclusion.UseCompatibleTextRendering = true;
            buttonRemoveExclusion.UseVisualStyleBackColor = false;
            buttonRemoveExclusion.FlatAppearance.BorderColor = ModernTheme.AccentColor;
            buttonRemoveExclusion.FlatAppearance.BorderSize = 1;
            buttonRemoveExclusion.FlatAppearance.MouseOverBackColor = ModernTheme.ControlHoverBackColor;
            buttonRemoveExclusion.FlatAppearance.MouseDownBackColor = ModernTheme.AccentColor;
            buttonRemoveExclusion.Click += buttonRemoveExclusion_Click;

            toolTipExclusions.SetToolTip(buttonAddExclusion, "Add exclusion");
            toolTipExclusions.SetToolTip(buttonRemoveExclusion, "Remove selected exclusion");
        }

        private void InitializeExclusionsGrid()
        {
            dataGridViewExclusions.Name = "dataGridViewExclusions";
            dataGridViewExclusions.AllowUserToAddRows = false;
            dataGridViewExclusions.AllowUserToDeleteRows = false;
            dataGridViewExclusions.AllowUserToResizeRows = false;
            dataGridViewExclusions.RowHeadersVisible = false;
            dataGridViewExclusions.MultiSelect = false;
            dataGridViewExclusions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewExclusions.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
            dataGridViewExclusions.Location = new Point(12, 78);
            dataGridViewExclusions.Size = new Size(496, 229);
            dataGridViewExclusions.BackgroundColor = ModernTheme.WindowBackColor;
            dataGridViewExclusions.BorderStyle = BorderStyle.None;
            dataGridViewExclusions.GridColor = ModernTheme.ControlBackColor;
            dataGridViewExclusions.EnableHeadersVisualStyles = false;
            dataGridViewExclusions.ColumnHeadersDefaultCellStyle.BackColor = ModernTheme.ControlBackColor;
            dataGridViewExclusions.ColumnHeadersDefaultCellStyle.ForeColor = ModernTheme.TextColor;
            dataGridViewExclusions.ColumnHeadersDefaultCellStyle.SelectionBackColor = ModernTheme.ControlBackColor;
            dataGridViewExclusions.ColumnHeadersDefaultCellStyle.SelectionForeColor = ModernTheme.TextColor;
            dataGridViewExclusions.ColumnHeadersDefaultCellStyle.Font = new Font(ModernTheme.FontFamilyName, ModernTheme.HeaderFontSize, FontStyle.Bold);
            dataGridViewExclusions.DefaultCellStyle.BackColor = ModernTheme.TitleBarBackColor;
            dataGridViewExclusions.DefaultCellStyle.ForeColor = ModernTheme.TextColor;
            dataGridViewExclusions.DefaultCellStyle.SelectionBackColor = ModernTheme.AccentColor;
            dataGridViewExclusions.DefaultCellStyle.SelectionForeColor = ModernTheme.DarkTextColor;
            dataGridViewExclusions.AlternatingRowsDefaultCellStyle.BackColor = ModernTheme.WindowBackColor;
            dataGridViewExclusions.AlternatingRowsDefaultCellStyle.ForeColor = ModernTheme.TextColor;

            DataGridViewTextBoxColumn columnExcludedPath = new DataGridViewTextBoxColumn
            {
                HeaderText = "Excluded Path",
                Name = "ColumnExcludedPath",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ToolTipText = "Excluded source path"
            };

            dataGridViewExclusions.Columns.Add(columnExcludedPath);
        }

        private void InitializeDialogButtons()
        {
            buttonOk.Text = "OK";
            buttonOk.DialogResult = DialogResult.OK;
            buttonOk.TextAlign = ContentAlignment.MiddleCenter;
            buttonOk.Padding = ModernTheme.DialogPrimaryButtonTextPadding;
            buttonOk.UseCompatibleTextRendering = true;
            buttonOk.Location = new Point(352, 314);
            buttonOk.Size = ModernTheme.DialogButtonSize;
            buttonOk.FlatStyle = FlatStyle.Flat;
            buttonOk.BackColor = ModernTheme.AccentColor;
            buttonOk.ForeColor = ModernTheme.DarkTextColor;
            buttonOk.Cursor = Cursors.Hand;
            buttonOk.UseVisualStyleBackColor = false;
            buttonOk.FlatAppearance.BorderSize = 0;
            buttonOk.FlatAppearance.MouseOverBackColor = ModernTheme.AccentHoverColor;
            buttonOk.FlatAppearance.MouseDownBackColor = ModernTheme.ControlBackColor;
            buttonOk.Click += buttonOk_Click;

            buttonCancel.Text = "Cancel";
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.TextAlign = ContentAlignment.MiddleCenter;
            buttonCancel.Padding = ModernTheme.DialogSecondaryButtonTextPadding;
            buttonCancel.UseCompatibleTextRendering = true;
            buttonCancel.Location = new Point(433, 314);
            buttonCancel.Size = ModernTheme.DialogButtonSize;
            buttonCancel.FlatStyle = FlatStyle.Flat;
            buttonCancel.BackColor = ModernTheme.ControlBackColor;
            buttonCancel.ForeColor = ModernTheme.TextColor;
            buttonCancel.Cursor = Cursors.Hand;
            buttonCancel.UseVisualStyleBackColor = false;
            buttonCancel.FlatAppearance.BorderColor = ModernTheme.AccentColor;
            buttonCancel.FlatAppearance.BorderSize = 1;
            buttonCancel.FlatAppearance.MouseOverBackColor = ModernTheme.ControlHoverBackColor;
            buttonCancel.FlatAppearance.MouseDownBackColor = ModernTheme.AccentColor;
        }

        private void buttonAddExclusion_Click(object? sender, EventArgs e)
        {
            int rowIndex = dataGridViewExclusions.Rows.Add(string.Empty);

            dataGridViewExclusions.ClearSelection();
            dataGridViewExclusions.Rows[rowIndex].Selected = true;
            dataGridViewExclusions.CurrentCell = dataGridViewExclusions.Rows[rowIndex].Cells["ColumnExcludedPath"];
            dataGridViewExclusions.BeginEdit(true);
        }

        private void buttonRemoveExclusion_Click(object? sender, EventArgs e)
        {
            if (dataGridViewExclusions.CurrentRow == null)
            {
                return;
            }

            if (dataGridViewExclusions.CurrentRow.IsNewRow)
            {
                return;
            }

            string excludedPath = dataGridViewExclusions.CurrentRow.Cells["ColumnExcludedPath"].Value?.ToString()?.Trim() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(excludedPath))
            {
                DialogResult result = ModernConfirmationDialog.Show(
                    this,
                    "Remove exclusion",
                    "The selected exclusion contains data. Do you really want to remove it?");

                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            dataGridViewExclusions.Rows.Remove(dataGridViewExclusions.CurrentRow);
        }

        private void buttonOk_Click(object? sender, EventArgs e)
        {
            ResultExcludedPaths = dataGridViewExclusions.Rows
                .Cast<DataGridViewRow>()
                .Where(row => !row.IsNewRow)
                .Select(row => row.Cells["ColumnExcludedPath"].Value?.ToString()?.Trim() ?? string.Empty)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToList();

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}