using System.Drawing;
using System.Windows.Forms;

namespace EasyVersionBackup
{
    public static class ModernWindowFrame
    {
        public static void Apply(Form form)
        {
            form.Paint += ModernWindowFrame_Paint;
            form.Resize += ModernWindowFrame_Resize;
        }

        private static void ModernWindowFrame_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Form form)
            {
                return;
            }

            using Pen borderPen = new Pen(ModernTheme.WindowBorderColor, ModernTheme.WindowBorderWidth);

            e.Graphics.DrawRectangle(
                borderPen,
                0,
                0,
                form.ClientSize.Width - 1,
                form.ClientSize.Height - 1);
        }

        private static void ModernWindowFrame_Resize(object? sender, System.EventArgs e)
        {
            if (sender is Form form)
            {
                form.Invalidate();
            }
        }
    }
}