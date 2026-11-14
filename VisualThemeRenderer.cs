// Semantic Anchor [VisualThemeRenderer]: Draw only decorative pixels from the validated GlueDock
// JSON theme model. The JSON and VisualDockThemeService own animation parameters; drawing must
// never change form hit-testing, input, backup state or the saved theme selection.
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using EasyVersionBackup.VisualThemes;

namespace EasyVersionBackup
{
    internal static class VisualThemeRenderer
    {
        private static Color ParseColor(string value, Color fallback)
        {
            if (string.IsNullOrWhiteSpace(value) || value[0] != '#') return fallback;
            string raw = value[1..];
            if (raw.Length != 6 && raw.Length != 8) return fallback;
            if (!uint.TryParse(raw, System.Globalization.NumberStyles.HexNumber,
                System.Globalization.CultureInfo.InvariantCulture, out uint argb)) return fallback;
            if (raw.Length == 6) argb |= 0xFF000000;
            return Color.FromArgb(unchecked((int)argb));
        }

        internal static Color ThemeColor(string value, Color fallback) => ParseColor(value, fallback);

        internal static Color Composite(Color foreground, Color background)
        {
            double a = foreground.A / 255.0;
            return Color.FromArgb(
                Math.Clamp((int)Math.Round(foreground.R * a + background.R * (1 - a)), 0, 255),
                Math.Clamp((int)Math.Round(foreground.G * a + background.G * (1 - a)), 0, 255),
                Math.Clamp((int)Math.Round(foreground.B * a + background.B * (1 - a)), 0, 255));
        }

