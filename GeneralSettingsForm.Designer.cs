namespace EasyVersionBackup
{
    partial class GeneralSettingsForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label labelZipDestinationFiles;
        private System.Windows.Forms.CheckBox checkBoxZipDestinationFiles;
        private System.Windows.Forms.Label labelDefaultVersioning;
        private System.Windows.Forms.ComboBox comboBoxDefaultVersioning;
        private System.Windows.Forms.Label labelAutoIncrementVersion;
        private System.Windows.Forms.CheckBox checkBoxAutoIncrementVersion;
        private System.Windows.Forms.Label labelMinimizeToSystray;
        private System.Windows.Forms.CheckBox checkBoxMinimizeToSystray;
        private System.Windows.Forms.Panel panelOptionsDivider;
        private System.Windows.Forms.Label labelIgnoreCopyErrors;
        private System.Windows.Forms.CheckBox checkBoxIgnoreCopyErrors;
        private System.Windows.Forms.Label labelDummy1;
        private System.Windows.Forms.CheckBox checkBoxDummy1;
        private System.Windows.Forms.Label labelDummy2;
        private System.Windows.Forms.CheckBox checkBoxDummy2;
        private System.Windows.Forms.Label labelDummy3;
        private System.Windows.Forms.CheckBox checkBoxDummy3;
        private System.Windows.Forms.DataGridView dataGridViewPaths;
        private System.Windows.Forms.Button buttonAddRow;
        private System.Windows.Forms.Button buttonRemoveRow;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ToolTip toolTipButtons;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnIsEnabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnSourceDirectory;
        private System.Windows.Forms.DataGridViewButtonColumn ColumnSourceBrowse;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnTargetDirectory;
        private System.Windows.Forms.DataGridViewButtonColumn ColumnTargetBrowse;

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
            components = new System.ComponentModel.Container();
            labelZipDestinationFiles = new System.Windows.Forms.Label();
            checkBoxZipDestinationFiles = new System.Windows.Forms.CheckBox();
            labelDefaultVersioning = new System.Windows.Forms.Label();
            comboBoxDefaultVersioning = new System.Windows.Forms.ComboBox();
            labelAutoIncrementVersion = new System.Windows.Forms.Label();
            checkBoxAutoIncrementVersion = new System.Windows.Forms.CheckBox();
            labelMinimizeToSystray = new System.Windows.Forms.Label();
            checkBoxMinimizeToSystray = new System.Windows.Forms.CheckBox();
            panelOptionsDivider = new System.Windows.Forms.Panel();
            labelIgnoreCopyErrors = new System.Windows.Forms.Label();
            checkBoxIgnoreCopyErrors = new System.Windows.Forms.CheckBox();
            labelDummy1 = new System.Windows.Forms.Label();
            checkBoxDummy1 = new System.Windows.Forms.CheckBox();
            labelDummy2 = new System.Windows.Forms.Label();
            checkBoxDummy2 = new System.Windows.Forms.CheckBox();
            labelDummy3 = new System.Windows.Forms.Label();
            checkBoxDummy3 = new System.Windows.Forms.CheckBox();
            dataGridViewPaths = new System.Windows.Forms.DataGridView();
            ColumnIsEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            ColumnSourceDirectory = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ColumnSourceBrowse = new System.Windows.Forms.DataGridViewButtonColumn();
            ColumnTargetDirectory = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ColumnTargetBrowse = new System.Windows.Forms.DataGridViewButtonColumn();
            buttonAddRow = new System.Windows.Forms.Button();
            buttonRemoveRow = new System.Windows.Forms.Button();
            buttonOk = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
            toolTipButtons = new System.Windows.Forms.ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)dataGridViewPaths).BeginInit();
            SuspendLayout();
            // 
            // labelZipDestinationFiles
            // 
            labelZipDestinationFiles.AutoSize = true;
            labelZipDestinationFiles.Location = new System.Drawing.Point(12, 16);
            labelZipDestinationFiles.Name = "labelZipDestinationFiles";
            labelZipDestinationFiles.Size = new System.Drawing.Size(113, 15);
            labelZipDestinationFiles.TabIndex = 0;
            labelZipDestinationFiles.Text = "Zip Destination Files";
            // 
            // checkBoxZipDestinationFiles
            // 
            checkBoxZipDestinationFiles.AutoSize = true;
            checkBoxZipDestinationFiles.Location = new System.Drawing.Point(145, 16);
            checkBoxZipDestinationFiles.Name = "checkBoxZipDestinationFiles";
            checkBoxZipDestinationFiles.Size = new System.Drawing.Size(15, 14);
            checkBoxZipDestinationFiles.TabIndex = 1;
            checkBoxZipDestinationFiles.UseVisualStyleBackColor = true;
            // 
            // labelDefaultVersioning
            // 
            labelDefaultVersioning.AutoSize = true;
            labelDefaultVersioning.Location = new System.Drawing.Point(12, 45);
            labelDefaultVersioning.Name = "labelDefaultVersioning";
            labelDefaultVersioning.Size = new System.Drawing.Size(110, 15);
            labelDefaultVersioning.TabIndex = 2;
            labelDefaultVersioning.Text = "Default Versioning";
            // 
            // comboBoxDefaultVersioning
            // 
            comboBoxDefaultVersioning.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxDefaultVersioning.FormattingEnabled = true;
            comboBoxDefaultVersioning.Location = new System.Drawing.Point(145, 41);
            comboBoxDefaultVersioning.Name = "comboBoxDefaultVersioning";
            comboBoxDefaultVersioning.Size = new System.Drawing.Size(135, 23);
            comboBoxDefaultVersioning.TabIndex = 3;
            // 
            // labelAutoIncrementVersion
            // 
            labelAutoIncrementVersion.AutoSize = true;
            labelAutoIncrementVersion.Location = new System.Drawing.Point(12, 76);
            labelAutoIncrementVersion.Name = "labelAutoIncrementVersion";
            labelAutoIncrementVersion.Size = new System.Drawing.Size(93, 15);
            labelAutoIncrementVersion.TabIndex = 4;
            labelAutoIncrementVersion.Text = "Auto increment";
            // 
            // checkBoxAutoIncrementVersion
            // 
            checkBoxAutoIncrementVersion.AutoSize = true;
            checkBoxAutoIncrementVersion.Location = new System.Drawing.Point(145, 76);
            checkBoxAutoIncrementVersion.Name = "checkBoxAutoIncrementVersion";
            checkBoxAutoIncrementVersion.Size = new System.Drawing.Size(15, 14);
            checkBoxAutoIncrementVersion.TabIndex = 5;
            checkBoxAutoIncrementVersion.UseVisualStyleBackColor = true;
            // 
            // labelMinimizeToSystray
            // 
            labelMinimizeToSystray.AutoSize = true;
            labelMinimizeToSystray.Location = new System.Drawing.Point(12, 107);
            labelMinimizeToSystray.Name = "labelMinimizeToSystray";
            labelMinimizeToSystray.Size = new System.Drawing.Size(113, 15);
            labelMinimizeToSystray.TabIndex = 6;
            labelMinimizeToSystray.Text = "Minimize to Systray";
            // 
            // checkBoxMinimizeToSystray
            // 
            checkBoxMinimizeToSystray.AutoSize = true;
            checkBoxMinimizeToSystray.Location = new System.Drawing.Point(145, 107);
            checkBoxMinimizeToSystray.Name = "checkBoxMinimizeToSystray";
            checkBoxMinimizeToSystray.Size = new System.Drawing.Size(15, 14);
            checkBoxMinimizeToSystray.TabIndex = 7;
            checkBoxMinimizeToSystray.UseVisualStyleBackColor = true;
            // 
            // panelOptionsDivider
            // 
            panelOptionsDivider.BackColor = System.Drawing.SystemColors.ControlDark;
            panelOptionsDivider.Location = new System.Drawing.Point(392, 12);
            panelOptionsDivider.Name = "panelOptionsDivider";
            panelOptionsDivider.Size = new System.Drawing.Size(1, 118);
            panelOptionsDivider.TabIndex = 8;
            // 
            // labelIgnoreCopyErrors
            // 
            labelIgnoreCopyErrors.AutoSize = true;
            labelIgnoreCopyErrors.Location = new System.Drawing.Point(420, 16);
            labelIgnoreCopyErrors.Name = "labelIgnoreCopyErrors";
            labelIgnoreCopyErrors.Size = new System.Drawing.Size(104, 15);
            labelIgnoreCopyErrors.TabIndex = 9;
            labelIgnoreCopyErrors.Text = "Ignore Copy-Errors";
            // 
            // checkBoxIgnoreCopyErrors
            // 
            checkBoxIgnoreCopyErrors.AutoSize = true;
            checkBoxIgnoreCopyErrors.Location = new System.Drawing.Point(555, 16);
            checkBoxIgnoreCopyErrors.Name = "checkBoxIgnoreCopyErrors";
            checkBoxIgnoreCopyErrors.Size = new System.Drawing.Size(15, 14);
            checkBoxIgnoreCopyErrors.TabIndex = 10;
            checkBoxIgnoreCopyErrors.UseVisualStyleBackColor = true;
            // 
            // labelDummy1
            // 
            labelDummy1.AutoSize = true;
            labelDummy1.Location = new System.Drawing.Point(420, 45);
            labelDummy1.Name = "labelDummy1";
            labelDummy1.Size = new System.Drawing.Size(55, 15);
            labelDummy1.TabIndex = 11;
            labelDummy1.Text = "Dummy 1";
            // 
            // checkBoxDummy1
            // 
            checkBoxDummy1.AutoSize = true;
            checkBoxDummy1.Location = new System.Drawing.Point(555, 45);
            checkBoxDummy1.Name = "checkBoxDummy1";
            checkBoxDummy1.Size = new System.Drawing.Size(15, 14);
            checkBoxDummy1.TabIndex = 12;
            checkBoxDummy1.UseVisualStyleBackColor = true;
            // 
            // labelDummy2
            // 
            labelDummy2.AutoSize = true;
            labelDummy2.Location = new System.Drawing.Point(420, 76);
            labelDummy2.Name = "labelDummy2";
            labelDummy2.Size = new System.Drawing.Size(55, 15);
            labelDummy2.TabIndex = 13;
            labelDummy2.Text = "Dummy 2";
            // 
            // checkBoxDummy2
            // 
            checkBoxDummy2.AutoSize = true;
            checkBoxDummy2.Location = new System.Drawing.Point(555, 76);
            checkBoxDummy2.Name = "checkBoxDummy2";
            checkBoxDummy2.Size = new System.Drawing.Size(15, 14);
            checkBoxDummy2.TabIndex = 14;
            checkBoxDummy2.UseVisualStyleBackColor = true;
            // 
            // labelDummy3
            // 
            labelDummy3.AutoSize = true;
            labelDummy3.Location = new System.Drawing.Point(420, 107);
            labelDummy3.Name = "labelDummy3";
            labelDummy3.Size = new System.Drawing.Size(55, 15);
            labelDummy3.TabIndex = 15;
            labelDummy3.Text = "Dummy 3";
            // 
            // checkBoxDummy3
            // 
            checkBoxDummy3.AutoSize = true;
            checkBoxDummy3.Location = new System.Drawing.Point(555, 107);
            checkBoxDummy3.Name = "checkBoxDummy3";
            checkBoxDummy3.Size = new System.Drawing.Size(15, 14);
            checkBoxDummy3.TabIndex = 16;
            checkBoxDummy3.UseVisualStyleBackColor = true;
            // 
            // dataGridViewPaths
            // 
            dataGridViewPaths.AllowUserToAddRows = false;
            dataGridViewPaths.AllowUserToDeleteRows = false;
            dataGridViewPaths.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewPaths.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { ColumnIsEnabled, ColumnSourceDirectory, ColumnSourceBrowse, ColumnTargetDirectory, ColumnTargetBrowse });
            dataGridViewPaths.Location = new System.Drawing.Point(12, 160);
            dataGridViewPaths.MultiSelect = false;
            dataGridViewPaths.Name = "dataGridViewPaths";
            dataGridViewPaths.RowHeadersVisible = false;
            dataGridViewPaths.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dataGridViewPaths.Size = new System.Drawing.Size(760, 209);
            dataGridViewPaths.TabIndex = 19;
            dataGridViewPaths.CellContentClick += dataGridViewPaths_CellContentClick;
            // 
            // ColumnIsEnabled
            // 
            ColumnIsEnabled.HeaderText = "";
            ColumnIsEnabled.Name = "ColumnIsEnabled";
            ColumnIsEnabled.Width = 40;
            // 
            // ColumnSourceDirectory
            // 
            ColumnSourceDirectory.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            ColumnSourceDirectory.HeaderText = "Source Directory";
            ColumnSourceDirectory.Name = "ColumnSourceDirectory";
            // 
            // ColumnSourceBrowse
            // 
            ColumnSourceBrowse.HeaderText = "";
            ColumnSourceBrowse.Name = "ColumnSourceBrowse";
            ColumnSourceBrowse.Text = "Browse...";
            ColumnSourceBrowse.UseColumnTextForButtonValue = true;
            ColumnSourceBrowse.Width = 90;
            // 
            // ColumnTargetDirectory
            // 
            ColumnTargetDirectory.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            ColumnTargetDirectory.HeaderText = "Target Directory";
            ColumnTargetDirectory.Name = "ColumnTargetDirectory";
            // 
            // ColumnTargetBrowse
            // 
            ColumnTargetBrowse.HeaderText = "";
            ColumnTargetBrowse.Name = "ColumnTargetBrowse";
            ColumnTargetBrowse.Text = "Browse...";
            ColumnTargetBrowse.UseColumnTextForButtonValue = true;
            ColumnTargetBrowse.Width = 90;
            // 
            // buttonAddRow
            // 
            buttonAddRow.Location = new System.Drawing.Point(12, 131);
            buttonAddRow.Name = "buttonAddRow";
            buttonAddRow.Size = new System.Drawing.Size(40, 23);
            buttonAddRow.TabIndex = 17;
            buttonAddRow.Text = "+";
            toolTipButtons.SetToolTip(buttonAddRow, "Add");
            buttonAddRow.UseVisualStyleBackColor = true;
            buttonAddRow.Click += buttonAddRow_Click;
            // 
            // buttonRemoveRow
            // 
            buttonRemoveRow.Location = new System.Drawing.Point(58, 131);
            buttonRemoveRow.Name = "buttonRemoveRow";
            buttonRemoveRow.Size = new System.Drawing.Size(40, 23);
            buttonRemoveRow.TabIndex = 18;
            buttonRemoveRow.Text = "-";
            toolTipButtons.SetToolTip(buttonRemoveRow, "Delete");
            buttonRemoveRow.UseVisualStyleBackColor = true;
            buttonRemoveRow.Click += buttonRemoveRow_Click;
            // 
            // buttonOk
            // 
            buttonOk.Location = new System.Drawing.Point(616, 383);
            buttonOk.Name = "buttonOk";
            buttonOk.Size = new System.Drawing.Size(75, 28);
            buttonOk.TabIndex = 20;
            buttonOk.Text = "OK";
            buttonOk.UseVisualStyleBackColor = true;
            buttonOk.Click += buttonOk_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new System.Drawing.Point(697, 383);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(75, 28);
            buttonCancel.TabIndex = 21;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // GeneralSettingsForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(784, 423);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOk);
            Controls.Add(dataGridViewPaths);
            Controls.Add(buttonRemoveRow);
            Controls.Add(buttonAddRow);
            Controls.Add(checkBoxDummy3);
            Controls.Add(labelDummy3);
            Controls.Add(checkBoxDummy2);
            Controls.Add(labelDummy2);
            Controls.Add(checkBoxDummy1);
            Controls.Add(labelDummy1);
            Controls.Add(checkBoxIgnoreCopyErrors);
            Controls.Add(labelIgnoreCopyErrors);
            Controls.Add(panelOptionsDivider);
            Controls.Add(checkBoxMinimizeToSystray);
            Controls.Add(labelMinimizeToSystray);
            Controls.Add(checkBoxAutoIncrementVersion);
            Controls.Add(labelAutoIncrementVersion);
            Controls.Add(comboBoxDefaultVersioning);
            Controls.Add(labelDefaultVersioning);
            Controls.Add(checkBoxZipDestinationFiles);
            Controls.Add(labelZipDestinationFiles);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "GeneralSettingsForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "General";
            ((System.ComponentModel.ISupportInitialize)dataGridViewPaths).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}