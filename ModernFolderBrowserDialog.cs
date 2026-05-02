using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EasyVersionBackup
{
    public sealed class ModernFolderBrowserDialog : Form
    {
        private readonly TreeView treeViewFolders = new TreeView();
        private readonly ListBox listBoxDrives = new ListBox();
        private readonly TextBox textBoxSelectedPath = new TextBox();
        private readonly Button buttonOk = new Button();
        private readonly Button buttonCancel = new Button();



        public string SelectedPath { get; private set; } = string.Empty;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        public ModernFolderBrowserDialog(string title, string initialPath)
        {
            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            ClientSize = new Size(720, 460);
            MinimumSize = SizeFromClientSize(new Size(620, 360));
            BackColor = ModernTheme.WindowBackColor;
            Font = new Font(ModernTheme.FontFamilyName, ModernTheme.DefaultFontSize);
            DoubleBuffered = true;
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            ModernWindowFrame.Apply(this);

            InitializeModernTitleBar();
            InitializePathTextBox();
            InitializeDriveList();
            InitializeFolderTree();
            InitializeDialogButtons();

            LoadDrives();

            if (Directory.Exists(initialPath))
            {
                SelectInitialPath(initialPath);
            }
        }
        private void InitializeDriveList()
        {
            listBoxDrives.Name = "listBoxDrives";
            listBoxDrives.Location = new Point(12, 78);
            listBoxDrives.Size = new Size(170, ClientSize.Height - 129);
            listBoxDrives.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            listBoxDrives.BorderStyle = BorderStyle.FixedSingle;
            listBoxDrives.BackColor = ModernTheme.TitleBarBackColor;
            listBoxDrives.ForeColor = ModernTheme.TextColor;
            listBoxDrives.DrawMode = DrawMode.OwnerDrawFixed;
            listBoxDrives.ItemHeight = 24;
            listBoxDrives.IntegralHeight = false;
            listBoxDrives.DrawItem += listBoxDrives_DrawItem;
            listBoxDrives.SelectedIndexChanged += listBoxDrives_SelectedIndexChanged;

            Controls.Add(listBoxDrives);
        }

        public static bool Show(Form owner, string title, string initialPath, out string selectedPath)
        {
            using ModernFolderBrowserDialog dialog = new ModernFolderBrowserDialog(title, initialPath);

            if (dialog.ShowDialog(owner) != DialogResult.OK)
            {
                selectedPath = string.Empty;
                return false;
            }

            selectedPath = dialog.SelectedPath;
            return true;
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

        private void InitializePathTextBox()
        {
            textBoxSelectedPath.Name = "textBoxSelectedPath";
            textBoxSelectedPath.Location = new Point(12, 44);
            textBoxSelectedPath.Size = new Size(ClientSize.Width - 24, 23);
            textBoxSelectedPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxSelectedPath.ReadOnly = true;
            textBoxSelectedPath.BorderStyle = BorderStyle.FixedSingle;
            textBoxSelectedPath.BackColor = ModernTheme.TitleBarBackColor;
            textBoxSelectedPath.ForeColor = ModernTheme.TextColor;

            Controls.Add(textBoxSelectedPath);
        }

        private void InitializeFolderTree()
        {
            treeViewFolders.Name = "treeViewFolders";
            treeViewFolders.Location = new Point(190, 78);
            treeViewFolders.Size = new Size(ClientSize.Width - 202, ClientSize.Height - 129);
            treeViewFolders.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            treeViewFolders.BorderStyle = BorderStyle.FixedSingle;
            treeViewFolders.BackColor = ModernTheme.TitleBarBackColor;
            treeViewFolders.ForeColor = ModernTheme.TextColor;
            treeViewFolders.HideSelection = false;
            treeViewFolders.FullRowSelect = true;
            treeViewFolders.DrawMode = TreeViewDrawMode.OwnerDrawText;
            treeViewFolders.BeforeExpand += treeViewFolders_BeforeExpand;
            treeViewFolders.AfterSelect += treeViewFolders_AfterSelect;
            treeViewFolders.DrawNode += treeViewFolders_DrawNode;

            Controls.Add(treeViewFolders);
        }

        private void InitializeDialogButtons()
        {
            buttonOk.Text = "OK";
            buttonOk.DialogResult = DialogResult.OK;
            buttonOk.Size = ModernTheme.DialogButtonSize;
            buttonOk.Location = new Point(ClientSize.Width - 168, ClientSize.Height - 39);
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
            buttonOk.Click += buttonOk_Click;

            buttonCancel.Text = "Cancel";
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Size = ModernTheme.DialogButtonSize;
            buttonCancel.Location = new Point(ClientSize.Width - 87, ClientSize.Height - 39);
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.FlatStyle = FlatStyle.Flat;
            buttonCancel.BackColor = ModernTheme.ControlBackColor;
            buttonCancel.ForeColor = ModernTheme.TextColor;
            buttonCancel.Cursor = Cursors.Hand;
            buttonCancel.TextAlign = ContentAlignment.MiddleCenter;
            buttonCancel.Padding = ModernTheme.DialogSecondaryButtonTextPadding;
            buttonCancel.UseCompatibleTextRendering = true;
            buttonCancel.UseVisualStyleBackColor = false;
            buttonCancel.FlatAppearance.BorderColor = ModernTheme.AccentColor;
            buttonCancel.FlatAppearance.BorderSize = 1;
            buttonCancel.FlatAppearance.MouseOverBackColor = ModernTheme.ControlHoverBackColor;
            buttonCancel.FlatAppearance.MouseDownBackColor = ModernTheme.AccentColor;

            Controls.Add(buttonOk);
            Controls.Add(buttonCancel);

            AcceptButton = buttonOk;
            CancelButton = buttonCancel;
        }

        private void LoadDrives()
        {
            listBoxDrives.Items.Clear();
            treeViewFolders.Nodes.Clear();

            foreach (DriveInfo driveInfo in DriveInfo.GetDrives().Where(drive => drive.IsReady))
            {
                listBoxDrives.Items.Add(driveInfo);

                TreeNode node = new TreeNode(FormatDriveNodeText(driveInfo))
                {
                    Tag = driveInfo.RootDirectory.FullName
                };

                AddPlaceholderNode(node);
                treeViewFolders.Nodes.Add(node);
            }

            if (listBoxDrives.Items.Count > 0)
            {
                listBoxDrives.SelectedIndex = 0;
            }
        }
        private void listBoxDrives_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            Color backColor = isSelected
                ? ModernTheme.AccentColor
                : ModernTheme.TitleBarBackColor;

            Color foreColor = isSelected
                ? ModernTheme.DarkTextColor
                : ModernTheme.TextColor;

            using SolidBrush backgroundBrush = new SolidBrush(backColor);

            e.Graphics.FillRectangle(backgroundBrush, e.Bounds);

            if (listBoxDrives.Items[e.Index] is not DriveInfo driveInfo)
            {
                return;
            }

            TextRenderer.DrawText(
                e.Graphics,
                FormatDriveNodeText(driveInfo),
                listBoxDrives.Font,
                new Rectangle(e.Bounds.Left + 8, e.Bounds.Top, e.Bounds.Width - 16, e.Bounds.Height),
                foreColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPrefix);
        }
        private void listBoxDrives_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (listBoxDrives.SelectedItem is not DriveInfo selectedDrive)
            {
                return;
            }

            foreach (TreeNode node in treeViewFolders.Nodes)
            {
                string nodePath = node.Tag?.ToString() ?? string.Empty;

                if (!string.Equals(nodePath, selectedDrive.RootDirectory.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                treeViewFolders.SelectedNode = node;
                node.Expand();
                node.EnsureVisible();
                return;
            }
        }
        private string FormatDriveNodeText(DriveInfo driveInfo)
        {
            string volumeLabel = driveInfo.VolumeLabel;

            if (string.IsNullOrWhiteSpace(volumeLabel))
            {
                return driveInfo.Name;
            }

            return $"{driveInfo.Name} ({volumeLabel})";
        }
        private void treeViewFolders_DrawNode(object? sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node == null)
            {
                return;
            }

            bool isSelected = (e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected;

            Color backColor = isSelected
                ? ModernTheme.AccentColor
                : ModernTheme.TitleBarBackColor;

            Color foreColor = isSelected
                ? ModernTheme.DarkTextColor
                : ModernTheme.TextColor;

            using SolidBrush backgroundBrush = new SolidBrush(backColor);
            using SolidBrush textBrush = new SolidBrush(foreColor);

            e.Graphics.FillRectangle(backgroundBrush, e.Bounds);

            TextRenderer.DrawText(
                e.Graphics,
                e.Node.Text,
                treeViewFolders.Font,
                e.Bounds,
                foreColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }

        private void LoadChildDirectories(TreeNode parentNode)
        {
            string parentPath = parentNode.Tag?.ToString() ?? string.Empty;

            parentNode.Nodes.Clear();

            if (string.IsNullOrWhiteSpace(parentPath) || !Directory.Exists(parentPath))
            {
                return;
            }

            try
            {
                foreach (string directoryPath in Directory.GetDirectories(parentPath).OrderBy(path => path))
                {
                    TreeNode childNode = new TreeNode(Path.GetFileName(directoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)))
                    {
                        Tag = directoryPath
                    };

                    if (DirectoryContainsChildDirectories(directoryPath))
                    {
                        AddPlaceholderNode(childNode);
                    }

                    parentNode.Nodes.Add(childNode);
                }
            }
            catch
            {
            }
        }

        private bool DirectoryContainsChildDirectories(string directoryPath)
        {
            try
            {
                return Directory.EnumerateDirectories(directoryPath).Any();
            }
            catch
            {
                return false;
            }
        }

        private void AddPlaceholderNode(TreeNode node)
        {
            node.Nodes.Add(new TreeNode("..."));
        }

        private void SelectInitialPath(string initialPath)
        {
            string fullInitialPath = Path.GetFullPath(initialPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            foreach (TreeNode driveNode in treeViewFolders.Nodes)
            {
                string drivePath = driveNode.Tag?.ToString() ?? string.Empty;

                if (!fullInitialPath.StartsWith(drivePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                for (int i = 0; i < listBoxDrives.Items.Count; i++)
                {
                    if (listBoxDrives.Items[i] is DriveInfo driveInfo &&
                        string.Equals(driveInfo.RootDirectory.FullName, drivePath, StringComparison.OrdinalIgnoreCase))
                    {
                        listBoxDrives.SelectedIndex = i;
                        break;
                    }
                }

                treeViewFolders.SelectedNode = driveNode;
                driveNode.Expand();

                TreeNode currentNode = driveNode;
                string currentPath = drivePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                foreach (string part in fullInitialPath.Substring(currentPath.Length).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
                {
                    if (string.IsNullOrWhiteSpace(part))
                    {
                        continue;
                    }

                    TreeNode? nextNode = currentNode.Nodes
                        .Cast<TreeNode>()
                        .FirstOrDefault(node => string.Equals(node.Text, part, StringComparison.OrdinalIgnoreCase));

                    if (nextNode == null)
                    {
                        break;
                    }

                    treeViewFolders.SelectedNode = nextNode;
                    nextNode.Expand();
                    currentNode = nextNode;
                }

                break;
            }
        }

        private void treeViewFolders_BeforeExpand(object? sender, TreeViewCancelEventArgs e)
        {
            if (e.Node == null)
            {
                return;
            }

            if (e.Node.Nodes.Count == 1 && e.Node.Nodes[0].Tag == null && e.Node.Nodes[0].Text == "...")
            {
                LoadChildDirectories(e.Node);
            }
        }

        private void treeViewFolders_AfterSelect(object? sender, TreeViewEventArgs e)
        {
            string selectedPath = e.Node?.Tag?.ToString() ?? string.Empty;

            textBoxSelectedPath.Text = selectedPath;
            SelectedPath = selectedPath;
            buttonOk.Enabled = Directory.Exists(selectedPath);
        }

        private void buttonOk_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SelectedPath) || !Directory.Exists(SelectedPath))
            {
                DialogResult = DialogResult.None;
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
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
    }
}