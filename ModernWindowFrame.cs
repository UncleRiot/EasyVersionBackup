// Design-Rule / UI consistency:
// Keep layout, spacing, colors, sizes, and fonts aligned with ModernTheme.
// Add new shared visual values to ModernTheme instead of hardcoding local exceptions here.
// 03.05.2026 /dc

using System.Drawing;
using System.Windows.Forms;

namespace EasyVersionBackup
{
    public static class ModernWindowFrame
    {
        public const int WmNclButtonDown = 0xA1;
        public const int WmNcHitTest = 0x84;
        public const int HtCaption = 0x2;

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
        public static bool TryGetResizeHitTestResult(Size clientSize, Point cursorPosition, out int hitTestResult)
        {
            const int htLeft = 10;
            const int htRight = 11;
            const int htTop = 12;
            const int htTopLeft = 13;
            const int htTopRight = 14;
            const int htBottom = 15;
            const int htBottomLeft = 16;
            const int htBottomRight = 17;

            int resizeBorderSize = Math.Max(8, SystemInformation.FrameBorderSize.Width + 4);

            bool isLeft = cursorPosition.X <= resizeBorderSize;
            bool isRight = cursorPosition.X >= clientSize.Width - resizeBorderSize;
            bool isTop = cursorPosition.Y <= resizeBorderSize;
            bool isBottom = cursorPosition.Y >= clientSize.Height - resizeBorderSize;

            if (isTop && isLeft)
            {
                hitTestResult = htTopLeft;
                return true;
            }

            if (isTop && isRight)
            {
                hitTestResult = htTopRight;
                return true;
            }

            if (isBottom && isLeft)
            {
                hitTestResult = htBottomLeft;
                return true;
            }

            if (isBottom && isRight)
            {
                hitTestResult = htBottomRight;
                return true;
            }

            if (isTop)
            {
                hitTestResult = htTop;
                return true;
            }

            if (isBottom)
            {
                hitTestResult = htBottom;
                return true;
            }

            if (isLeft)
            {
                hitTestResult = htLeft;
                return true;
            }

            if (isRight)
            {
                hitTestResult = htRight;
                return true;
            }

            hitTestResult = 0;
            return false;
        }
        public static Cursor GetResizeCursor(int hitTestResult)
        {
            const int htLeft = 10;
            const int htRight = 11;
            const int htTop = 12;
            const int htTopLeft = 13;
            const int htTopRight = 14;
            const int htBottom = 15;
            const int htBottomLeft = 16;
            const int htBottomRight = 17;

            return hitTestResult switch
            {
                htLeft or htRight => Cursors.SizeWE,
                htTop or htBottom => Cursors.SizeNS,
                htTopLeft or htBottomRight => Cursors.SizeNWSE,
                htTopRight or htBottomLeft => Cursors.SizeNESW,
                _ => Cursors.Default
            };
        }


        public static bool HandleResizeHitTest(Form form, ref Message message)
        {
            if (message.Msg != WmNcHitTest)
            {
                return false;
            }

            Point cursorPosition = form.PointToClient(new Point(
                unchecked((short)(long)message.LParam),
                unchecked((short)((long)message.LParam >> 16))));

            if (!TryGetResizeHitTestResult(form.ClientSize, cursorPosition, out int resizeHitTestResult))
            {
                return false;
            }

            message.Result = resizeHitTestResult;
            return true;
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