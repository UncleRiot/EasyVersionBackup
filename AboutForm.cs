using System;
using System.Drawing;
using System.Windows.Forms;

namespace EasyVersionBackup
{
    public class AboutForm : Form
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private Bitmap CreateCircularMolotovImage()
        {
            Bitmap output = new Bitmap(90, 90);

            using Graphics graphics = Graphics.FromImage(output);
            graphics.Clear(Color.Transparent);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using Stream? stream = typeof(AboutForm).Assembly.GetManifestResourceStream("EasyVersionBackup.Ressources.molotov.jpg");

            if (stream == null)
            {
                using Pen fallbackPen = new Pen(ModernTheme.AccentColor, 2);
                graphics.DrawEllipse(fallbackPen, 1, 1, 88, 88);
                return output;
            }

            using Image sourceImage = Image.FromStream(stream);
            using System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();

            path.AddEllipse(4, 4, 82, 82);
            graphics.SetClip(path);

            float scale = Math.Max(82f / sourceImage.Width, 82f / sourceImage.Height);
            int scaledWidth = (int)(sourceImage.Width * scale);
            int scaledHeight = (int)(sourceImage.Height * scale);
            int x = 4 + (82 - scaledWidth) / 2;
            int y = 4 + (82 - scaledHeight) / 2;

            graphics.DrawImage(sourceImage, x, y, scaledWidth, scaledHeight);
            graphics.ResetClip();

            using Pen borderPen = new Pen(ModernTheme.AccentColor, 2);
            graphics.DrawEllipse(borderPen, 4, 4, 82, 82);

            return output;
        }

        public AboutForm()
        {
            Text = "About";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(475, 315);
            BackColor = ModernTheme.WindowBackColor;
            Font = new Font(ModernTheme.FontFamilyName, ModernTheme.DefaultFontSize);
            DoubleBuffered = true;
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            ModernWindowFrame.Apply(this);

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
                Text = "About",
                AutoSize = false,
                Location = new Point(ModernTheme.TitleBarTextLeft, 0),
                Size = new Size(ClientSize.Width - 66, ModernTheme.TitleBarHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = ModernTheme.TextColor,
                Font = new Font(ModernTheme.FontFamilyName, ModernTheme.TitleFontSize, FontStyle.Regular)
            };

            Button buttonModernClose = CreateModernTitleBarButton(
                "buttonModernClose",
                "Close",
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

            PictureBox pictureBoxMolotov = new PictureBox
            {
                Image = CreateCircularMolotovImage(),
                Size = new Size(90, 90),
                Location = new Point(20, 57),
                SizeMode = PictureBoxSizeMode.CenterImage,
                BackColor = Color.Transparent
            };

            Label labelTitle = new Label
            {
                Text = "EasyVersionBackup",
                Font = new Font(ModernTheme.FontFamilyName, 11F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(130, 57),
                ForeColor = ModernTheme.TextColor,
                BackColor = Color.Transparent
            };

            Label labelCopyright = new Label
            {
                Text = "(c) Daniel Capilla",
                AutoSize = true,
                Location = new Point(130, 92),
                ForeColor = ModernTheme.TextColor,
                BackColor = Color.Transparent
            };

            Label labelVersion = new Label
            {
                Text = "Version: " + GetApplicationVersionText(),
                AutoSize = true,
                Location = new Point(130, 117),
                ForeColor = ModernTheme.TextColor,
                BackColor = Color.Transparent
            };

            LinkLabel linkLabelUpdate = new LinkLabel
            {
                Text = "Can not connect to Github",
                AutoSize = true,
                Location = new Point(130, 142),
                LinkColor = ModernTheme.AccentColor,
                ActiveLinkColor = ModernTheme.AccentHoverColor,
                VisitedLinkColor = ModernTheme.AccentColor,
                DisabledLinkColor = ModernTheme.DisabledTextColor,
                ForeColor = ModernTheme.TextColor,
                BackColor = Color.Transparent
            };

            linkLabelUpdate.LinkClicked += (sender, e) =>
            {
                string downloadUrl = linkLabelUpdate.Tag?.ToString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(downloadUrl))
                {
                    return;
                }

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = downloadUrl,
                    UseShellExecute = true
                });
            };

            LinkLabel linkLabelGithub = new LinkLabel
            {
                Text = "https://github.com/UncleRiot/EasyVersionBackup",
                AutoSize = true,
                Location = new Point(130, 167),
                LinkColor = ModernTheme.AccentColor,
                ActiveLinkColor = ModernTheme.AccentHoverColor,
                VisitedLinkColor = ModernTheme.AccentColor,
                BackColor = Color.Transparent
            };

