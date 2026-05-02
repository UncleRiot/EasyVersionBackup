using System;
using System.Drawing;
using System.Windows.Forms;

namespace EasyVersionBackup
{
    public class FileErrorDialog : Form
    {
        public FileErrorDialog(string filePath, string errorMessage)
        {
            Text = "File Error";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(600, 200);
            BackColor = ModernTheme.WindowBackColor;
            Font = new Font(ModernTheme.FontFamilyName, ModernTheme.DefaultFontSize);
            ModernWindowFrame.Apply(this);

            PictureBox pictureBoxIcon = new PictureBox
            {
                Location = new Point(20, 20),
                Size = new Size(32, 32),
                Image = SystemIcons.Warning.ToBitmap(),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent
            };

            TextBox textBoxMessage = new TextBox
            {
                Location = new Point(70, 20),
                Size = new Size(500, 100),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = ModernTheme.TitleBarBackColor,
                ForeColor = ModernTheme.TextColor,
                Text = errorMessage
            };

            Button buttonAbort = new Button
            {
                Text = "Abort",
                DialogResult = DialogResult.Abort,
                Size = new Size(100, 32),
                Location = new Point(150, 140)
            };

            Button buttonRetry = new Button
            {
                Text = "Retry",
                DialogResult = DialogResult.Retry,
                Size = new Size(100, 32),
                Location = new Point(260, 140)
            };

            Button buttonIgnore = new Button
            {
                Text = "Ignore",
                DialogResult = DialogResult.Ignore,
                Size = new Size(100, 32),
                Location = new Point(370, 140)
            };

            Button buttonIgnoreAll = new Button
            {
                Text = "Ignore All",
                DialogResult = DialogResult.Yes,
                Size = new Size(100, 32),
                Location = new Point(480, 140)
            };

            StyleModernButton(buttonAbort, ModernTheme.ControlBackColor, ModernTheme.TextColor, true);
            StyleModernButton(buttonRetry, ModernTheme.AccentColor, ModernTheme.DarkTextColor, false);
            StyleModernButton(buttonIgnore, ModernTheme.ControlBackColor, ModernTheme.TextColor, true);
            StyleModernButton(buttonIgnoreAll, ModernTheme.ControlBackColor, ModernTheme.TextColor, true);

            Controls.Add(pictureBoxIcon);
            Controls.Add(textBoxMessage);
            Controls.Add(buttonAbort);
            Controls.Add(buttonRetry);
            Controls.Add(buttonIgnore);
            Controls.Add(buttonIgnoreAll);

            AcceptButton = buttonRetry;
            CancelButton = buttonAbort;
        }
        private void StyleModernButton(Button button, Color backColor, Color foreColor, bool showBorder)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;

            button.FlatAppearance.BorderColor = ModernTheme.AccentColor;
            button.FlatAppearance.BorderSize = showBorder ? 1 : 0;
            button.FlatAppearance.MouseOverBackColor = showBorder
                ? ModernTheme.ControlHoverBackColor
                : ModernTheme.AccentHoverColor;
            button.FlatAppearance.MouseDownBackColor = ModernTheme.AccentColor;
        }
    }

}