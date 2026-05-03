// Design-Rule / UI consistency:
// Keep layout, spacing, colors, sizes, and fonts aligned with ModernTheme.
// Add new shared visual values to ModernTheme instead of hardcoding local exceptions here.
// 03.05.2026 /dc


using System;
using System.Drawing;
using System.Windows.Forms;

namespace EasyVersionBackup
{
    public sealed class ModernMessageDialog : Form
    {
        private const int WmNclButtonDown = 0xA1;
        private const int HtCaption = 0x2;
        private const int WmNcHitTest = 0x84;
        private const int HtBottomRight = 17;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private readonly Label labelMessage;
        private readonly Button buttonOk;

        public ModernMessageDialog(Form owner, string title, string message)
        {
            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = ModernTheme.MessageDialogDefaultSize;
            MinimumSize = ModernTheme.MessageDialogMinimumSize;
            BackColor = ModernTheme.WindowBackColor;
            Font = new Font(ModernTheme.FontFamilyName, ModernTheme.DefaultFontSize);
            DoubleBuffered = true;
            Icon = owner.Icon;
            ModernWindowFrame.Apply(this);

            InitializeModernTitleBar();

            labelMessage = new Label
            {
                Name = "labelMessage",
                Text = message,
                AutoSize = false,
                Location = new Point(ModernTheme.MessageDialogContentLeft, ModernTheme.MessageDialogContentTop),
                Size = new Size(
                    ClientSize.Width - ModernTheme.MessageDialogContentLeft - ModernTheme.MessageDialogContentRightMargin,
                    ClientSize.Height - ModernTheme.MessageDialogContentTop - ModernTheme.MessageDialogButtonAreaHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ForeColor = ModernTheme.TextColor,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };

            buttonOk = ModernTheme.CreateDialogPrimaryButton("buttonOk", "OK");
            buttonOk.DialogResult = DialogResult.OK;
            ModernTheme.PositionSingleDialogButton(this, buttonOk, true);

            Controls.Add(labelMessage);
            Controls.Add(buttonOk);

            AcceptButton = buttonOk;
            CancelButton = buttonOk;
        }

        public static DialogResult Show(Form owner, string title, string message)
        {
            using ModernMessageDialog dialog = new ModernMessageDialog(owner, title, message);
            return dialog.ShowDialog(owner);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            ModernTheme.DrawResizeGrip(e.Graphics, ClientSize);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmNcHitTest)
            {
                base.WndProc(ref m);

                Point cursorPosition = PointToClient(Cursor.Position);

                if (ModernTheme.IsInResizeGripArea(ClientSize, cursorPosition))
                {
                    m.Result = HtBottomRight;
                }

                return;
            }

            base.WndProc(ref m);
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
            buttonModernClose.Click += (sender, e) =>
            {
                DialogResult = DialogResult.OK;
                Close();
            };

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

            ReleaseCapture();
            SendMessage(Handle, WmNclButtonDown, HtCaption, 0);
        }
    }
}