            linkLabelGithub.LinkClicked += (sender, e) =>
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = linkLabelGithub.Text,
                    UseShellExecute = true
                });
            };

            Label labelKoFiText = new Label
            {
                Text = "EasyVersionBackup is free to use." + Environment.NewLine +
                       "If this tool saves you time, you can support development here:",
                AutoSize = false,
                Location = new Point(20, 205),
                Size = new Size(430, 40),
                ForeColor = ModernTheme.TextColor,
                BackColor = Color.Transparent
            };

            PictureBox pictureBoxKoFi = new PictureBox
            {
                Name = "pictureBoxKoFi",
                Image = CreateKoFiImage(),
                Size = new Size(179, 42),
                Location = new Point(20, 251),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            pictureBoxKoFi.Click += (sender, e) =>
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://ko-fi.com/uncleriot",
                    UseShellExecute = true
                });
            };

            Button buttonOk = new Button
            {
                Text = "OK",
                Size = ModernTheme.DialogButtonSize,
                Location = new Point(388, 276),
                DialogResult = DialogResult.OK,
                FlatStyle = FlatStyle.Flat,
                BackColor = ModernTheme.AccentColor,
                ForeColor = ModernTheme.DarkTextColor,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = ModernTheme.DialogPrimaryButtonTextPadding,
                UseCompatibleTextRendering = true,
                UseVisualStyleBackColor = false
            };

            buttonOk.FlatAppearance.BorderSize = 0;
            buttonOk.FlatAppearance.MouseOverBackColor = ModernTheme.AccentHoverColor;
            buttonOk.FlatAppearance.MouseDownBackColor = ModernTheme.ControlBackColor;

            Controls.Add(panelModernTitleBar);
            Controls.Add(pictureBoxMolotov);
            Controls.Add(labelTitle);
            Controls.Add(labelCopyright);
            Controls.Add(labelVersion);
            Controls.Add(linkLabelUpdate);
            Controls.Add(linkLabelGithub);
            Controls.Add(labelKoFiText);
            Controls.Add(pictureBoxKoFi);
            Controls.Add(buttonOk);

            panelModernTitleBar.BringToFront();

            AcceptButton = buttonOk;

            UpdateGitHubStatusAsync(linkLabelUpdate);
        }
        private Image? CreateKoFiImage()
        {
            using System.IO.Stream? stream = typeof(AboutForm).Assembly.GetManifestResourceStream("EasyVersionBackup.Ressources.ko-fi.png");

            if (stream == null)
            {
                return null;
            }

            using Image sourceImage = Image.FromStream(stream);
            return new Bitmap(sourceImage);
        }

        private Button CreateModernTitleBarButton(string name, string text, Point location)
        {
            Button button = new Button
            {
                Name = name,
                Text = string.Empty,
                Tag = text,
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

                if (button.Tag?.ToString() == "Close")
                {
                    e.Graphics.DrawLine(pen, 13, 11, 23, 21);
                    e.Graphics.DrawLine(pen, 23, 11, 13, 21);
                }
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
        private string GetApplicationVersionText()
        {
            System.Reflection.Assembly assembly = typeof(AboutForm).Assembly;

            foreach (object attribute in assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false))
            {
                if (attribute is System.Reflection.AssemblyInformationalVersionAttribute informationalVersionAttribute &&
                    !string.IsNullOrWhiteSpace(informationalVersionAttribute.InformationalVersion))
                {
                    return informationalVersionAttribute.InformationalVersion.Split('+')[0];
                }
            }

            Version? version = assembly.GetName().Version;

            if (version == null)
            {
                return "unknown";
            }

            return $"{version.Major}.{version.Minor}.{version.Build}";
        }
        private async void UpdateGitHubStatusAsync(LinkLabel linkLabelUpdate)
        {
            VersionHelperGitResult result = await VersionHelperGit.CheckForUpdateAsync(GetApplicationVersionText());

            if (IsDisposed)
            {
                return;
            }

            linkLabelUpdate.Tag = string.Empty;
            linkLabelUpdate.Links.Clear();

            if (!result.CanConnectToGitHub)
            {
                linkLabelUpdate.Text = "Can not connect to Github";
                linkLabelUpdate.LinkBehavior = LinkBehavior.NeverUnderline;
                return;
            }

            if (!result.UpdateAvailable)
            {
                linkLabelUpdate.Text = "No new updates";
                linkLabelUpdate.LinkBehavior = LinkBehavior.NeverUnderline;
                return;
            }

            linkLabelUpdate.Text = "Update available: " + result.LatestVersion;
            linkLabelUpdate.Tag = result.DownloadUrl;
            linkLabelUpdate.LinkBehavior = LinkBehavior.SystemDefault;
            linkLabelUpdate.Links.Add(0, linkLabelUpdate.Text.Length);
        }
    }
}