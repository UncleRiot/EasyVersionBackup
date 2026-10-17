// Semantic Anchor: Grid keyboard navigation; keep appearance controlled by ModernTheme.
// Arrow keys cannot transfer selection between cells; Tab/Shift+Tab retain standard navigation.
// Home/End remain available to an active text editing control.
using System.Windows.Forms;

namespace EasyVersionBackup
{
    internal sealed class TabNavigationDataGridView : DataGridView
    {
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
