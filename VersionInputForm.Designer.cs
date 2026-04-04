namespace EasyVersionBackup
{
    partial class VersionInputForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dataGridViewVersions;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnSourceName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnVersion;
        private System.Windows.Forms.CheckBox checkBoxIgnoreCopyErrors;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            dataGridViewVersions = new System.Windows.Forms.DataGridView();
            ColumnSourceName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ColumnVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            buttonOk = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
            labelInfo = new System.Windows.Forms.Label();
            checkBoxIgnoreCopyErrors = new System.Windows.Forms.CheckBox();

            ((System.ComponentModel.ISupportInitialize)dataGridViewVersions).BeginInit();
            SuspendLayout();

            dataGridViewVersions.AllowUserToAddRows = false;
            dataGridViewVersions.AllowUserToDeleteRows = false;
            dataGridViewVersions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewVersions.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { ColumnSourceName, ColumnVersion });
            dataGridViewVersions.Location = new System.Drawing.Point(12, 34);
            dataGridViewVersions.Name = "dataGridViewVersions";
            dataGridViewVersions.RowHeadersVisible = false;
            dataGridViewVersions.Size = new System.Drawing.Size(460, 90);

            ColumnSourceName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            ColumnSourceName.HeaderText = "Source";
            ColumnSourceName.Width = 200;

            ColumnVersion.HeaderText = "Version";
            ColumnVersion.Width = 140;

            checkBoxIgnoreCopyErrors.Location = new System.Drawing.Point(12, 130);
            checkBoxIgnoreCopyErrors.Text = "Ignore copy errors";
            checkBoxIgnoreCopyErrors.AutoSize = true;

            buttonOk.Location = new System.Drawing.Point(208, 160);
            buttonOk.Size = new System.Drawing.Size(75, 28);
            buttonOk.Text = "OK";
            buttonOk.Click += buttonOk_Click;

            buttonCancel.Location = new System.Drawing.Point(289, 160);
            buttonCancel.Size = new System.Drawing.Size(75, 28);
            buttonCancel.Text = "Cancel";
            buttonCancel.Click += buttonCancel_Click;

            labelInfo.AutoSize = true;
            labelInfo.Location = new System.Drawing.Point(12, 9);
            labelInfo.Text = "Check/adjust version values";

            ClientSize = new System.Drawing.Size(484, 210);
            Controls.Add(checkBoxIgnoreCopyErrors);
            Controls.Add(labelInfo);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOk);
            Controls.Add(dataGridViewVersions);

            StartPosition = FormStartPosition.Manual;
            Text = "Backup Version";

            ((System.ComponentModel.ISupportInitialize)dataGridViewVersions).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}