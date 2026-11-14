// SEMANTIC ANCHOR [COLOR/CONTRAST SSOT]: All adapters use this exact alpha-composition and
// WCAG relative-luminance calculation. Contrast is evaluated against the composited surface,
// not an alpha-only JSON color. Host-specific controls must not reimplement these rules.
using System.Globalization;

namespace PortableThemes.Core;

public readonly record struct ThemeColor(byte A, byte R, byte G, byte B)
{
    public static ThemeColor Black => new(255, 0, 0, 0);
    public static ThemeColor White => new(255, 255, 255, 255);

    // Original GlueDock colors use #RRGGBB and #AARRGGBB, not CSS #RRGGBBAA.
    public static ThemeColor Parse(string value)
    {
        if (!TryParse(value, out ThemeColor color))
            throw new FormatException($"Invalid ARGB color: {value}");
        return color;
    }

    public static bool TryParse(string? value, out ThemeColor color)
    {
        color = default;
        if (value is null || (value.Length != 7 && value.Length != 9) || value[0] != '#')
            return false;
        if (!uint.TryParse(value.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture,
                           out uint parsed)) return false;
        color = value.Length == 7
            ? new ThemeColor(255, (byte)(parsed >> 16), (byte)(parsed >> 8), (byte)parsed)
            : new ThemeColor((byte)(parsed >> 24), (byte)(parsed >> 16), (byte)(parsed >> 8), (byte)parsed);
        return true;
    }

    public ThemeColor Over(ThemeColor background)
    {
        // All color/layout calculations use straight-alpha compositing; opaque output is
        // obtained when the backdrop is opaque, as it is in the frame renderer.
        double foregroundAlpha = A / 255.0;
        double backgroundAlpha = background.A / 255.0;
        double outAlpha = foregroundAlpha + backgroundAlpha * (1 - foregroundAlpha);
        if (outAlpha <= 0) return new ThemeColor(0, 0, 0, 0);
        byte Blend(byte f, byte b) => (byte)Math.Clamp((int)Math.Round(
            (f * foregroundAlpha + b * backgroundAlpha * (1 - foregroundAlpha)) / outAlpha), 0, 255);
        return new ThemeColor((byte)Math.Clamp((int)Math.Round(outAlpha * 255), 0, 255),
                              Blend(R, background.R), Blend(G, background.G), Blend(B, background.B));
    }

    public ThemeColor WithAlpha(byte alpha) => this with { A = alpha };

    public ThemeColor Interpolate(ThemeColor to, double fraction)
    {
        double t = Math.Clamp(fraction, 0, 1);
        byte Mix(byte x, byte y) => (byte)Math.Clamp((int)Math.Round(x + (y - x) * t), 0, 255);
        return new ThemeColor(Mix(A, to.A), Mix(R, to.R), Mix(G, to.G), Mix(B, to.B));
    }

    public double Luminance()
    {
        static double Channel(byte channel)
        {
            double s = channel / 255.0;
            return s <= 0.04045 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
        }
        return 0.2126 * Channel(R) + 0.7152 * Channel(G) + 0.0722 * Channel(B);
    }

    public double ContrastRatio(ThemeColor other)
    {
        // The caller supplies opaque/composited colors. Alpha-only contrast is undefined.
        double a = Luminance(), b = other.Luminance();
        return (Math.Max(a, b) + 0.05) / (Math.Min(a, b) + 0.05);
    }

    public string Hex => $"#{A:X2}{R:X2}{G:X2}{B:X2}";
}

public sealed record ThemeContrastPalette(ThemeColor Background, ThemeColor Surface,
    ThemeColor Text, ThemeColor OnSurface, ThemeColor OnAccent, ThemeColor OnHover,
    ThemeColor DisabledText, ThemeColor Accent, ThemeColor Hover, ThemeColor Border)
{
    public const double MinimumNormalTextRatio = 4.5;

    public static ThemeContrastPalette From(ThemeDefinition theme)
    {
        var background = theme.Background.Over(ThemeColor.Black);
        var surface = theme.Surface.Over(background);
        var accent = theme.Accent.Over(background);
        var hover = theme.Hover.Over(surface);
        var foreground = EnsureContrast(theme.Text, background);
        var onSurface = EnsureContrast(theme.Text, surface);
        var onAccent = EnsureContrast(theme.Text, accent);
        var onHover = EnsureContrast(theme.Text, hover);
        // Disabled icons remain legible; disabled interaction remains owned by the host.
        var disabledText = EnsureContrast(theme.Text.WithAlpha(170).Over(surface), surface);
        return new ThemeContrastPalette(background, surface, foreground, onSurface,
            onAccent, onHover, disabledText, accent, hover, theme.Border.Over(surface));
    }

    public static ThemeColor EnsureContrast(ThemeColor desired, ThemeColor background,
        double minimum = MinimumNormalTextRatio)
    {
        ThemeColor opaqueBackground = background.Over(ThemeColor.Black);
        ThemeColor candidate = desired.Over(opaqueBackground);
        if (candidate.ContrastRatio(opaqueBackground) >= minimum) return candidate;
        var white = ThemeColor.White;
        var black = ThemeColor.Black;
        return white.ContrastRatio(opaqueBackground) >= black.ContrastRatio(opaqueBackground)
            ? white : black;
    }
}
