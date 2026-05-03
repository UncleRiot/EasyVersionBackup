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
            InitializeExclusionHintIcon();
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
            buttonRemoveExclusion.Location = new Point(buttonAddExclusion.Right + ModernTheme.ToolbarButtonSpacing, 44);
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
            dataGridViewExclusions.CellValidating += dataGridViewExclusions_CellValidating;
            dataGridViewExclusions.CellEndEdit += dataGridViewExclusions_CellEndEdit;

            DataGridViewTextBoxColumn columnExcludedPath = new DataGridViewTextBoxColumn
            {
                HeaderText = "Exclusion-List",
                Name = "ColumnExcludedPath",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ToolTipText = "Excluded source path, folder name or wildcard pattern"
            };

            dataGridViewExclusions.Columns.Add(columnExcludedPath);
        }

        private void InitializeExclusionHintIcon()
        {
            PictureBox pictureBoxExclusionListHint = CreateExclusionHintIcon(
                "pictureBoxExclusionListHint",
                "Exclusion examples:" + Environment.NewLine +
                "bin = exclude every folder or file named bin" + Environment.NewLine +
                "bin\\ = exclude every folder named bin" + Environment.NewLine +
                "*.tmp = exclude matching files" + Environment.NewLine +
                "d:\\source\\ = exclude this absolute folder path" + Environment.NewLine +
                "file*xyz.exe = exclude matching file names" + Environment.NewLine +
                "file.* = exclude matching file names" + Environment.NewLine +
                "* = wildcard for any characters inside a name" + Environment.NewLine +
                Environment.NewLine +
                "Invalid Windows characters:" + Environment.NewLine +
                "< > \" / | ?");

            pictureBoxExclusionListHint.Left = dataGridViewExclusions.Left + 110;
            pictureBoxExclusionListHint.Top = dataGridViewExclusions.Top + 6;
            pictureBoxExclusionListHint.BackColor = dataGridViewExclusions.ColumnHeadersDefaultCellStyle.BackColor;

            Controls.Add(pictureBoxExclusionListHint);
            pictureBoxExclusionListHint.BringToFront();
        }

        private PictureBox CreateExclusionHintIcon(string name, string hintText)
        {
            PictureBox pictureBox = new PictureBox
            {
                Name = name,
                Size = new Size(18, 18),
                Image = CreateExclusionHintIconBitmap(),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Cursor = Cursors.Help
            };

            toolTipExclusions.SetToolTip(pictureBox, hintText);

            pictureBox.Click += (sender, e) =>
            {
                if (pictureBox.IsDisposed || Disposing || IsDisposed)
                {
                    return;
                }

                toolTipExclusions.Hide(pictureBox);
                toolTipExclusions.Show(hintText, pictureBox, pictureBox.Width + 5, 0, 8000);
            };

            return pictureBox;
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            toolTipExclusions.Active = false;
            toolTipExclusions.RemoveAll();

            base.OnFormClosing(e);
        }

        private Bitmap CreateExclusionHintIconBitmap()
        {
            Bitmap bitmap = new Bitmap(18, 18);

            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Transparent);

            using SolidBrush brush = new SolidBrush(ModernTheme.AccentColor);
            graphics.FillEllipse(brush, 1, 1, 16, 16);

            using Font font = new Font(ModernTheme.FontFamilyName, 8F, FontStyle.Regular);
            Size textSize = TextRenderer.MeasureText("?", font);

            int x = (18 - textSize.Width) / 2 + 5;
            int y = (18 - textSize.Height) / 2;

            TextRenderer.DrawText(
                graphics,
                "?",
                font,
                new Point(x, y),
                ModernTheme.DarkTextColor,
                TextFormatFlags.NoPadding);

            return bitmap;
        }

        private void InitializeDialogButtons()
        {
            buttonOk.Text = "OK";
            buttonOk.DialogResult = DialogResult.None;
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

        private void dataGridViewExclusions_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex < 0 || dataGridViewExclusions.Columns[e.ColumnIndex].Name != "ColumnExcludedPath")
            {
                return;
            }

            string value = e.FormattedValue?.ToString()?.Trim() ?? string.Empty;
            string validationError = GetExclusionValidationError(value);

            dataGridViewExclusions.Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = validationError;
            e.Cancel = !string.IsNullOrEmpty(validationError);
        }

        private void dataGridViewExclusions_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dataGridViewExclusions.Columns[e.ColumnIndex].Name != "ColumnExcludedPath")
            {
                return;
            }

            string value = dataGridViewExclusions.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString()?.Trim() ?? string.Empty;
            dataGridViewExclusions.Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = GetExclusionValidationError(value);
        }

        private void buttonOk_Click(object? sender, EventArgs e)
        {
            if (!ValidateAllExclusions())
            {
                return;
            }

            ResultExcludedPaths = dataGridViewExclusions.Rows
                .Cast<DataGridViewRow>()
                .Where(row => !row.IsNewRow)
                .Select(row => row.Cells["ColumnExcludedPath"].Value?.ToString()?.Trim() ?? string.Empty)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToList();

            DialogResult = DialogResult.OK;
            Close();
        }

        private bool ValidateAllExclusions()
        {
            foreach (DataGridViewRow row in dataGridViewExclusions.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                string value = row.Cells["ColumnExcludedPath"].Value?.ToString()?.Trim() ?? string.Empty;
                string validationError = GetExclusionValidationError(value);

                row.Cells["ColumnExcludedPath"].ErrorText = validationError;

                if (!string.IsNullOrEmpty(validationError))
                {
                    dataGridViewExclusions.ClearSelection();
                    row.Selected = true;
                    dataGridViewExclusions.CurrentCell = row.Cells["ColumnExcludedPath"];

                    ModernMessageDialog.Show(
                        this,
                        "Invalid exclusion",
                        validationError);

                    return false;
                }
            }

            return true;
        }

        private string GetExclusionValidationError(string exclusion)
        {
            if (string.IsNullOrWhiteSpace(exclusion))
            {
                return string.Empty;
            }

            string value = exclusion.Trim();

            if (value == "." || value == "..")
            {
                return "Invalid exclusion. Relative navigation entries are not allowed.";
            }

            if (value.Contains('/'))
            {
                return "Invalid exclusion. Use Windows backslash \\ instead of /.";
            }

            if (value.Contains('?'))
            {
                return "Invalid exclusion. Use * as wildcard. ? is not supported.";
            }

            if (value.IndexOfAny(new[] { '<', '>', '"', '|' }) >= 0)
            {
                return "Invalid exclusion. The Windows characters < > \" | are not allowed.";
            }

            if (value.Any(char.IsControl))
            {
                return "Invalid exclusion. Control characters are not allowed.";
            }

            int colonIndex = value.IndexOf(':');

            if (colonIndex >= 0)
            {
                if (colonIndex != 1 || value.Length < 3 || !char.IsLetter(value[0]) || value[2] != '\\')
                {
                    return "Invalid exclusion. A drive path must look like d:\\source\\.";
                }

                if (value.IndexOf(':', colonIndex + 1) >= 0)
                {
                    return "Invalid exclusion. Only one drive separator is allowed.";
                }
            }

            string valueWithoutTrailingSlash = value.TrimEnd('\\');

            if (string.IsNullOrWhiteSpace(valueWithoutTrailingSlash))
            {
                return "Invalid exclusion. A root separator alone is not allowed.";
            }

            if (valueWithoutTrailingSlash.All(character => character == '*'))
            {
                return "Invalid exclusion. A wildcard-only exclusion would exclude everything.";
            }

            string[] parts = valueWithoutTrailingSlash.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                if (part == "." || part == "..")
                {
                    return "Invalid exclusion. Relative navigation entries are not allowed.";
                }

                if (part.EndsWith(" ", StringComparison.Ordinal) || part.EndsWith(".", StringComparison.Ordinal))
                {
                    return "Invalid exclusion. Windows names must not end with a space or dot.";
                }

                if (part.All(character => character == '*'))
                {
                    return "Invalid exclusion. Wildcard-only path parts are not allowed.";
                }
            }

            return string.Empty;
        }
    }
}