using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace EasyVersionBackup
{
    public partial class VersionInputForm : Form
    {
        public List<BackupVersionItem> ResultItems { get; private set; }
        public bool IgnoreCopyErrors { get; private set; }

        public VersionInputForm(List<BackupVersionItem> items, bool ignoreCopyErrors)
        {
            InitializeComponent();

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

            Height = buttonOk.Bottom + 60;

            PositionBottomRight();
        }

        private void PositionBottomRight()
        {
            Screen screen = Screen.PrimaryScreen;
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
                    MessageBox.Show($"Invalid version for '{sourceName}'. The value must be usable as part of a file name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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