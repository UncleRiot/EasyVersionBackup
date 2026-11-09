// Semantic Anchor [VisualThemeRuntime]: Theme selection is visual-only. AppSettings.VisualThemeId
// is the only persisted choice; ModernTheme is the SSOT for WinForms colors. The GlueDock JSON
// remains the SSOT for generated effects. The runtime must not touch backup data or enablement.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EasyVersionBackup.VisualThemes;

namespace EasyVersionBackup
{
    internal static class VisualThemeRuntime
    {
        private static readonly Color[] DefaultPalette =
        [
            Color.FromArgb(27, 40, 56), Color.FromArgb(23, 26, 33),
            Color.FromArgb(42, 71, 94), Color.FromArgb(55, 90, 120),
            Color.FromArgb(102, 192, 244), Color.FromArgb(143, 212, 255),
            Color.FromArgb(199, 213, 224), Color.FromArgb(23, 26, 33),
            Color.FromArgb(92, 104, 112)
        ];

        private static Color[] _palette = (Color[])DefaultPalette.Clone();
        internal static event EventHandler? ThemeChanged;
        internal static string ActiveId { get; private set; } = "Default";
        internal static DockTheme ActiveDefinition { get; private set; } = new();
        internal static bool HasActiveAnimation => ActiveDefinition.GlassGradientAnimationEnabled ||
            ActiveDefinition.GlassFlameEffectEnabled || ActiveDefinition.GlassStarEffectEnabled ||
            VisualThemeSmokeRenderer.IsEnabled(ActiveDefinition);

        internal static void Apply(string? requestedThemeId, Form? constructingForm = null)
        {
            // Resolve by catalog ID, never by user-controlled absolute/relative path.
            DockThemeOption? option = DockThemeService.GetAvailableThemes()
                .FirstOrDefault(item => string.Equals(item.Id, requestedThemeId, StringComparison.OrdinalIgnoreCase));
            string selectedId = option?.Id ?? "Default";
            Color[] old = _palette;
            DockTheme definition = DockThemeService.Load(selectedId);
            Color[] next = selectedId == "Default" ? (Color[])DefaultPalette.Clone() : BuildPalette(definition);
            ActiveId = selectedId;
            ActiveDefinition = definition;
            VisualThemeSmokeRenderer.Reset();
            _palette = next;
            ModernTheme.SetVisualThemeColors(next);
            ThemeChanged?.Invoke(null, EventArgs.Empty);

            // Keep the complete theme conversion within the UI thread. Do not mutate the
            // underlying settings object or rebuild data grids / filtered backup rows.
            List<Form> roots = Application.OpenForms.Cast<Form>().ToList();
            if (constructingForm is not null && !roots.Contains(constructingForm)) roots.Add(constructingForm);
            foreach (Form root in roots)
            {
                if (root.IsDisposed) continue;
                RecolorTree(root, old, next);
                root.Invalidate(true);
            }
        }

        internal static void RefreshConstructedForm(Form form)
        {
            if (ActiveId == "Default") return;
            RecolorTree(form, DefaultPalette, _palette);
            form.Invalidate(true);
        }

