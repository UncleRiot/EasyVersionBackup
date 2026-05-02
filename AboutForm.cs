using System;
using System.Drawing;
using System.Windows.Forms;

namespace EasyVersionBackup
{
    public class AboutForm : Form
    {
        private Bitmap CreateCircularMolotovImage()
        {
            Bitmap output = new Bitmap(90, 90);

            using Graphics graphics = Graphics.FromImage(output);
            graphics.Clear(Color.Transparent);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using Stream? stream = typeof(AboutForm).Assembly.GetManifestResourceStream("EasyVersionBackup.Ressources.molotov.jpg");

            if (stream == null)
            {
                using Pen fallbackPen = new Pen(Color.Gray, 2);
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

            using Pen borderPen = new Pen(Color.Gray, 2);
            graphics.DrawEllipse(borderPen, 4, 4, 82, 82);

            return output;
        }
        public AboutForm()
        {
            Text = "About";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(475, 197);

            PictureBox pictureBoxMolotov = new PictureBox
            {
                Image = CreateCircularMolotovImage(),
                Size = new Size(90, 90),
                Location = new Point(20, 25),
                SizeMode = PictureBoxSizeMode.CenterImage
            };

            Label labelTitle = new Label
            {
                Text = "EasyVersionBackup",
                Font = new Font(Font.FontFamily, 11, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(130, 25)
            };

            Label labelCopyright = new Label
            {
                Text = "(c) Daniel Capilla",
                AutoSize = true,
                Location = new Point(130, 60)
            };

            Label labelVersion = new Label
            {
                Text = "Version: 0.9.4.4",
                AutoSize = true,
                Location = new Point(130, 85)
            };

            LinkLabel linkLabelGithub = new LinkLabel
            {
                Text = "https://github.com/UncleRiot/EasyVersionBackup",
                AutoSize = true,
                Location = new Point(130, 110)
            };

            linkLabelGithub.LinkClicked += (sender, e) =>
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = linkLabelGithub.Text,
                    UseShellExecute = true
                });
            };

            Button buttonOk = new Button
            {
                Text = "OK",
                Size = new Size(75, 25),
                Location = new Point(388, 160),
                DialogResult = DialogResult.OK
            };

            Controls.Add(pictureBoxMolotov);
            Controls.Add(labelTitle);
            Controls.Add(labelCopyright);
            Controls.Add(labelVersion);
            Controls.Add(linkLabelGithub);
            Controls.Add(buttonOk);

            AcceptButton = buttonOk;
        }
    }
}