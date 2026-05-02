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

        public List<string> ResultExcludedPaths { get; private set; } = new List<string>();

        public ExclusionsDialog(List<string> excludedPaths)
        {
            Text = "Exclusions";
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            ClientSize = new Size(520, 321);

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

        private void InitializeToolbarButtons()
        {
            buttonAddExclusion.Name = "buttonAddExclusion";
            buttonAddExclusion.Text = "+";
            buttonAddExclusion.Size = new Size(28, 26);
            buttonAddExclusion.Location = new Point(12, 12);
            buttonAddExclusion.FlatStyle = FlatStyle.Flat;
            buttonAddExclusion.BackColor = Color.White;
            buttonAddExclusion.ForeColor = Color.Black;
            buttonAddExclusion.Cursor = Cursors.Hand;
            buttonAddExclusion.TextAlign = ContentAlignment.MiddleCenter;
            buttonAddExclusion.Padding = new Padding(0, 0, 0, 2);
            buttonAddExclusion.UseVisualStyleBackColor = false;
            buttonAddExclusion.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 210);
            buttonAddExclusion.FlatAppearance.BorderSize = 1;
            buttonAddExclusion.FlatAppearance.MouseOverBackColor = Color.FromArgb(245, 245, 245);
            buttonAddExclusion.FlatAppearance.MouseDownBackColor = Color.FromArgb(230, 230, 230);
            buttonAddExclusion.Click += buttonAddExclusion_Click;

            buttonRemoveExclusion.Name = "buttonRemoveExclusion";
            buttonRemoveExclusion.Text = "−";
            buttonRemoveExclusion.Size = new Size(28, 26);
            buttonRemoveExclusion.Location = new Point(46, 12);
            buttonRemoveExclusion.FlatStyle = FlatStyle.Flat;
            buttonRemoveExclusion.BackColor = Color.White;
            buttonRemoveExclusion.ForeColor = Color.Black;
            buttonRemoveExclusion.Cursor = Cursors.Hand;
            buttonRemoveExclusion.TextAlign = ContentAlignment.MiddleCenter;
            buttonRemoveExclusion.Padding = new Padding(0, 0, 0, 2);
            buttonRemoveExclusion.UseVisualStyleBackColor = false;
            buttonRemoveExclusion.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 210);
            buttonRemoveExclusion.FlatAppearance.BorderSize = 1;
            buttonRemoveExclusion.FlatAppearance.MouseOverBackColor = Color.FromArgb(245, 245, 245);
            buttonRemoveExclusion.FlatAppearance.MouseDownBackColor = Color.FromArgb(230, 230, 230);
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
            dataGridViewExclusions.Location = new Point(12, 46);
            dataGridViewExclusions.Size = new Size(496, 229);
            dataGridViewExclusions.BackgroundColor = Color.White;
            dataGridViewExclusions.BorderStyle = BorderStyle.None;
            dataGridViewExclusions.EnableHeadersVisualStyles = false;
            dataGridViewExclusions.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            dataGridViewExclusions.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewExclusions.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 235, 252);
            dataGridViewExclusions.DefaultCellStyle.SelectionForeColor = Color.Black;

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
            buttonOk.Padding = Padding.Empty;
            buttonOk.Location = new Point(352, 282);
            buttonOk.Size = new Size(75, 27);
            buttonOk.Click += buttonOk_Click;

            buttonCancel.Text = "Cancel";
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.TextAlign = ContentAlignment.MiddleCenter;
            buttonCancel.Padding = Padding.Empty;
            buttonCancel.Location = new Point(433, 282);
            buttonCancel.Size = new Size(75, 27);
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
                DialogResult result = MessageBox.Show(
                    "The selected exclusion contains data. Do you really want to remove it?",
                    "Remove exclusion",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

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