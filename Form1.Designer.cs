namespace EasyVersionBackup
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.Button buttonBackup;
        private System.Windows.Forms.Label labelConfiguredPaths;
        private System.Windows.Forms.DataGridView dataGridViewConfiguredPaths;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnConfiguredIsEnabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnConfiguredSourceDirectory;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnConfiguredTargetDirectory;
        private System.Windows.Forms.NotifyIcon notifyIconMain;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripTray;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOpen;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemBackup;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExitTray;

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
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            generalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            buttonBackup = new System.Windows.Forms.Button();
            labelConfiguredPaths = new System.Windows.Forms.Label();
            dataGridViewConfiguredPaths = new System.Windows.Forms.DataGridView();
            ColumnConfiguredIsEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            ColumnConfiguredSourceDirectory = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ColumnConfiguredTargetDirectory = new System.Windows.Forms.DataGridViewTextBoxColumn();
            notifyIconMain = new System.Windows.Forms.NotifyIcon(components);
            contextMenuStripTray = new System.Windows.Forms.ContextMenuStrip(components);
            toolStripMenuItemOpen = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItemBackup = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItemExitTray = new System.Windows.Forms.ToolStripMenuItem();
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewConfiguredPaths).BeginInit();
            contextMenuStripTray.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem, settingsToolStripMenuItem, helpToolStripMenuItem });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(800, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new System.Drawing.Size(93, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { generalToolStripMenuItem });
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            settingsToolStripMenuItem.Text = "Settings";
            // 
            // generalToolStripMenuItem
            // 
            generalToolStripMenuItem.Name = "generalToolStripMenuItem";
            generalToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            generalToolStripMenuItem.Text = "General";
            generalToolStripMenuItem.Click += generalToolStripMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            helpToolStripMenuItem.Text = "About";
            helpToolStripMenuItem.Click += helpToolStripMenuItem_Click;
            // 
            // buttonBackup
            // 
            buttonBackup.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            buttonBackup.Location = new System.Drawing.Point(698, 33);
            buttonBackup.Name = "buttonBackup";
            buttonBackup.Size = new System.Drawing.Size(90, 26);
            buttonBackup.TabIndex = 1;
            buttonBackup.Text = "Backup";
            buttonBackup.UseVisualStyleBackColor = true;
            buttonBackup.Click += buttonBackup_Click;
            // 
            // labelConfiguredPaths
            // 
            labelConfiguredPaths.AutoSize = true;
            labelConfiguredPaths.Location = new System.Drawing.Point(12, 39);
            labelConfiguredPaths.Name = "labelConfiguredPaths";
            labelConfiguredPaths.Size = new System.Drawing.Size(99, 15);
            labelConfiguredPaths.TabIndex = 2;
            labelConfiguredPaths.Text = "Configured Paths";
            // 
            // dataGridViewConfiguredPaths
            // 
            dataGridViewConfiguredPaths.AllowUserToAddRows = false;
            dataGridViewConfiguredPaths.AllowUserToDeleteRows = false;
            dataGridViewConfiguredPaths.AllowUserToResizeRows = false;
            dataGridViewConfiguredPaths.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            dataGridViewConfiguredPaths.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewConfiguredPaths.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { ColumnConfiguredIsEnabled, ColumnConfiguredSourceDirectory, ColumnConfiguredTargetDirectory });
            dataGridViewConfiguredPaths.Location = new System.Drawing.Point(12, 65);
            dataGridViewConfiguredPaths.MultiSelect = false;
            dataGridViewConfiguredPaths.Name = "dataGridViewConfiguredPaths";
            dataGridViewConfiguredPaths.RowHeadersVisible = false;
            dataGridViewConfiguredPaths.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dataGridViewConfiguredPaths.Size = new System.Drawing.Size(776, 318);
            dataGridViewConfiguredPaths.TabIndex = 3;
            dataGridViewConfiguredPaths.CellValueChanged += dataGridViewConfiguredPaths_CellValueChanged;
            dataGridViewConfiguredPaths.CurrentCellDirtyStateChanged += dataGridViewConfiguredPaths_CurrentCellDirtyStateChanged;
            // 
            // ColumnConfiguredIsEnabled
            // 
            ColumnConfiguredIsEnabled.HeaderText = "";
            ColumnConfiguredIsEnabled.Name = "ColumnConfiguredIsEnabled";
            ColumnConfiguredIsEnabled.Width = 35;
            // 
            // ColumnConfiguredSourceDirectory
            // 
            ColumnConfiguredSourceDirectory.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            ColumnConfiguredSourceDirectory.HeaderText = "Source Directory";
            ColumnConfiguredSourceDirectory.Name = "ColumnConfiguredSourceDirectory";
            ColumnConfiguredSourceDirectory.ReadOnly = true;
            // 
            // ColumnConfiguredTargetDirectory
            // 
            ColumnConfiguredTargetDirectory.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            ColumnConfiguredTargetDirectory.HeaderText = "Target Directory";
            ColumnConfiguredTargetDirectory.Name = "ColumnConfiguredTargetDirectory";
            ColumnConfiguredTargetDirectory.ReadOnly = true;
            // 
            // notifyIconMain
            // 
            notifyIconMain.ContextMenuStrip = contextMenuStripTray;
            notifyIconMain.Text = "EasyVersionBackup";
            notifyIconMain.Visible = false;
            notifyIconMain.MouseDoubleClick += notifyIconMain_MouseDoubleClick;
            // 
            // contextMenuStripTray
            // 
            contextMenuStripTray.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripMenuItemOpen, toolStripMenuItemBackup, toolStripMenuItemExitTray });
            contextMenuStripTray.Name = "contextMenuStripTray";
            contextMenuStripTray.Size = new System.Drawing.Size(113, 70);
            // 
            // toolStripMenuItemOpen
            // 
            toolStripMenuItemOpen.Name = "toolStripMenuItemOpen";
            toolStripMenuItemOpen.Size = new System.Drawing.Size(112, 22);
            toolStripMenuItemOpen.Text = "Open";
            toolStripMenuItemOpen.Click += toolStripMenuItemOpen_Click;
            // 
            // toolStripMenuItemBackup
            // 
            toolStripMenuItemBackup.Name = "toolStripMenuItemBackup";
            toolStripMenuItemBackup.Size = new System.Drawing.Size(112, 22);
            toolStripMenuItemBackup.Text = "Backup";
            toolStripMenuItemBackup.Click += toolStripMenuItemBackup_Click;
            // 
            // toolStripMenuItemExitTray
            // 
            toolStripMenuItemExitTray.Name = "toolStripMenuItemExitTray";
            toolStripMenuItemExitTray.Size = new System.Drawing.Size(112, 22);
            toolStripMenuItemExitTray.Text = "Exit";
            toolStripMenuItemExitTray.Click += toolStripMenuItemExitTray_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 395);
            Controls.Add(dataGridViewConfiguredPaths);
            Controls.Add(labelConfiguredPaths);
            Controls.Add(buttonBackup);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            MinimumSize = new System.Drawing.Size(500, 300);
            Name = "Form1";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "EasyVersionBackup";
            FormClosing += Form1_FormClosing;
            Move += Form1_Move;
            Resize += Form1_Resize;
            ResizeEnd += Form1_ResizeEnd;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewConfiguredPaths).EndInit();
            contextMenuStripTray.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}