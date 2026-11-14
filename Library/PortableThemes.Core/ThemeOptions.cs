// SEMANTIC ANCHOR [MOTION POLICY]: Host settings are the only persistence authority.
// Options are visual-only; ReducedMotion freezes at t=0, inactive hosts pause clocks,
// and the renderer never creates timers or changes application data.
namespace PortableThemes.Core;

public enum ThemeRenderQuality { Auto, Low, Balanced, High }

public sealed record ThemeOptions(
    double Intensity = 1.0,
    double Speed = 1.0,
    bool ReducedMotion = false,
    ThemeRenderQuality Quality = ThemeRenderQuality.Auto,
    int TargetFps = 30,
    bool PauseWhenInactive = true)
{
    public ThemeOptions Normalize() => this with
    {
        Intensity = double.IsFinite(Intensity) ? Math.Clamp(Intensity, 0, 1) : 1,
        Speed = double.IsFinite(Speed) ? Math.Clamp(Speed, 0.1, 3) : 1,
        TargetFps = Math.Clamp(TargetFps, 5, 60),
        Quality = Enum.IsDefined(Quality) ? Quality : ThemeRenderQuality.Auto
    };
}

public readonly record struct ThemeDiagnostics(double FrameMilliseconds, double FramesPerSecond,
    int RenderWidth, int RenderHeight, long CachedBytes);

public sealed class ThemeFrame
{
    internal ThemeFrame(byte[] pixels, int width, int height)
    {
        Pixels = pixels;
        Width = width; Height = height;
    }
    // BGRA 32bpp, opaque; only valid until the next render call on this session.
    public ReadOnlyMemory<byte> Pixels { get; }
    public int Width { get; }
    public int Height { get; }
    public int Stride => Width * 4;
}
