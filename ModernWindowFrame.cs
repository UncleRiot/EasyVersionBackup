// Design-Rule / UI consistency:
// Keep layout, spacing, colors, sizes, and fonts aligned with ModernTheme.
// Add new shared visual values to ModernTheme instead of hardcoding local exceptions here.
// 03.05.2026 /dc

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

// Semantic Anchor [ModernWindowFrame.cs]: Custom border/resize hit-testing must respect form client coordinates and preserve caption drag, control hit targets and existing window sizing.
namespace EasyVersionBackup
{
    public static class ModernWindowFrame
    {
        public const int WmNclButtonDown = 0xA1;
        public const int WmNcHitTest = 0x84;
        public const int HtCaption = 0x2;

        // Semantic Anchor [Shared themed backdrop]: A form owns exactly one cached effect surface.
        // All child regions sample the same window-coordinate pixels; no child starts its own
        // particle timeline. Never change z-order or input handling to display an effect.
        private sealed class ThemeSurface : IDisposable
        {
            private Bitmap? _bitmap;
            private string _themeId = string.Empty;
            private bool _dirty = true;
            private VisualThemePortableCanvas? _portableCanvas;
            private PortableThemes.Core.ThemeDefinition? _portableDefinition;

            internal void MarkDirty() => _dirty = true;

            internal void SetRunning(bool running) => _portableCanvas?.SetRunning(running);

            internal Bitmap? GetBitmap(Form form)
            {
                Size size = form.ClientSize;
                if (size.Width < 1 || size.Height < 1) return null;

                if (_bitmap is null || _bitmap.Size != size)
                {
                    _bitmap?.Dispose();
                    _bitmap = new Bitmap(size.Width, size.Height);
                    _dirty = true;
                }

                if (_themeId != VisualThemeRuntime.ActiveId ||
                    !ReferenceEquals(_portableDefinition, VisualThemeRuntime.ActivePortableDefinition))
                {
                    _themeId = VisualThemeRuntime.ActiveId;
                    _portableDefinition = VisualThemeRuntime.ActivePortableDefinition;
                    _portableCanvas?.Dispose();
                    _portableCanvas = _portableDefinition is null ? null :
                        new VisualThemePortableCanvas(_portableDefinition, VisualThemeRuntime.ActiveOptions);
                    _dirty = true;
                }

                if (_dirty)
                {
                    using Graphics graphics = Graphics.FromImage(_bitmap);
                    if (_portableCanvas is not null)
                    {
                        _portableCanvas.SetOptions(VisualThemeRuntime.ActiveOptions);
                        _portableCanvas.SetRunning(form.Visible && form.ContainsFocus &&
                            form.WindowState != FormWindowState.Minimized &&
                            VisualThemeRuntime.HasActiveAnimation);
                        _portableCanvas.Draw(graphics, new Rectangle(Point.Empty, size));
                    }
                    else
                    {
                        double seconds = VisualThemeRuntime.ActiveOptions.ReducedMotion ? 0 :
                            Environment.TickCount64 / 1000.0 * VisualThemeRuntime.ActiveOptions.Speed;
                        VisualThemeRenderer.Draw(graphics, new Rectangle(Point.Empty, size),
                            VisualThemeRuntime.ActiveDefinition, seconds);
                    }
                    _dirty = false;
                }

                return _bitmap;
            }

            public void Dispose()
            {
                _bitmap?.Dispose();
                _bitmap = null;
                _portableCanvas?.Dispose();
                _portableCanvas = null;
                _portableDefinition = null;
            }
        }

        // Accessed only by WinForms UI events on the owning UI thread.
        private static readonly Dictionary<Form, ThemeSurface> ThemeSurfaces = new();

        // Semantic Anchor [Theme sampling contract]: Opaque native controls cannot receive
        // real per-pixel alpha in WinForms. Draw a slice of the form's effect surface beneath
        // text/icons, then a semi-transparent theme tint to preserve legibility.
        internal static bool PaintThemeSurface(Control control, Graphics graphics,
            Rectangle area, Color tint, int tintOpacity)
        {
            if (VisualThemeRuntime.ActiveId == "Default" || area.Width <= 0 || area.Height <= 0)
                return false;

            Form? form = control as Form ?? control.FindForm();
            if (form is null || !ThemeSurfaces.TryGetValue(form, out ThemeSurface? surface))
                return false;

            Bitmap? image = surface.GetBitmap(form);
            if (image is null) return false;

            Point sourceOrigin = form.PointToClient(control.PointToScreen(area.Location));
            Rectangle source = new(sourceOrigin, area.Size);
            Rectangle visibleSource = Rectangle.Intersect(source, form.ClientRectangle);
            if (visibleSource.Width <= 0 || visibleSource.Height <= 0) return false;

            Rectangle destination = new(area.X + visibleSource.X - source.X,
                area.Y + visibleSource.Y - source.Y, visibleSource.Width, visibleSource.Height);
            graphics.DrawImage(image, destination, visibleSource, GraphicsUnit.Pixel);
            if (tintOpacity > 0)
            {
                using SolidBrush veil = new(Color.FromArgb(Math.Clamp(tintOpacity, 0, 255), tint));
                graphics.FillRectangle(veil, destination);
            }
            return true;
        }

