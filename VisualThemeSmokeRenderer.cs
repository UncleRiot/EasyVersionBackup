// Semantic Anchor [Procedural smoke backdrop]: A validated Background/Smoke theme effect is
// decorative only. Its time-varying noise is rendered into one bounded, reusable bitmap on
// the WinForms UI thread; native controls, hit testing, backup state and theme persistence
// must never depend on its pixels. Other effect renderers and the Default theme are unchanged.
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using EasyVersionBackup.VisualThemes;

namespace EasyVersionBackup
{
    internal static class VisualThemeSmokeRenderer
    {
        private const int TextureWidth = 256;
        private const int TextureHeight = 160;
        // Lower frame step for smoother motion without changing the rendering model.
        private const int FrameMilliseconds = 80;
        private static Bitmap? _texture;
        private static byte[]? _pixels;
        private static DockTheme? _cachedTheme;
        private static long _cachedFrame = -1;

        // Single source of truth for activation: a JSON background effect is not a flame layer.
        internal static bool IsEnabled(DockTheme? theme) => theme?.GlassEffects?.Any(effect =>
            effect.Enabled &&
            string.Equals(effect.Type, "Background", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(effect.Renderer, "Smoke", StringComparison.OrdinalIgnoreCase)) == true;

        internal static void Reset()
        {
            _texture?.Dispose();
            _texture = null;
            _pixels = null;
            _cachedTheme = null;
            _cachedFrame = -1;
        }

        internal static void Draw(Graphics graphics, Rectangle bounds, DockTheme theme)
        {
            if (bounds.Width < 2 || bounds.Height < 2) return;
            DockThemeEffectDefinition? effect = theme.GlassEffects?.FirstOrDefault(item =>
                item.Enabled && string.Equals(item.Type, "Background", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(item.Renderer, "Smoke", StringComparison.OrdinalIgnoreCase));
            if (effect is null) return;

            if (_texture is null)
            {
                _texture = new Bitmap(TextureWidth, TextureHeight, PixelFormat.Format32bppArgb);
                _pixels = new byte[TextureWidth * TextureHeight * 4];
            }

            long frame = Environment.TickCount64 / FrameMilliseconds;
            if (!ReferenceEquals(_cachedTheme, theme) || frame != _cachedFrame)
            {
                Render(effect, theme, frame * FrameMilliseconds / 1000.0);
                _cachedTheme = theme;
                _cachedFrame = frame;
            }

            Bitmap? texture = _texture;
            if (texture is null) return;
            InterpolationMode previous = graphics.InterpolationMode;
            try
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
                graphics.DrawImage(texture, bounds);
            }
            finally { graphics.InterpolationMode = previous; }
        }

        private static void Render(DockThemeEffectDefinition effect, DockTheme theme, double time)
        {
            if (_texture is null || _pixels is null) return;
            double opacity = Parameter(effect, "Opacity", 0.82, 0, 1);
            double speed = Parameter(effect, "Speed", 0.35, 0.03, 2);
            double scale = Parameter(effect, "Scale", 3.6, 1.5, 8);
            double edgeStrength = Parameter(effect, "EdgeStrength", 1.55, 0.6, 3.5);
            string layout = TextParameter(effect, "Layout", string.Empty);
            bool isRisingLayout =
                string.Equals(layout, "Campfire", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(layout, "Rise", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(layout, "RisingColumn", StringComparison.OrdinalIgnoreCase);
            int seed = effect.Seed;
            Color bright = VisualThemeRenderer.ThemeColor(theme.GlassHighlightColor, Color.FromArgb(30, 218, 245));
            Color dark = VisualThemeRenderer.ThemeColor(theme.GlassBottomColor, Color.FromArgb(3, 25, 46));
            double t = time * speed;

            // Semantic Anchor [Campfire rising-flame mode]: Only the Campfire JSON opts into this
            // directional smoke field. One procedural layer replaces the old ellipse particles;
            // other smoke presets keep their original palette and edge-density algorithm.
            // Read shared flame parameters once per frame, never once per pixel.
            double centerX = isRisingLayout ? Parameter(effect, "CenterX", 0.5, 0, 1) : 0;
            double spreadX = isRisingLayout ? Parameter(effect, "SpreadX", 0.29, 0.08, 0.48) : 0;
            double baseHeight = isRisingLayout ? Parameter(effect, "BaseHeightRatio", 0.98, 0.55, 1) : 0;
            double riseHeight = isRisingLayout ? Parameter(effect, "RiseHeightRatio", 0.8, 0.15, 1) : 0;
            double taper = isRisingLayout ? Parameter(effect, "Taper", 0.55, 0, 1.5) : 0;
            double baseGlow = isRisingLayout ? Parameter(effect, "BaseGlow", 0.20, 0, 1) : 0;
            Color lowColor = isRisingLayout
                ? VisualThemeRenderer.ThemeColor(theme.GlassFlameBaseGlowColor, Color.FromArgb(225, 54, 10))
                : dark;

            for (int y = 0; y < TextureHeight; y++)
            {
                double v = (y + 0.5) / TextureHeight;
                for (int x = 0; x < TextureWidth; x++)
                {
                    double u = (x + 0.5) / TextureWidth;
                    double nx = u * scale;
                    double ny = v * scale * 0.73;
                    // Domain warp + multiscale contour bands create fine, curling wisps.
                    // Unlike Flame, nothing here is drawn as an ellipse or a polygon.
                    double warpX = Fractal(nx * .63 + t * .12, ny * .71 - t * .16, seed, 2) - .5;
                    double warpY = Fractal(nx * .69 - t * .10 + 17, ny * .64 + t * .13, seed + 11, 2) - .5;
                    double fx = nx + warpX * 1.5 + t * .055;
                    double fy = ny + warpY * 1.45 - t * .09;
                    double large = Fractal(fx, fy, seed + 23, 4);
                    double fine = Fractal(fx * 2.15 - t * .03, fy * 2.15 + t * .05, seed + 47, 2);
                    double ribbon = Math.Pow(Math.Clamp(1 - Math.Abs(large - .53) * 6.5, 0, 1), 1.75);
                    double haze = SmoothStep(.34, .74, large * .74 + fine * .26);
                    double density;
                    double highlight;

                    if (isRisingLayout)
                    {
                        // Rising tongues, not ellipse sprites: noisy tips taper from a shared
                        // glowing bed. Warped coordinates add curling edges and upward motion.
                        double rise = Math.Clamp((baseHeight - v) / riseHeight, 0, 1.5);
                        double flow = Math.Min(rise, 1);
                        double shiftedX = u + warpX * (.10 + flow * .14) +
                            Math.Sin(t * 1.18 + v * 9.0 + warpY * 4.0) * flow * .025;
                        double halfWidth = Math.Max(.015, spreadX * (1 - taper * flow * .32));
                        double edgeDistance = Math.Abs(shiftedX - centerX) / halfWidth;
                        double footprint = 1 - SmoothStep(.72, 1.24, edgeDistance);
                        double tongues = .16 * Math.Sin(shiftedX * 31.5 - t * 1.12) +
                            .11 * Math.Sin(shiftedX * 73 + warpY * 5.8 + t * 1.45);
                        double tipHeight = Math.Clamp((.53 + tongues + .23 * (large - .5) +
                            .19 * (fine - .5)) * (.50 + .50 * footprint), .12, 1.05);
                        double tipMask = 1 - SmoothStep(tipHeight - .07, tipHeight + .055, rise);
                        double flowingFilaments = .16 + .53 * haze + .80 * ribbon;
                        double emberBed = 1 - SmoothStep(0, .17, Math.Abs(v - baseHeight));
                        density = Math.Clamp(footprint * tipMask * flowingFilaments *
                            (.90 - .26 * flow) + footprint * emberBed * baseGlow, 0, 1);
                        highlight = Math.Clamp(.19 + .52 * (1 - flow) + .33 * ribbon +
                            .10 * fine, 0, 1);
                    }
                    else
                    {
                        double edge = Math.Pow(Math.Abs(2 * u - 1), edgeStrength);
                        double vertical = .66 + .34 * Math.Sin((v + .22) * Math.PI * .88);
                        density = Math.Clamp(edge * vertical * (.13 + .59 * haze + .56 * ribbon), 0, 1);
                        highlight = Math.Clamp(.24 + .58 * ribbon + .28 * fine, 0, 1);
                    }
                    int index = (y * TextureWidth + x) * 4;
                    _pixels[index] = Channel(lowColor.B, bright.B, highlight);
                    _pixels[index + 1] = Channel(lowColor.G, bright.G, highlight);
                    _pixels[index + 2] = Channel(lowColor.R, bright.R, highlight);
                    _pixels[index + 3] = (byte)Math.Clamp((int)Math.Round(opacity * density * 238), 0, 235);
                }
            }

            BitmapData bits = _texture.LockBits(new Rectangle(0, 0, TextureWidth, TextureHeight),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < TextureHeight; y++)
                    Marshal.Copy(_pixels, y * TextureWidth * 4,
                        IntPtr.Add(bits.Scan0, y * bits.Stride), TextureWidth * 4);
            }
            finally { _texture.UnlockBits(bits); }
        }

        private static byte Channel(byte from, byte to, double factor) =>
            (byte)Math.Clamp((int)Math.Round(from + (to - from) * factor), 0, 255);

        private static double Parameter(DockThemeEffectDefinition effect, string name, double fallback,
            double minimum, double maximum) => effect.Parameters is not null &&
            effect.Parameters.TryGetValue(name, out JsonElement value) &&
            value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out double number) &&
            double.IsFinite(number) ? Math.Clamp(number, minimum, maximum) : fallback;

        private static string TextParameter(DockThemeEffectDefinition effect, string name, string fallback) =>
            effect.Parameters is not null &&
            effect.Parameters.TryGetValue(name, out JsonElement value) &&
            value.ValueKind == JsonValueKind.String &&
            !string.IsNullOrWhiteSpace(value.GetString())
                ? value.GetString()!.Trim()
                : fallback;

        private static double SmoothStep(double start, double end, double x)
        {
            double t = Math.Clamp((x - start) / (end - start), 0, 1);
            return t * t * (3 - 2 * t);
        }

        private static double Fractal(double x, double y, int seed, int octaves)
        {
            double sum = 0, amplitude = .5, total = 0;
            for (int octave = 0; octave < octaves; octave++)
            {
                sum += Noise(x, y, seed + octave * 31) * amplitude;
                total += amplitude;
                x *= 2.02;
                y *= 2.02;
                amplitude *= .5;
            }
            return sum / total;
        }

        private static double Noise(double x, double y, int seed)
        {
            int ix = (int)Math.Floor(x), iy = (int)Math.Floor(y);
            double tx = x - ix, ty = y - iy;
            tx = tx * tx * (3 - 2 * tx);
            ty = ty * ty * (3 - 2 * ty);
            double top = Hash(ix, iy, seed) * (1 - tx) + Hash(ix + 1, iy, seed) * tx;
            double bottom = Hash(ix, iy + 1, seed) * (1 - tx) + Hash(ix + 1, iy + 1, seed) * tx;
            return top * (1 - ty) + bottom * ty;
        }

        private static double Hash(int x, int y, int seed)
        {
            uint n = unchecked((uint)x * 374761393u + (uint)y * 668265263u + (uint)seed * 1442695041u);
            n = unchecked((n ^ (n >> 13)) * 1274126177u);
            return (n ^ (n >> 16)) / (double)uint.MaxValue;
        }
    }
}
