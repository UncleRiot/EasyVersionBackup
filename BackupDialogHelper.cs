// Design-Rule / UI consistency:
// Keep layout, spacing, colors, sizes, and fonts aligned with ModernTheme.
// Add new shared visual values to ModernTheme instead of hardcoding local exceptions here.
// 03.05.2026 /dc

using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EasyVersionBackup
{
    public static class BackupDialogHelper
    {
        public static DialogResult ShowDestinationConflictDialog(Form owner, string destinationPath, out string selectedConflictAction)
        {
            using DestinationConflictDialog dialog = new DestinationConflictDialog(owner, destinationPath);
            DialogResult result = dialog.ShowDialog(owner);
            selectedConflictAction = dialog.SelectedConflictAction;
            return result;
        }

        public static void ShowBackupCanceledBecauseDestinationExists(Form owner, string destinationPath)
        {
            ModernMessageDialog.Show(
                owner,
                "Backup canceled",
                BackupHelper.FormatBackupCanceledBecauseDestinationExistsMessage(destinationPath));
        }

        private sealed class DestinationConflictDialog : Form
        {
            private const int WmNclButtonDown = 0xA1;
            private const int HtCaption = 0x2;
            private const int WmNcHitTest = 0x84;
            private const int HtBottomRight = 17;

            private readonly Panel panelTitleBar;
            private readonly PictureBox pictureBoxTitleIcon;
            private readonly Label labelTitle;
            private readonly Label labelMessageTitle;
            private readonly Label labelDestinationPath;
            private readonly ComboBox comboBoxConflictAction;
            private readonly Button buttonOk;
            private readonly Button buttonCancel;

            public string SelectedConflictAction { get; private set; } = BackupHelper.DestinationConflictAppend;

            public DestinationConflictDialog(Form owner, string destinationPath)
            {
                Icon = owner.Icon;
                Text = "Destination Conflict";
                StartPosition = FormStartPosition.CenterParent;
                ClientSize = ModernTheme.BackupDialogDefaultSize;
                MinimumSize = ModernTheme.BackupDialogMinimumSize;
                FormBorderStyle = FormBorderStyle.None;
                BackColor = ModernTheme.WindowBackColor;
                Font = new Font(ModernTheme.FontFamilyName, ModernTheme.DefaultFontSize);
                ShowInTaskbar = false;
                DoubleBuffered = true;

                ModernWindowFrame.Apply(this);

                panelTitleBar = new Panel
                {
                    Name = "panelTitleBar",
                    Height = ModernTheme.TitleBarHeight,
                    Dock = DockStyle.Top,
                    BackColor = ModernTheme.TitleBarBackColor,
                    Cursor = Cursors.SizeAll
                };

                pictureBoxTitleIcon = new PictureBox
                {
                    Name = "pictureBoxTitleIcon",
                    Location = new Point(ModernTheme.TitleBarIconLeft, ModernTheme.TitleBarIconTop),
                    Size = new Size(ModernTheme.TitleBarIconSize, ModernTheme.TitleBarIconSize),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Image = Icon?.ToBitmap(),
                    BackColor = Color.Transparent
                };

                labelTitle = new Label
                {
                    Name = "labelTitle",
                    Text = "Destination Conflict",
                    AutoSize = false,
                    Location = new Point(ModernTheme.TitleBarTextLeft, 0),
                    Size = new Size(ClientSize.Width - ModernTheme.TitleBarTextLeft - 12, ModernTheme.TitleBarHeight),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = ModernTheme.TextColor,
                    Font = new Font(ModernTheme.FontFamilyName, ModernTheme.TitleFontSize, FontStyle.Bold),
                    BackColor = Color.Transparent,
                    Cursor = Cursors.SizeAll
                };

                labelMessageTitle = new Label
                {
                    Name = "labelMessageTitle",
                    Text = "Destination already exists:",
                    Location = new Point(ModernTheme.BackupDialogContentLeft, ModernTheme.BackupDialogContentTop),
                    Size = new Size(ClientSize.Width - ModernTheme.BackupDialogContentLeft - ModernTheme.BackupDialogContentRightMargin, ModernTheme.SettingsLabelHeight),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    ForeColor = ModernTheme.TextColor,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                labelDestinationPath = new Label
                {
                    Name = "labelDestinationPath",
                    Text = destinationPath,
                    Location = new Point(ModernTheme.BackupDialogContentLeft, labelMessageTitle.Bottom + ModernTheme.BackupDialogControlSpacing),
                    Size = new Size(ClientSize.Width - ModernTheme.BackupDialogContentLeft - ModernTheme.BackupDialogContentRightMargin, ModernTheme.BackupDialogPathLabelHeight),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    ForeColor = ModernTheme.TextColor,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoEllipsis = false
                };

                comboBoxConflictAction = new ComboBox
                {
                    Name = "comboBoxConflictAction",
                    Location = new Point(ModernTheme.BackupDialogContentLeft, labelDestinationPath.Bottom + ModernTheme.BackupDialogControlSpacing),
                    Size = new Size(ModernTheme.BackupDialogComboBoxWidth, ModernTheme.SettingsControlHeight),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = ModernTheme.TitleBarBackColor,
                    ForeColor = ModernTheme.TextColor
                };

                comboBoxConflictAction.Items.Add(BackupHelper.DestinationConflictAppend);
                comboBoxConflictAction.Items.Add(BackupHelper.DestinationConflictOverwrite);
                comboBoxConflictAction.Text = BackupHelper.DestinationConflictAppend;

                buttonOk = ModernTheme.CreateDialogPrimaryButton("buttonOk", "OK");
                buttonCancel = ModernTheme.CreateDialogSecondaryButton("buttonCancel", "Cancel", DialogResult.Cancel);
                ModernTheme.PositionDialogButtons(this, buttonOk, buttonCancel, true);

                buttonOk.Click += buttonOk_Click;

                panelTitleBar.MouseDown += titleBar_MouseDown;
                labelTitle.MouseDown += titleBar_MouseDown;
                pictureBoxTitleIcon.MouseDown += titleBar_MouseDown;

                panelTitleBar.Controls.Add(pictureBoxTitleIcon);
                panelTitleBar.Controls.Add(labelTitle);

                Controls.Add(panelTitleBar);
                Controls.Add(labelMessageTitle);
                Controls.Add(labelDestinationPath);
                Controls.Add(comboBoxConflictAction);
                Controls.Add(buttonOk);
                Controls.Add(buttonCancel);

                AcceptButton = buttonOk;
                CancelButton = buttonCancel;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                using Pen pen = new Pen(ModernTheme.WindowBorderColor, 1F);

                int right = ClientSize.Width - 5;
                int bottom = ClientSize.Height - 5;

                e.Graphics.DrawLine(pen, right - 10, bottom, right, bottom - 10);
                e.Graphics.DrawLine(pen, right - 6, bottom, right, bottom - 6);
                e.Graphics.DrawLine(pen, right - 2, bottom, right, bottom - 2);
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WmNcHitTest)
                {
                    base.WndProc(ref m);

                    Point cursorPosition = PointToClient(Cursor.Position);

                    if (cursorPosition.X >= ClientSize.Width - ModernTheme.BackupDialogResizeGripSize &&
                        cursorPosition.Y >= ClientSize.Height - ModernTheme.BackupDialogResizeGripSize)
                    {
                        m.Result = HtBottomRight;
                    }

                    return;
                }

                base.WndProc(ref m);
            }

            private void buttonOk_Click(object? sender, System.EventArgs e)
            {
                SelectedConflictAction = BackupHelper.NormalizeDestinationConflictHandling(comboBoxConflictAction.Text);
                DialogResult = DialogResult.OK;
                Close();
            }

            private void titleBar_MouseDown(object? sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, WmNclButtonDown, HtCaption, 0);
                }
            }

            [DllImport("user32.dll")]
            private static extern bool ReleaseCapture();

            [DllImport("user32.dll")]
            private static extern int SendMessage(nint hWnd, int msg, int wParam, int lParam);
        }
    }
}