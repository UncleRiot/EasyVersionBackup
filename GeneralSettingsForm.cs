using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace EasyVersionBackup
{
    public partial class GeneralSettingsForm : Form
    {
        public AppSettings ResultSettings { get; private set; }

        public GeneralSettingsForm(AppSettings settings)
        {
            InitializeComponent();

            InitializeAutoBackupControls();

            dataGridViewPaths.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;
            dataGridViewPaths.KeyDown += dataGridViewPaths_KeyDown;

            ResultSettings = CloneSettings(settings);
            ApplySettingsToUi(ResultSettings);
        }

        private void checkBoxDummy1_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateAutoBackupControls();
        }
        private void UpdateAutoBackupControls()
        {
            TextBox textBoxAutoBackupInterval = GetAutoBackupIntervalTextBox();

            textBoxAutoBackupInterval.Enabled = checkBoxDummy1.Checked;
            checkBoxIgnoreCopyErrors.Enabled = !checkBoxDummy1.Checked;

            if (checkBoxDummy1.Checked)
            {
                checkBoxIgnoreCopyErrors.Checked = true;
            }
        }
        private TextBox GetAutoBackupIntervalTextBox()
        {
            foreach (Control control in Controls)
            {
                if (control.Name == "textBoxAutoBackupInterval" && control is TextBox textBox)
                {
                    return textBox;
                }
            }

            throw new InvalidOperationException("textBoxAutoBackupInterval not found.");
        }
        private void InitializeAutoBackupControls()
        {
            labelDummy1.Text = "Auto-Backup (min)";
            checkBoxDummy1.CheckedChanged += checkBoxDummy1_CheckedChanged;

            TextBox textBoxAutoBackupInterval = new TextBox
            {
                Name = "textBoxAutoBackupInterval",
                Size = new System.Drawing.Size(45, 23),
                TabIndex = 13
            };

            // exakt vertikal mittig zur Checkbox ausrichten
            textBoxAutoBackupInterval.Left = checkBoxDummy1.Right + 5;
            textBoxAutoBackupInterval.Top = checkBoxDummy1.Top + (checkBoxDummy1.Height - textBoxAutoBackupInterval.Height) / 2;

            Controls.Add(textBoxAutoBackupInterval);
        }
        private void dataGridViewPaths_KeyDown(object? sender, KeyEventArgs e)
        {
            if (!e.Control || e.KeyCode != Keys.C)
            {
                return;
            }

            List<int> selectedRowIndexes = new List<int>();

            foreach (DataGridViewCell cell in dataGridViewPaths.SelectedCells)
            {
                if (cell.RowIndex < 0 || cell.RowIndex >= dataGridViewPaths.Rows.Count)
                {
                    continue;
                }

                if (dataGridViewPaths.Rows[cell.RowIndex].IsNewRow)
                {
                    continue;
                }

                if (!selectedRowIndexes.Contains(cell.RowIndex))
                {
                    selectedRowIndexes.Add(cell.RowIndex);
                }
            }

            foreach (DataGridViewRow row in dataGridViewPaths.SelectedRows)
            {
                if (row.Index < 0 || row.Index >= dataGridViewPaths.Rows.Count)
                {
                    continue;
                }

                if (row.IsNewRow)
                {
                    continue;
                }

                if (!selectedRowIndexes.Contains(row.Index))
                {
                    selectedRowIndexes.Add(row.Index);
                }
            }

            if (selectedRowIndexes.Count == 0)
            {
                return;
            }

            selectedRowIndexes.Sort();

            List<string> lines = new List<string>();

            foreach (int rowIndex in selectedRowIndexes)
            {
                DataGridViewRow row = dataGridViewPaths.Rows[rowIndex];

                string sourceDirectory = row.Cells["ColumnSourceDirectory"].Value?.ToString() ?? string.Empty;
                string targetDirectory = row.Cells["ColumnTargetDirectory"].Value?.ToString() ?? string.Empty;

                lines.Add(sourceDirectory + "\t" + targetDirectory);
            }

            Clipboard.SetText(string.Join(Environment.NewLine, lines));

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void ApplySettingsToUi(AppSettings settings)
        {
            checkBoxZipDestinationFiles.Checked = settings.ZipDestinationFiles;

            comboBoxDefaultVersioning.Items.Clear();
            comboBoxDefaultVersioning.Items.Add("none");
            comboBoxDefaultVersioning.Items.Add("0.0.1");

            if (comboBoxDefaultVersioning.Items.Contains(settings.DefaultVersioning))
            {
                comboBoxDefaultVersioning.SelectedItem = settings.DefaultVersioning;
            }
            else
            {
                comboBoxDefaultVersioning.SelectedItem = "0.0.1";
            }

            checkBoxAutoIncrementVersion.Checked = settings.AutoIncrementVersion;
            checkBoxMinimizeToSystray.Checked = settings.MinimizeToSystray;
            checkBoxIgnoreCopyErrors.Checked = settings.IgnoreCopyErrors;
            checkBoxDummy1.Checked = settings.AutoBackupEnabled;

            TextBox textBoxAutoBackupInterval = Controls.Find("textBoxAutoBackupInterval", true).OfType<TextBox>().First();
            textBoxAutoBackupInterval.Text = Math.Clamp(settings.AutoBackupIntervalMinutes, 1, 60).ToString();

            UpdateAutoBackupControls();

            dataGridViewPaths.Rows.Clear();

            foreach (BackupPathPair pair in settings.BackupPathPairs)
            {
                dataGridViewPaths.Rows.Add(
                    pair.IsEnabled,
                    pair.SourceDirectory,
                    "Browse...",
                    pair.TargetDirectory,
                    "Browse...");
            }
        }

        private AppSettings ReadSettingsFromUi()
        {
            AppSettings settings = CloneSettings(ResultSettings);

            TextBox textBoxAutoBackupInterval = GetAutoBackupIntervalTextBox();

            settings.ZipDestinationFiles = checkBoxZipDestinationFiles.Checked;
            settings.DefaultVersioning = comboBoxDefaultVersioning.SelectedItem?.ToString() ?? "0.0.1";
            settings.MinimizeToSystray = checkBoxMinimizeToSystray.Checked;
            settings.AutoIncrementVersion = checkBoxAutoIncrementVersion.Checked;
            settings.AutoBackupEnabled = checkBoxDummy1.Checked;
            settings.AutoBackupIntervalMinutes = int.Parse(textBoxAutoBackupInterval.Text.Trim());
            settings.IgnoreCopyErrors = checkBoxDummy1.Checked || checkBoxIgnoreCopyErrors.Checked;
            settings.BackupPathPairs = new List<BackupPathPair>();

            foreach (DataGridViewRow row in dataGridViewPaths.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                bool isEnabled = false;
                object? enabledValue = row.Cells["ColumnIsEnabled"].Value;

                if (enabledValue != null)
                {
                    bool.TryParse(enabledValue.ToString(), out isEnabled);
                }

                string sourceDirectory = row.Cells["ColumnSourceDirectory"].Value?.ToString()?.Trim() ?? string.Empty;
                string targetDirectory = row.Cells["ColumnTargetDirectory"].Value?.ToString()?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(sourceDirectory) && string.IsNullOrWhiteSpace(targetDirectory))
                {
                    continue;
                }

                settings.BackupPathPairs.Add(new BackupPathPair
                {
                    IsEnabled = isEnabled,
                    SourceDirectory = sourceDirectory,
                    TargetDirectory = targetDirectory
                });
            }

            return settings;
        }

        private void buttonAddRow_Click(object sender, EventArgs e)
        {
            dataGridViewPaths.Rows.Add(true, string.Empty, "Browse...", string.Empty, "Browse...");
        }

        private void buttonRemoveRow_Click(object sender, EventArgs e)
        {
            if (dataGridViewPaths.CurrentRow == null || dataGridViewPaths.CurrentRow.IsNewRow)
            {
                return;
            }

            dataGridViewPaths.Rows.Remove(dataGridViewPaths.CurrentRow);
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            TextBox textBoxAutoBackupInterval = GetAutoBackupIntervalTextBox();

            if (checkBoxDummy1.Checked &&
                (!int.TryParse(textBoxAutoBackupInterval.Text.Trim(), out int autoBackupIntervalMinutes) ||
                 autoBackupIntervalMinutes < 1 ||
                 autoBackupIntervalMinutes > 60))
            {
                MessageBox.Show("Auto-Backup (min) must be between 1 and 60.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            AppSettings newSettings = ReadSettingsFromUi();

            foreach (BackupPathPair pair in newSettings.BackupPathPairs)
            {
                if (string.IsNullOrWhiteSpace(pair.SourceDirectory) || string.IsNullOrWhiteSpace(pair.TargetDirectory))
                {
                    MessageBox.Show("Each row must contain source and target.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            ResultSettings = newSettings;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void dataGridViewPaths_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (dataGridViewPaths.Columns[e.ColumnIndex].Name == "ColumnSourceBrowse")
            {
                using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

                string currentValue = dataGridViewPaths.Rows[e.RowIndex].Cells["ColumnSourceDirectory"].Value?.ToString() ?? string.Empty;
                if (Directory.Exists(currentValue))
                {
                    folderBrowserDialog.SelectedPath = currentValue;
                }

                if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
                {
                    dataGridViewPaths.Rows[e.RowIndex].Cells["ColumnSourceDirectory"].Value = folderBrowserDialog.SelectedPath;
                }
            }

            if (dataGridViewPaths.Columns[e.ColumnIndex].Name == "ColumnTargetBrowse")
            {
                using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

                string currentValue = dataGridViewPaths.Rows[e.RowIndex].Cells["ColumnTargetDirectory"].Value?.ToString() ?? string.Empty;
                if (Directory.Exists(currentValue))
                {
                    folderBrowserDialog.SelectedPath = currentValue;
                }

                if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
                {
                    dataGridViewPaths.Rows[e.RowIndex].Cells["ColumnTargetDirectory"].Value = folderBrowserDialog.SelectedPath;
                }
            }
        }

        private AppSettings CloneSettings(AppSettings settings)
        {
            AppSettings clone = new AppSettings
            {
                ZipDestinationFiles = settings.ZipDestinationFiles,
                DefaultVersioning = settings.DefaultVersioning,
                AutoIncrementVersion = settings.AutoIncrementVersion,
                MinimizeToSystray = settings.MinimizeToSystray,
                IgnoreCopyErrors = settings.IgnoreCopyErrors,
                AutoBackupEnabled = settings.AutoBackupEnabled,
                AutoBackupIntervalMinutes = settings.AutoBackupIntervalMinutes,
                BackupPathPairs = new List<BackupPathPair>(),
                LastUsedVersionsByPair = new Dictionary<string, string>(settings.LastUsedVersionsByPair),
                MainWindowWidth = settings.MainWindowWidth,
                MainWindowHeight = settings.MainWindowHeight,
                MainWindowLeft = settings.MainWindowLeft,
                MainWindowTop = settings.MainWindowTop
            };

            foreach (BackupPathPair pair in settings.BackupPathPairs)
            {
                clone.BackupPathPairs.Add(new BackupPathPair
                {
                    IsEnabled = pair.IsEnabled,
                    SourceDirectory = pair.SourceDirectory,
                    TargetDirectory = pair.TargetDirectory
                });
            }

            return clone;
        }
    }
}