        internal static void Draw(Graphics graphics, Rectangle bounds, DockTheme theme, double seconds)
        {
            if (bounds.Width <= 1 || bounds.Height <= 1) return;
            GraphicsState state = graphics.Save();
            try
            {
                graphics.SetClip(bounds);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Color baseColor = Composite(ParseColor(theme.DockBackgroundColor, Color.FromArgb(27, 40, 56)),
                    Color.FromArgb(27, 40, 56));
                using (var backgroundBrush = new SolidBrush(baseColor)) graphics.FillRectangle(backgroundBrush, bounds);
                if (theme.GlassSurfaceEnabled)
                {
                    Color top = ParseColor(theme.GlassTopColor, Color.Transparent);
                    Color bottom = ParseColor(theme.GlassBottomColor, Color.Transparent);
                    if (top.A > 0 || bottom.A > 0)
                    {
                        var x1 = Math.Clamp(theme.GlassGradientStartX, -2, 3);
                        var y1 = Math.Clamp(theme.GlassGradientStartY, -2, 3);
                        var x2 = Math.Clamp(theme.GlassGradientEndX, -2, 3);
                        var y2 = Math.Clamp(theme.GlassGradientEndY, -2, 3);
                        if (theme.GlassGradientAnimationEnabled)
                        {
                            double duration = Math.Clamp(theme.GlassGradientAnimationDurationSeconds, 0.1, 120);
                            double cycle = seconds / duration;
                            double progress = theme.GlassGradientAnimationAutoReverse
                                ? 1 - Math.Abs(cycle % 2 - 1) : cycle % 1;
                            progress = Ease(progress, theme.GlassGradientAnimationEasing,
                                theme.GlassGradientAnimationEasingMode);
                            if (string.Equals(theme.GlassGradientAnimationMode, "RotateTransform", StringComparison.OrdinalIgnoreCase))
                            {
                                double angle = progress * theme.GlassGradientAnimationRotationDegrees * Math.PI / 180;
                                double dx = x2 - x1;
                                double dy = y2 - y1;
                                double cx = (x1 + x2) / 2;
                                double cy = (y1 + y2) / 2;
                                x1 = cx - (dx * Math.Cos(angle) - dy * Math.Sin(angle)) / 2;
                                y1 = cy - (dx * Math.Sin(angle) + dy * Math.Cos(angle)) / 2;
                                x2 = 2 * cx - x1;
                                y2 = 2 * cy - y1;
                            }
                            else
                            {
                                x1 = Lerp(x1, theme.GlassGradientAnimationToStartX, progress);
                                y1 = Lerp(y1, theme.GlassGradientAnimationToStartY, progress);
                                x2 = Lerp(x2, theme.GlassGradientAnimationToEndX, progress);
                                y2 = Lerp(y2, theme.GlassGradientAnimationToEndY, progress);
                            }
                        }
                        if (theme.GlassGradientAnimationAspectCorrect)
                        {
                            double dx = x2 - x1, dy = y2 - y1;
                            double length = Math.Sqrt(dx * dx + dy * dy);
                            if (length > 0.00001)
                            {
                                dx /= length; dy /= length;
                                double span = Math.Abs(dx) * bounds.Width + Math.Abs(dy) * bounds.Height;
                                double centerX = (x1 + x2) / 2, centerY = (y1 + y2) / 2;
                                x1 = centerX - span * dx / bounds.Width / 2;
                                x2 = centerX + span * dx / bounds.Width / 2;
                                y1 = centerY - span * dy / bounds.Height / 2;
                                y2 = centerY + span * dy / bounds.Height / 2;
                            }
                        }
                        PointF start = new((float)(bounds.Left + bounds.Width * Math.Clamp(x1, -3, 4)),
                            (float)(bounds.Top + bounds.Height * Math.Clamp(y1, -3, 4)));
                        PointF end = new((float)(bounds.Left + bounds.Width * Math.Clamp(x2, -3, 4)),
                            (float)(bounds.Top + bounds.Height * Math.Clamp(y2, -3, 4)));
                        if (Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y) < 1f) end.Y += 1;
                        using LinearGradientBrush glass = new(start, end, top, bottom);
                        graphics.FillRectangle(glass, bounds);
                    }
                }
                // Semantic Anchor [Smoke layer ordering]: Procedural smoke is composited behind
                // the existing particle layers; non-smoke themes follow the original path.
                if (VisualThemeSmokeRenderer.IsEnabled(theme))
                    VisualThemeSmokeRenderer.Draw(graphics, bounds, theme);
                if (theme.GlassFlameBaseGlowEnabled)
                {
                    Color glow = ParseColor(theme.GlassFlameBaseGlowColor, Color.Transparent);
                    int height = Math.Max(1, (int)(bounds.Height * Math.Clamp(theme.GlassFlameBaseGlowHeightRatio, 0.02, 1)));
                    Rectangle glowArea = new(bounds.X, bounds.Bottom - height, bounds.Width, height);
                    Color glowStart = Color.FromArgb(0, glow.R, glow.G, glow.B);
                    Color glowEnd = Color.FromArgb(Math.Clamp((int)(glow.A * Math.Clamp(theme.GlassFlameBaseGlowOpacity, 0, 1)), 0, 255), glow);
                    using LinearGradientBrush flameBase = new(glowArea, glowStart, glowEnd, LinearGradientMode.Vertical);
                    graphics.FillRectangle(flameBase, glowArea);
                }
                if (theme.GlassFlameEffectEnabled)
                {
                    foreach (DockThemeFlameLayer flame in theme.GlassFlameLayers.Take(96))
                    {
                        double duration = Math.Clamp(flame.DurationSeconds, .3, 40);
                        double progress = ((seconds - flame.DelaySeconds) / duration) % 1;
                        if (progress < 0) progress += 1;
                        double waviness = Math.Sin(seconds * 3 + flame.X * 29);
                        float width = (float)Math.Max(4, bounds.Width * Math.Clamp(flame.WidthRatio, .004, .30));
                        float height = (float)Math.Max(10, bounds.Height * Math.Clamp(flame.HeightRatio, .08, 1.4));
                        float x = (float)(bounds.Left + bounds.Width * Math.Clamp(flame.X, 0, 1) - width / 2 +
                            bounds.Width * Math.Clamp(flame.DriftRatio, -.1, .1) * waviness);
                        float y = (float)(bounds.Top + bounds.Height * Math.Clamp(flame.BaseYRatio, 0, 1.4) - height -
                            bounds.Height * Math.Clamp(flame.RiseRatio, -.5, .9) * progress);
                        float flicker = (float)(.85 + .15 * Math.Sin(seconds * 9 + flame.X * 13));
                        RectangleF area = new(x, y, width * flicker, height);
                        Color tip = Alpha(ParseColor(flame.TipColor, Color.Transparent), flame.Opacity);
                        Color mid = Alpha(ParseColor(flame.MidColor, Color.OrangeRed), flame.Opacity * (1 - .4 * progress));
                        Color core = Alpha(ParseColor(flame.CoreColor, Color.Orange), flame.Opacity * (1 - .65 * progress));
                        if (area.Width > 1 && area.Height > 1)
                        {
                            using var brush = new LinearGradientBrush(area, tip, core, LinearGradientMode.Vertical);
                            brush.InterpolationColors = new ColorBlend
                            {
                                Colors = [tip, mid, core],
                                Positions = [0f, (float)Math.Clamp(flame.MidStop, .01, .99), 1f]
                            };
                            graphics.FillEllipse(brush, area);
                        }
                    }
                }
                if (theme.GlassStarEffectEnabled)
                {
                    foreach (DockThemeStarLayer star in theme.GlassStarLayers.Take(256))
                    {
                        double duration = Math.Clamp(star.DurationSeconds, .6, 30);
                        double phase = (seconds - star.DelaySeconds) / duration * Math.PI;
                        double pulse = (1 - Math.Cos(phase)) / 2;
                        double opacity = Lerp(star.MinimumOpacity, star.MaximumOpacity, pulse);
                        double scale = Lerp(star.MinimumScale, star.MaximumScale, pulse);
                        float size = (float)Math.Clamp(star.Size * scale, .4, 36);
                        float x = (float)(bounds.Left + bounds.Width * (star.X + Math.Sin(phase) * star.DriftXRatio));
                        float y = (float)(bounds.Top + bounds.Height * (star.Y + Math.Cos(phase) * star.DriftYRatio));
                        Color color = Alpha(ParseColor(star.Color, Color.White), opacity);
                        float halo = (float)Math.Clamp(star.GlowRadius, 0, 12);
                        using (var glowBrush = new SolidBrush(Color.FromArgb(color.A / 4, color)))
                            graphics.FillEllipse(glowBrush, x - size / 2 - halo, y - size / 2 - halo,
                                size + halo * 2, size + halo * 2);
                        using (var dotBrush = new SolidBrush(color))
                            graphics.FillEllipse(dotBrush, x - size / 2, y - size / 2, size, size);
                    }
                }
            }
            finally { graphics.Restore(state); }
        }