        private static Color[] BuildPalette(DockTheme definition)
        {
            Color background = VisualThemeRenderer.Composite(
                VisualThemeRenderer.ThemeColor(definition.DockBackgroundColor, DefaultPalette[0]),
                DefaultPalette[0]);
            Color title = VisualThemeRenderer.Composite(
                VisualThemeRenderer.ThemeColor(definition.GlassTopColor, Color.Transparent),
                VisualThemeRenderer.Composite(Color.FromArgb(60, 0, 0, 0), background));
            Color control = VisualThemeRenderer.Composite(
                VisualThemeRenderer.ThemeColor(definition.ItemBackgroundColor, Color.FromArgb(55, DefaultPalette[2])),
                background);
            Color accent = VisualThemeRenderer.Composite(
                VisualThemeRenderer.ThemeColor(definition.DragGhostBorderColor, DefaultPalette[4]),
                DefaultPalette[4]);
            Color hover = VisualThemeRenderer.Composite(
                VisualThemeRenderer.ThemeColor(definition.DragGhostBackgroundColor, Color.FromArgb(90, accent)),
                control);
            Color text = VisualThemeRenderer.Composite(
                VisualThemeRenderer.ThemeColor(definition.TextColor, DefaultPalette[6]), background);
            Color darkText = (accent.R * .299 + accent.G * .587 + accent.B * .114) > 140
                ? Color.FromArgb(23, 26, 33) : Color.White;
            Color border = VisualThemeRenderer.Composite(
                VisualThemeRenderer.ThemeColor(definition.ItemBorderColor, DefaultPalette[8]),
                background);
            return [background, title, control, hover, accent,
                VisualThemeRenderer.Composite(Color.FromArgb(50, Color.White), accent),
                text, darkText, border];
        }

        private static Color ConvertColor(Color current, Color[] previous, Color[] next)
        {
            if (current.IsEmpty || current == Color.Transparent) return current;
            for (int i = 0; i < previous.Length; i++)
                if (current.ToArgb() == previous[i].ToArgb()) return next[i];
            return current;
        }

        private static void RecolorStyle(DataGridViewCellStyle style, Color[] previous, Color[] next)
        {
            style.BackColor = ConvertColor(style.BackColor, previous, next);
            style.ForeColor = ConvertColor(style.ForeColor, previous, next);
            style.SelectionBackColor = ConvertColor(style.SelectionBackColor, previous, next);
            style.SelectionForeColor = ConvertColor(style.SelectionForeColor, previous, next);
        }

        private static void RecolorTree(Control root, Color[] previous, Color[] next)
        {
            root.BackColor = ConvertColor(root.BackColor, previous, next);
            root.ForeColor = ConvertColor(root.ForeColor, previous, next);
            if (root is Button button)
            {
                button.FlatAppearance.BorderColor = ConvertColor(button.FlatAppearance.BorderColor, previous, next);
                button.FlatAppearance.MouseOverBackColor = ConvertColor(button.FlatAppearance.MouseOverBackColor, previous, next);
                button.FlatAppearance.MouseDownBackColor = ConvertColor(button.FlatAppearance.MouseDownBackColor, previous, next);
            }
            if (root is DataGridView grid)
            {
                grid.BackgroundColor = ConvertColor(grid.BackgroundColor, previous, next);
                grid.GridColor = ConvertColor(grid.GridColor, previous, next);
                RecolorStyle(grid.DefaultCellStyle, previous, next);
                RecolorStyle(grid.ColumnHeadersDefaultCellStyle, previous, next);
                RecolorStyle(grid.RowHeadersDefaultCellStyle, previous, next);
                RecolorStyle(grid.AlternatingRowsDefaultCellStyle, previous, next);
                foreach (DataGridViewColumn column in grid.Columns)
                    if (column.HasDefaultCellStyle) RecolorStyle(column.DefaultCellStyle, previous, next);
                foreach (DataGridViewRow row in grid.Rows)
                    if (row.HasDefaultCellStyle) RecolorStyle(row.DefaultCellStyle, previous, next);
            }
            if (root is ToolStrip strip)
                foreach (ToolStripItem item in strip.Items) RecolorToolStripItem(item, previous, next);
            foreach (Control control in root.Controls) RecolorTree(control, previous, next);
        }

        private static void RecolorToolStripItem(ToolStripItem item, Color[] previous, Color[] next)
        {
            item.BackColor = ConvertColor(item.BackColor, previous, next);
            item.ForeColor = ConvertColor(item.ForeColor, previous, next);
            if (item is ToolStripDropDownItem dropdown)
                foreach (ToolStripItem child in dropdown.DropDownItems) RecolorToolStripItem(child, previous, next);
        }
    }
}