        public static void Apply(Form form)
        {
            // The same form must never accumulate timers or theme frame buffers.
            if (ThemeSurfaces.ContainsKey(form)) return;
            ThemeSurface surface = new();
            ThemeSurfaces.Add(form, surface);
            form.Paint += ModernWindowFrame_Paint;
            form.Resize += ModernWindowFrame_Resize;

            // Semantic Anchor [Theme effect lifetime]: Repaint a single form-wide animation
            // texture only while the form is active. Child controls merely blit that texture.
            // Disposal releases the frame buffer, events and timer with the owning form.
            System.Windows.Forms.Timer timer = new() { Interval = 75 };
            var titlePanels = new List<Panel>();
            form.ControlAdded += (_, e) =>
            {
                if (e.Control is Panel panel && (panel.Name == "panelTitleBar" || panel.Name == "panelModernTitleBar"))
                {
                    titlePanels.Add(panel);
                    // Semantic Anchor [Title bar paint owner]: The buffered title panel draws
                    // its sampled background, icon and text atomically. Do not paint over it
                    // again in the form hook; older panels retain their original behavior.
                    if (panel is not ThemeTitleBarPanel)
                    {
                        panel.Paint += (_, paint) =>
                        {
                            if (VisualThemeRuntime.ActiveId != "Default")
                                PaintThemeSurface(panel, paint.Graphics, panel.ClientRectangle,
                                    ModernTheme.TitleBarBackColor, ModernTheme.ThemeTitleBackdropOpacity);
                        };
                    }
                }
            };
            timer.Tick += (_, _) =>
            {
                if (form.IsDisposed || !form.Visible || !form.ContainsFocus ||
                    form.WindowState == FormWindowState.Minimized || !VisualThemeRuntime.HasActiveAnimation) return;
                surface.MarkDirty();
                form.Invalidate();
                foreach (Panel panel in titlePanels.Where(panel => !panel.IsDisposed))
                {
                    panel.Invalidate();
                    // Semantic Anchor [Caption frame coherence]: The caption's child windows
                    // do not repaint automatically with their parent. Repaint themed buttons
                    // against the same cached frame without starting another animation timer.
                    foreach (Control child in panel.Controls)
                        if (child is ThemeTitleBarButton && child.Visible)
                            child.Invalidate();
                }
                // Semantic Anchor [Animated child repaint SSOT]: Invalidate only buffered
                // themed descendants against this form's shared cached frame. Do not repaint
                // native editors, dropdowns or unrelated input controls each animation tick.
                InvalidateAnimatedChildren(form);
            };
            // Semantic Anchor [Single window animation clock]: Core frames share this
            // existing timer; only its cadence changes for interpolated effects.
            void UpdateAnimationState()
            {
                timer.Interval = VisualThemeRuntime.ActivePortableDefinition is null ? 75 : 33;
                bool running = !form.IsDisposed && form.Visible && form.ContainsFocus &&
                    form.WindowState != FormWindowState.Minimized && VisualThemeRuntime.HasActiveAnimation;
                timer.Enabled = running;
                surface.SetRunning(running);
            }
            form.Shown += (_, _) => UpdateAnimationState();
            form.Activated += (_, _) => UpdateAnimationState();
            form.Deactivate += (_, _) => { timer.Stop(); surface.SetRunning(false); };
            form.VisibleChanged += (_, _) => UpdateAnimationState();
            form.Resize += (_, _) => { surface.MarkDirty(); UpdateAnimationState(); };
            System.EventHandler themeChangedHandler = (_, _) =>
            {
                // The default theme has no effect texture: free its cached bitmap.
                if (VisualThemeRuntime.ActiveId == "Default") surface.Dispose();
                else surface.MarkDirty();
                UpdateAnimationState();
            };
            VisualThemeRuntime.ThemeChanged += themeChangedHandler;
            form.Disposed += (_, _) =>
            {
                VisualThemeRuntime.ThemeChanged -= themeChangedHandler;
                timer.Stop();
                timer.Dispose();
                surface.Dispose();
                ThemeSurfaces.Remove(form);
            };
        }