        private static Color Alpha(Color color, double factor) => Color.FromArgb(
            Math.Clamp((int)Math.Round(color.A * Math.Clamp(factor, 0, 1)), 0, 255), color);
        private static double Lerp(double a, double b, double t) => a + (b - a) * t;
        private static double Ease(double progress, string easing, string mode)
        {
            progress = Math.Clamp(progress, 0, 1);
            double eased = easing.ToLowerInvariant() switch
            {
                "sine" => 1 - Math.Cos(progress * Math.PI / 2),
                "quadratic" => progress * progress,
                "cubic" => progress * progress * progress,
                _ => progress
            };
            return mode switch
            {
                "EaseOut" => Math.Sqrt(progress),
                "EaseInOut" => progress < .5 ? eased / 2 : (1 + Math.Sqrt((progress - .5) * 2)) / 2,
                _ => eased
            };
        }
    }

    internal sealed class VisualThemePreviewPanel : Panel
    {
        private readonly System.Windows.Forms.Timer _timer;
        private DockTheme _theme = new();
        private string _themeId = "Default";
        private string _displayName = "Default";
        private readonly System.Diagnostics.Stopwatch _clock = new();
        private VisualThemePortableCanvas? _portableCanvas;

        internal VisualThemePreviewPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BorderStyle = BorderStyle.FixedSingle;
            _timer = new System.Windows.Forms.Timer { Interval = 75 };
            _timer.Tick += (_, _) => { if (Visible && HasAnimation) Invalidate(); };
            VisibleChanged += (_, _) =>
            {
                _timer.Enabled = Visible && HasAnimation;
                _portableCanvas?.SetRunning(_timer.Enabled);
            };
        }

        private bool HasAnimation => _themeId != "Default" && !VisualThemeRuntime.ActiveOptions.ReducedMotion &&
            (_portableCanvas is not null || _theme.GlassGradientAnimationEnabled ||
             _theme.GlassFlameEffectEnabled || _theme.GlassStarEffectEnabled ||
             VisualThemeSmokeRenderer.IsEnabled(_theme));

        internal void ShowTheme(string id, string name)
        {
            _themeId = id;
            _displayName = name;
            _theme = VisualThemeRuntime.ActiveDefinition;
            _portableCanvas?.Dispose();
            _portableCanvas = VisualThemeRuntime.ActivePortableDefinition is { } definition
                ? new VisualThemePortableCanvas(definition, VisualThemeRuntime.ActiveOptions) : null;
            _clock.Restart();
            _timer.Interval = _portableCanvas is null ? 75 : 33;
            _timer.Enabled = Visible && HasAnimation;
            _portableCanvas?.SetRunning(_timer.Enabled);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            bool isDefault = string.Equals(_themeId, "Default", StringComparison.OrdinalIgnoreCase);
            if (isDefault)
            {
                using var background = new SolidBrush(Color.FromArgb(27, 40, 56));
                e.Graphics.FillRectangle(background, ClientRectangle);
            }
            else if (_portableCanvas is not null)
                _portableCanvas.Draw(e.Graphics, ClientRectangle);
            else
                VisualThemeRenderer.Draw(e.Graphics, ClientRectangle, _theme,
                    VisualThemeRuntime.ActiveOptions.ReducedMotion ? 0 :
                    _clock.Elapsed.TotalSeconds * VisualThemeRuntime.ActiveOptions.Speed);
            Color foreground = isDefault ? Color.FromArgb(199, 213, 224) :
                VisualThemeRenderer.ThemeColor(_theme.TextColor, Color.White);
            using var titleFont = new Font(ModernTheme.FontFamilyName, 12F, FontStyle.Bold);
            TextRenderer.DrawText(e.Graphics, _displayName,
                titleFont,
                new Rectangle(20, 16, Math.Max(50, Width - 40), 28), foreground,
                TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
            Color chip = isDefault ? Color.FromArgb(42, 71, 94) :
                VisualThemeRenderer.Composite(
                    VisualThemeRenderer.ThemeColor(_theme.ItemBackgroundColor, Color.FromArgb(55, 90, 120)),
                    VisualThemeRenderer.ThemeColor(_theme.DockBackgroundColor, Color.FromArgb(27, 40, 56)));
            using var chipBrush = new SolidBrush(chip);
            e.Graphics.FillRectangle(chipBrush, 20, Math.Max(60, Height - 65), 170, 32);
            TextRenderer.DrawText(e.Graphics, "Sample backup path",
                Font, new Rectangle(28, Math.Max(69, Height - 56), 160, 22), foreground,
                TextFormatFlags.EndEllipsis);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
                _portableCanvas?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
