using System;
using System.Drawing;
using System.Windows.Forms;

namespace EasyVersionBackup
{
    public class AboutForm : Form
    {
        public AboutForm()
        {
            Text = "About";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(300, 150);

            Label labelTitle = new Label
            {
                Text = "EasyVersionBackup",
                Font = new Font(Font.FontFamily, 11, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            Label labelCopyright = new Label
            {
                Text = "(c) Daniel Capilla",
                AutoSize = true,
                Location = new Point(20, 55)
            };

            Label labelVersion = new Label
            {
                Text = "Version: 0.7",
                AutoSize = true,
                Location = new Point(20, 80)
            };

            Button buttonOk = new Button
            {
                Text = "OK",
                Size = new Size(75, 25),
                Location = new Point(200, 105),
                DialogResult = DialogResult.OK
            };

            Controls.Add(labelTitle);
            Controls.Add(labelCopyright);
            Controls.Add(labelVersion);
            Controls.Add(buttonOk);

            AcceptButton = buttonOk;
        }
    }
}