        // Semantic Anchor [Animated descendant invalidation]: Dialogs may contain nested
        // content panels. Traversal changes no layout or focus; only opted-in themed controls
        // are invalidated so all use the same cached background on the owning UI thread.
        private static void InvalidateAnimatedChildren(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                if (child.IsDisposed || !child.Visible) continue;
                if (child is ThemeBackdropLabel or ThemeBackdropCheckBox or
                    ThemeBackdropPanel or ThemeBackdropDataGridView or
                    TabNavigationDataGridView)
                    child.Invalidate();
                if (child.HasChildren) InvalidateAnimatedChildren(child);
            }
        }

        private static void ModernWindowFrame_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Form form) return;

            if (VisualThemeRuntime.ActiveId != "Default" &&
                !PaintThemeSurface(form, e.Graphics, form.ClientRectangle,
                    ModernTheme.WindowBackColor, 0))
            {
                VisualThemeRenderer.Draw(e.Graphics, form.ClientRectangle,
                    VisualThemeRuntime.ActiveDefinition, Environment.TickCount64 / 1000.0);
            }

            using Pen borderPen = new Pen(ModernTheme.WindowBorderColor, ModernTheme.WindowBorderWidth);
            e.Graphics.DrawRectangle(borderPen, 0, 0,
                form.ClientSize.Width - 1, form.ClientSize.Height - 1);
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

    // Semantic Anchor [Atomic animated title bar]: Sample the owning form's one cached effect
    // frame (ModernWindowFrame is the surface SSOT) and draw icon/title in the same buffered
    // paint. Never start a second timer or alter caption drag, resize handling or button hits.
    // The original PictureBox and Label retain the authoritative image, text, font and bounds;
    // they are hidden only for non-Default themes to prevent WinForms background erasure.
    internal sealed class ThemeTitleBarPanel : Panel
    {
        private PictureBox? _titleIcon;
        private Label? _titleLabel;

        public ThemeTitleBarPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
            ControlAdded += TitleBarChildAdded;
            VisualThemeRuntime.ThemeChanged += ActiveThemeChanged;
        }

        private void TitleBarChildAdded(object? sender, ControlEventArgs e)
        {
            if (e.Control is PictureBox image &&
                (image.Name == "pictureBoxModernTitleIcon" || image.Name == "pictureBoxTitleIcon"))
            {
                _titleIcon = image;
                image.Visible = VisualThemeRuntime.ActiveId == "Default";
                Invalidate();
            }
            else if (e.Control is Label label &&
                (label.Name == "labelModernTitle" || label.Name == "labelTitle"))
            {
                _titleLabel = label;
                label.TextChanged += TitleTextChanged;
                label.FontChanged += TitleTextChanged;
                label.ForeColorChanged += TitleTextChanged;
                label.Visible = VisualThemeRuntime.ActiveId == "Default";
                Invalidate();
            }
        }

        private void TitleTextChanged(object? sender, EventArgs e) => Invalidate();

        private void ActiveThemeChanged(object? sender, EventArgs e)
        {
            bool useNativeChildren = VisualThemeRuntime.ActiveId == "Default";
            if (_titleIcon is not null) _titleIcon.Visible = useNativeChildren;
            if (_titleLabel is not null) _titleLabel.Visible = useNativeChildren;
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (!ModernWindowFrame.PaintThemeSurface(this, e.Graphics, ClientRectangle,
                    ModernTheme.TitleBarBackColor, ModernTheme.ThemeTitleBackdropOpacity))
                base.OnPaintBackground(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (VisualThemeRuntime.ActiveId == "Default") return;

            if (_titleIcon?.Image is Image image && _titleIcon.Bounds.Width > 0 &&
                _titleIcon.Bounds.Height > 0)
                e.Graphics.DrawImage(image, _titleIcon.Bounds);

            if (_titleLabel is { } label && !string.IsNullOrEmpty(label.Text))
            {
                TextRenderer.DrawText(e.Graphics, label.Text, label.Font, label.Bounds,
                    label.ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis |
                    TextFormatFlags.NoPadding | TextFormatFlags.PreserveGraphicsClipping);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                VisualThemeRuntime.ThemeChanged -= ActiveThemeChanged;
                if (_titleLabel is not null)
                {
                    _titleLabel.TextChanged -= TitleTextChanged;
                    _titleLabel.FontChanged -= TitleTextChanged;
                    _titleLabel.ForeColorChanged -= TitleTextChanged;
                }
                ControlAdded -= TitleBarChildAdded;
            }
            base.Dispose(disposing);
        }
    }

    // Semantic Anchor [Buffered caption button]: The source button still owns its action,
    // bounds, Paint glyph and hover handlers. The backdrop is sampled from the same cached
    // window frame as ThemeTitleBarPanel (SSOT); no independent timer, theme state or layout.
    // The first Paint subscription replaces Button's opaque fill before the original glyph
    // Paint subscribers run. Default retains standard WinForms button rendering.
    internal sealed class ThemeTitleBarButton : Button
    {
        private bool _pointerOver;
        private bool _pointerPressed;

        public ThemeTitleBarButton()
        {
            DoubleBuffered = true;
            Paint += PaintCaptionBackdrop;
        }

        private void PaintCaptionBackdrop(object? sender, PaintEventArgs e)
        {
            if (!ModernWindowFrame.PaintThemeSurface(this, e.Graphics, ClientRectangle,
                    ModernTheme.TitleBarBackColor, ModernTheme.ThemeTitleBackdropOpacity))
                return;

            if (!_pointerOver) return;
            Color overlay = _pointerPressed ? ModernTheme.AccentColor :
                string.Equals(Name, "buttonModernClose", StringComparison.OrdinalIgnoreCase)
                    ? ModernTheme.CloseButtonHoverColor : ModernTheme.ControlHoverBackColor;
            using SolidBrush hover = new(Color.FromArgb(_pointerPressed ? 155 : 110, overlay));
            e.Graphics.FillRectangle(hover, ClientRectangle);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _pointerOver = true;
            base.OnMouseEnter(e);
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _pointerOver = false;
            _pointerPressed = false;
            base.OnMouseLeave(e);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) _pointerPressed = true;
            base.OnMouseDown(e);
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _pointerPressed = false;
            base.OnMouseUp(e);
            Invalidate();
        }
    }

    // Semantic Anchor [Buffered modal form]: Plain Form dialogs animated by
    // ModernWindowFrame must have a buffered paint path; using this type changes
    // no modal results, hit testing, settings or confirmation semantics.
    internal sealed class ThemeBufferedForm : Form
    {
        public ThemeBufferedForm()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
        }
    }

    // Semantic Anchor [Shared decorative panel]: Paint a clipped slice of the owning
    // form's cached animation in the panel buffer without affecting any child layout,
    // focus, borders or event handling. Default uses the original WinForms background.
    internal sealed class ThemeBackdropPanel : Panel
    {
        public ThemeBackdropPanel()
        {
            DoubleBuffered = true;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (!ModernWindowFrame.PaintThemeSurface(this, e.Graphics, ClientRectangle,
                    ModernTheme.WindowBackColor, ModernTheme.ThemeGridEmptyBackdropOpacity))
                base.OnPaintBackground(e);
        }
    }

    // Semantic Anchor [Themed checkbox]: Native check state and interaction remain the
    // single source of truth; only its background is sampled from the cached form frame.
    internal sealed class ThemeBackdropCheckBox : CheckBox
    {
        public ThemeBackdropCheckBox()
        {
            DoubleBuffered = true;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (!ModernWindowFrame.PaintThemeSurface(this, e.Graphics, ClientRectangle,
                    ModernTheme.WindowBackColor, ModernTheme.ThemeWarningBackdropOpacity))
                base.OnPaintBackground(e);
        }
    }

    // Semantic Anchor [Readable inactive button]: WinForms' system disabled glyph color
    // can disappear on imported themes. Keep Enabled, click handlers and bounds unchanged;
    // override only disabled paint for animated/non-Default themes.
    internal sealed class ThemeContrastButton : Button
    {
        // Semantic Anchor [Profile disabled button contrast]: Only opted-in controls
        // override the Default theme's native disabled text. All colors still come
        // from ModernTheme; existing dialog buttons retain their prior rendering.
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool AlwaysReadableWhenDisabled { get; set; }

        public ThemeContrastButton()
        {
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (Enabled || (VisualThemeRuntime.ActiveId == "Default" && !AlwaysReadableWhenDisabled) ||
                string.IsNullOrWhiteSpace(Text)) return;

            Color surface = ModernTheme.ControlBackColor;
            using SolidBrush fill = new(surface);
            e.Graphics.FillRectangle(fill, ClientRectangle);
            if (FlatStyle == FlatStyle.Flat && FlatAppearance.BorderSize > 0)
            {
                using Pen border = new(AlwaysReadableWhenDisabled
                    ? FlatAppearance.BorderColor : ModernTheme.WindowBorderColor);
                e.Graphics.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
            }
            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle,
                ModernTheme.GetReadableButtonTextColor(surface),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
        }
    }

    // Semantic Anchor [Reusable themed grid]: The DataGridView remains authoritative for
    // cells, selection, editing and rules. Only paint background pixels are composited;
    // header, content, borders and the empty lower area use the same cached frame.
    internal sealed class ThemeBackdropDataGridView : DataGridView
    {
        public ThemeBackdropDataGridView()
        {
            DoubleBuffered = true;
        }

        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            // Semantic Anchor [Nullable paint context]: Custom backdrop painting requires
            // a valid Graphics instance; otherwise preserve DataGridView's default painting.
            if (VisualThemeRuntime.ActiveId != "Default" &&
                e.Graphics is Graphics graphics &&
                e.ColumnIndex >= 0 && e.RowIndex >= -1 &&
                e.CellBounds.Width > 0 && e.CellBounds.Height > 0)
            {
                bool header = e.RowIndex == -1;
                bool selected = !header && (e.State & DataGridViewElementStates.Selected) != 0;
                Color tint = header ? ModernTheme.ControlBackColor :
                    selected ? ModernTheme.AccentColor : ModernTheme.TitleBarBackColor;
                int opacity = header ? ModernTheme.ThemeGridHeaderBackdropOpacity :
                    selected ? ModernTheme.ThemeGridSelectionBackdropOpacity :
                    ModernTheme.ThemeGridCellBackdropOpacity;
                if (ModernWindowFrame.PaintThemeSurface(this, graphics, e.CellBounds,
                        tint, opacity))
                {
                    e.Paint(e.CellBounds, e.PaintParts &
                        ~(DataGridViewPaintParts.Background | DataGridViewPaintParts.SelectionBackground));
                    e.Handled = true;
                }
            }
            base.OnCellPainting(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (VisualThemeRuntime.ActiveId == "Default") return;
            int bottom = ColumnHeadersVisible ? ColumnHeadersHeight : 0;
            // Semantic Anchor [Visible-row paint cost]: Only inspect the displayed
            // viewport, not the entire backup-log or exclusion collection per frame.
            int firstRow = Rows.Count == 0 ? 0 : Math.Max(0, FirstDisplayedScrollingRowIndex);
            int displayedRows = DisplayedRowCount(false);
            int lastRow = Math.Min(Rows.Count, firstRow + displayedRows + 2);
            for (int index = firstRow; index < lastRow; index++)
            {
                if (!Rows[index].Visible) continue;
                Rectangle rect = GetRowDisplayRectangle(index, false);
                if (rect.Height > 0) bottom = Math.Max(bottom, rect.Bottom);
                if (bottom >= ClientSize.Height) return;
            }
            Rectangle empty = new(0, bottom, ClientSize.Width, ClientSize.Height - bottom);
            ModernWindowFrame.PaintThemeSurface(this, e.Graphics, empty,
                ModernTheme.WindowBackColor, ModernTheme.ThemeGridEmptyBackdropOpacity);
        }
    }

    // Runtime-only label: preserves Label text, tooltips and mouse behavior.
    // The default theme continues to use the unmodified Label background rendering.
    internal sealed class ThemeBackdropLabel : Label
    {
        // Semantic Anchor [Atomic themed label repaint]: Sampled effect pixels and
        // label text are composed off-screen in one paint cycle. Keep Label's native
        // text layout, hit testing and the Default theme background unchanged.
        public ThemeBackdropLabel()
        {
            DoubleBuffered = true;
        }

        // Semantic Anchor [Per-label backdrop opacity]: Individual status labels may
        // request a fully transparent sampled theme surface while existing dialogs keep
        // the current tinted readability backdrop.
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int ThemeTintOpacity { get; set; } = ModernTheme.ThemeWarningBackdropOpacity;

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Semantic Anchor [Explicit status background]: Warning/status fills remain
            // opaque unless a caller explicitly requests a transparent sampled backdrop;
            // only surfaces deriving from the central theme colors sample the shared
            // animation. Text rendering and accessibility are unchanged.
            bool themeSurface = BackColor == Color.Transparent ||
                BackColor.ToArgb() == ModernTheme.WindowBackColor.ToArgb() ||
                BackColor.ToArgb() == ModernTheme.TitleBarBackColor.ToArgb() ||
                BackColor.ToArgb() == ModernTheme.ControlBackColor.ToArgb();
            if (!themeSurface || !ModernWindowFrame.PaintThemeSurface(this, e.Graphics, ClientRectangle,
                    ModernTheme.WindowBackColor, ThemeTintOpacity))
                base.OnPaintBackground(e);
        }
    }
}