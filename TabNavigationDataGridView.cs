// Semantic Anchor: Grid keyboard navigation; keep appearance controlled by ModernTheme.
// Arrow keys cannot transfer selection between cells; Tab/Shift+Tab retain standard navigation.
// Home/End remain available to an active text editing control.
using System.Windows.Forms;

// Semantic Anchor [TabNavigationDataGridView.cs]: Keyboard navigation overrides only intended grid navigation keys; preserve standard DataGridView editing and selection behavior.
namespace EasyVersionBackup
{
    internal sealed class TabNavigationDataGridView : DataGridView
    {
        // Semantic Anchor [Atomic grid repaint]: The animated form backdrop and all
        // DataGridView cells (including text, headers and vacant area) must be drawn
        // into the same WinForms back buffer before the frame reaches the screen.
        // This does not change cell selection, editing, events or default-theme colors.
        public TabNavigationDataGridView()
        {
            DoubleBuffered = true;
        }

        // Semantic Anchor [Grid vacant backdrop]: The area below existing rows is not a
        // data cell; paint only that vacant region to avoid covering editors or selection.
        // Cell backgrounds are composited by Form1's existing CellPainting handler.
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (VisualThemeRuntime.ActiveId == "Default") return;

            int bottom = ColumnHeadersHeight;
            foreach (DataGridViewRow row in Rows)
            {
                if (!row.Visible) continue;
                var bounds = GetRowDisplayRectangle(row.Index, false);
                if (bounds.Height > 0) bottom = System.Math.Max(bottom, bounds.Bottom);
                if (bottom >= ClientSize.Height) return;
            }

            if (bottom >= ClientSize.Height) return;
            var vacant = new System.Drawing.Rectangle(0, bottom,
                ClientSize.Width, ClientSize.Height - bottom);
            ModernWindowFrame.PaintThemeSurface(this, e.Graphics, vacant,
                ModernTheme.WindowBackColor, ModernTheme.ThemeGridEmptyBackdropOpacity);
        }

        private static bool IsNavigationArrow(Keys keyData)
        {
            Keys key = keyData & Keys.KeyCode;
            return key == Keys.Left || key == Keys.Right ||
                   key == Keys.Up || key == Keys.Down ||
                   key == Keys.Home || key == Keys.End;
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (IsNavigationArrow(keyData) && !(IsCurrentCellInEditMode &&
                ((keyData & Keys.KeyCode) == Keys.Home || (keyData & Keys.KeyCode) == Keys.End)))
            {
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        protected override bool ProcessDataGridViewKey(KeyEventArgs e)
        {
            if (IsNavigationArrow(e.KeyData) && !(IsCurrentCellInEditMode &&
                (e.KeyCode == Keys.Home || e.KeyCode == Keys.End)))
            {
                return true;
            }

            return base.ProcessDataGridViewKey(e);
        }
    }
}
