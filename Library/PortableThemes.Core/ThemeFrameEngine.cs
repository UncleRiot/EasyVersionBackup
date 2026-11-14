// SEMANTIC ANCHOR [FRAME SSOT AND LIFECYCLE]: One engine per window owns exactly two
// time-keyed low-resolution backgrounds and one interpolated output buffer. All theme
// surfaces use this output, never independent timers or independently sampled animation.
// Presentation-only: no input interception, model mutation or application setting writes.
using System.Diagnostics;

namespace PortableThemes.Core;

public sealed class ThemeFrameEngine : IDisposable
{
    private const double KeyIntervalSeconds = 0.125;
    private readonly ThemeDefinition _theme;
    private readonly PreparedEffect[] _effects;
    private ThemeOptions _options;
    private byte[] _first = [];
    private byte[] _second = [];
    private byte[] _interpolated = [];
    private ThemeFrame? _frame;
    private long _firstIndex = long.MinValue;
    private int _width, _height;
    private bool _autoLow;
    private int _slowSamples, _fastSamples;
    private double _averageRenderMs, _averageFps, _lastElapsed = -1;
    private bool _disposed;

    public ThemeFrameEngine(ThemeDefinition theme, ThemeOptions? options = null)
    {
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
        _effects = theme.Effects.Select(PreparedEffect.From).ToArray();
        _options = (options ?? new ThemeOptions()).Normalize();
    }

    public ThemeOptions Options => _options;
    public ThemeContrastPalette Contrast => ThemeContrastPalette.From(_theme);
    public ThemeDiagnostics Diagnostics => new(_averageRenderMs, _averageFps, _width, _height,
        (long)(_first.Length + _second.Length + _interpolated.Length));

    public void SetOptions(ThemeOptions options)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _options = (options ?? throw new ArgumentNullException(nameof(options))).Normalize();
        _firstIndex = long.MinValue;
        _lastElapsed = -1;
    }

    public ThemeFrame Render(double elapsedSeconds, int surfaceWidth, int surfaceHeight)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (surfaceWidth <= 0 || surfaceHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(surfaceWidth), "Surface dimensions must be positive.");
        if (!double.IsFinite(elapsedSeconds)) throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));

        var (width, height) = DetermineSize(surfaceWidth, surfaceHeight);
        if (width != _width || height != _height)
        {
            _width = width; _height = height;
            _first = new byte[width * height * 4];
            _second = new byte[width * height * 4];
            _interpolated = new byte[width * height * 4];
            _frame = new ThemeFrame(_interpolated, width, height);
            _firstIndex = long.MinValue;
        }

        double animationTime = _options.ReducedMotion ? 0 : Math.Max(0, elapsedSeconds) * _options.Speed;
        long index = (long)Math.Floor(animationTime / KeyIntervalSeconds);
        double fraction = _options.ReducedMotion ? 0 :
            Math.Clamp(animationTime / KeyIntervalSeconds - index, 0, 1);
        if (_firstIndex != index)
        {
            var watch = Stopwatch.StartNew();
            if (_firstIndex != long.MinValue && _firstIndex + 1 == index)
            {
                // Reuse previous second keyframe. Only one new procedural pass is needed.
                (_first, _second) = (_second, _first);
                RenderKey(_second, (index + 1) * KeyIntervalSeconds);
            }
            else
            {
                RenderKey(_first, index * KeyIntervalSeconds);
                if (!_options.ReducedMotion)
                    RenderKey(_second, (index + 1) * KeyIntervalSeconds);
            }
            _firstIndex = index;
            watch.Stop();
            _averageRenderMs = _averageRenderMs == 0 ? watch.Elapsed.TotalMilliseconds :
                _averageRenderMs * 0.85 + watch.Elapsed.TotalMilliseconds * 0.15;
            AutoQuality();
        }

        // Linear temporal interpolation smooths 8 Hz procedural updates at the host's
        // selected 30/60 FPS while avoiding heavy per-frame domain-noise computation.
        if (fraction <= 0 || _options.ReducedMotion)
            Buffer.BlockCopy(_first, 0, _interpolated, 0, _first.Length);
        else
            for (int i = 0; i < _interpolated.Length; i++)
                _interpolated[i] = (byte)Math.Clamp((int)Math.Round(
                    _first[i] + (_second[i] - _first[i]) * fraction), 0, 255);

        if (_lastElapsed >= 0 && elapsedSeconds > _lastElapsed)
        {
            double instantFps = Math.Min(1000, 1 / (elapsedSeconds - _lastElapsed));
            _averageFps = _averageFps == 0 ? instantFps : _averageFps * 0.85 + instantFps * 0.15;
        }
        _lastElapsed = elapsedSeconds;
        return _frame!;
    }

    private (int Width, int Height) DetermineSize(int surfaceWidth, int surfaceHeight)
    {
        int maxWidth = _options.Quality switch
        {
            ThemeRenderQuality.Low => 200,
            ThemeRenderQuality.High => 480,
            ThemeRenderQuality.Auto when _autoLow => 200,
            _ => 320
        };
        int maxPixels = _options.Quality switch
        {
            ThemeRenderQuality.Low => 20_000,
            ThemeRenderQuality.High => 96_000,
            ThemeRenderQuality.Auto when _autoLow => 20_000,
            _ => 48_000
        };
        double ratio = Math.Min(1, Math.Min(maxWidth / (double)surfaceWidth,
            Math.Sqrt(maxPixels / ((double)surfaceWidth * surfaceHeight))));
        return (Math.Max(2, (int)Math.Round(surfaceWidth * ratio)),
                Math.Max(2, (int)Math.Round(surfaceHeight * ratio)));
    }

    private void AutoQuality()
    {
        if (_options.Quality != ThemeRenderQuality.Auto) return;
        if (_averageRenderMs > 28)
        {
            _slowSamples++;
            _fastSamples = 0;
            if (_slowSamples >= 3 && !_autoLow) { _autoLow = true; _firstIndex = long.MinValue; }
        }
        else if (_averageRenderMs < 8)
        {
            _fastSamples++;
            _slowSamples = 0;
            if (_fastSamples >= 50 && _autoLow)
            {
                _autoLow = false;
                _firstIndex = long.MinValue;
                _fastSamples = 0;
            }
        }
    }

    private void RenderKey(byte[] destination, double time)
    {
        ThemeColor background = _theme.Background.Over(ThemeColor.Black);
        ThemeColor highlight = _theme.Highlight.Over(background);
        ThemeColor accent = _theme.Accent.Over(background);
        ThemeColor flame = _theme.FlameBase.Over(background);
        for (int y = 0; y < _height; y++)
        {
            double v = (y + .5) / _height;
            ThemeColor backdrop = _theme.GlassSurfaceEnabled
                ? _theme.GlassTop.Interpolate(_theme.GlassBottom, v).Over(background)
                : background;
            for (int x = 0; x < _width; x++)
            {
                double u = (x + .5) / _width;
                ThemeColor pixel = backdrop;
                for (int n = 0; n < _effects.Length; n++)
                {
                    PreparedEffect effect = _effects[n];
                    if (effect.Kind == ThemeEffectKind.GlowDot) continue;
                    var (density, warmth) = EffectAt(effect, u, v, time);
                    double alpha = Math.Clamp(density * _options.Intensity, 0, 0.85);
                    if (alpha <= 0.0001) continue;
                    ThemeColor to = effect.Kind switch
                    {
                        ThemeEffectKind.Ember or ThemeEffectKind.Flame => flame.Interpolate(highlight, warmth),
                        ThemeEffectKind.Aurora => accent.Interpolate(highlight, warmth),
                        ThemeEffectKind.DeepCurrent => accent.Interpolate(highlight, warmth),
                        _ => _theme.GlassBottom.Over(background).Interpolate(highlight, warmth)
                    };
                    pixel = to.WithAlpha((byte)Math.Clamp((int)Math.Round(alpha * 255), 0, 255)).Over(pixel);
                }
                int position = (y * _width + x) * 4;
                destination[position] = pixel.B;
                destination[position + 1] = pixel.G;
                destination[position + 2] = pixel.R;
                destination[position + 3] = 255;
            }
        }
        foreach (var effect in _effects)
            if (effect.Kind == ThemeEffectKind.GlowDot)
                PaintStars(destination, effect, time, highlight);
    }

    private (double Alpha, double Warmth) EffectAt(PreparedEffect e, double u, double v, double time)
    {
        if (e.Kind == ThemeEffectKind.Gradient)
            return ((.08 + .05 * Math.Sin(u * 7 + v * 5 + time * e.Speed * .32)) * e.Opacity, .5);
        double t = time * e.Speed;
        double scale = e.Scale;
        double opacity = e.Opacity;
        double nx = u * scale, ny = v * scale * 0.73;
        double warpX = ThemeNoise.Fractal(nx * .63 + t * .12, ny * .71 - t * .16, e.Seed, 2) - .5;
        double warpY = ThemeNoise.Fractal(nx * .69 - t * .10 + 17, ny * .64 + t * .13, e.Seed + 11, 2) - .5;
        double fx = nx + warpX * 1.5 + t * .055;
        double fy = ny + warpY * 1.45 - t * .09;
        double large = ThemeNoise.Fractal(fx, fy, e.Seed + 23, 3);
        double fine = ThemeNoise.Fractal(fx * 2.15 - t * .03, fy * 2.15 + t * .05, e.Seed + 47, 2);
        double ribbon = Math.Pow(Math.Clamp(1 - Math.Abs(large - .53) * 6.5, 0, 1), 1.75);
        double haze = ThemeNoise.SmoothStep(.34, .74, large * .74 + fine * .26);
        double warmth = Math.Clamp(.22 + .6 * ribbon + .22 * fine, 0, 1);
        bool rising = e.Rising;
        if (rising)
        {
            double centerX = e.CenterX;
            double spreadX = e.SpreadX;
            double baseY = e.BaseY;
            double height = e.RiseHeight;
            double taper = e.Taper;
            double risingDistance = Math.Clamp((baseY - v) / height, 0, 1.5);
            double flow = Math.Min(risingDistance, 1);
            double shifted = u + warpX * (.10 + flow * .14) +
                             Math.Sin(t * 1.18 + v * 9 + warpY * 4) * flow * .025;
            double width = Math.Max(.015, spreadX * (1 - taper * flow * .32));
            double envelope = 1 - ThemeNoise.SmoothStep(.72, 1.24, Math.Abs(shifted - centerX) / width);
            double tongues = .16 * Math.Sin(shifted * 31.5 - t * 1.12) +
                             .11 * Math.Sin(shifted * 73 + warpY * 5.8 + t * 1.45);
            double tipHeight = Math.Clamp((.53 + tongues + .23 * (large - .5) +
                              .19 * (fine - .5)) * (.5 + .5 * envelope), .12, 1.05);
            double tip = 1 - ThemeNoise.SmoothStep(tipHeight - .14, tipHeight + .02, risingDistance);
            double glow = Math.Clamp((baseY - v) / .13, 0, 1);
            double baseGlow = (1 - glow) * e.BaseGlow;
            double density = Math.Clamp(envelope * tip * (.24 + .48 * haze + .5 * ribbon) +
                                        baseGlow, 0, 1);
            return (density * opacity, Math.Clamp(warmth + (1 - flow) * .15, 0, 1));
        }
        return e.Kind switch
        {
            ThemeEffectKind.Smoke =>
                (Math.Clamp(Math.Pow(Math.Abs(2 * u - 1), e.EdgeStrength) *
                 (.66 + .34 * Math.Sin((v + .22) * Math.PI * .88)) *
                 (.13 + .59 * haze + .56 * ribbon), 0, 1) * opacity, warmth),
            ThemeEffectKind.Aurora =>
                (Math.Clamp(Math.Exp(-Math.Pow((v - (.32 + .18 * Math.Sin(u * 9.4 + t * .39 + warpY * 5))) /
                     (.13 + .08 * fine), 2)) * (.25 + .55 * haze + .55 * ribbon), 0, 1) * opacity,
                     Math.Clamp(.2 + .8 * Math.Sin(u * 4.9 + t * .24) * .5 + .4, 0, 1)),
            ThemeEffectKind.DeepCurrent =>
                (Math.Clamp((.18 + .62 * Math.Pow(Math.Max(0, Math.Sin(u * 18 + v * 6 +
                 3 * large + t * .21)), 7)) * (1 - v * .42) *
                 (.45 + .55 * haze), 0, 1) * opacity, warmth),
            ThemeEffectKind.Gradient =>
                ((.08 + .05 * Math.Sin(u * 7 + v * 5 + t * .32)) * opacity, warmth),
            _ => (0, 0)
        };
    }

    private void PaintStars(byte[] destination, PreparedEffect effect, double time, ThemeColor color)
    {
        int count = Math.Clamp(effect.Count == 0 ? 60 : effect.Count, 0, 180);
        double strength = effect.Opacity * _options.Intensity;
        for (int i = 0; i < count; i++)
        {
            double px = ThemeNoise.Hash(i, 0, effect.Seed) * (_width - 1);
            double py = ThemeNoise.Hash(i, 1, effect.Seed) * (_height - 1);
            double pulse = .15 + .85 * (.5 + .5 * Math.Sin(time * 1.7 + i * 2.3));
            double radius = 1.2 + ThemeNoise.Hash(i, 2, effect.Seed) * 3.2;
            int left = Math.Max(0, (int)(px - radius * 3));
            int right = Math.Min(_width - 1, (int)(px + radius * 3));
            int top = Math.Max(0, (int)(py - radius * 3));
            int bottom = Math.Min(_height - 1, (int)(py + radius * 3));
            for (int y = top; y <= bottom; y++)
                for (int x = left; x <= right; x++)
                {
                    double dx = (x - px) / radius, dy = (y - py) / radius;
                    double alpha = Math.Exp(-(dx * dx + dy * dy) * 1.3) * pulse * strength * .64;
                    if (alpha < .004) continue;
                    int index = (y * _width + x) * 4;
                    var backdrop = new ThemeColor(255, destination[index + 2], destination[index + 1], destination[index]);
                    var result = color.WithAlpha((byte)(Math.Clamp(alpha, 0, 1) * 255)).Over(backdrop);
                    destination[index] = result.B;
                    destination[index + 1] = result.G;
                    destination[index + 2] = result.R;
                }
        }
    }

    // Prepared settings avoid repeated JSON parameter reads and string allocations per pixel.
    // All parameters still originate from the parsed JSON theme (SSOT).
    private readonly record struct PreparedEffect(
        ThemeEffectKind Kind, int Seed, int Count, double Speed, double Scale,
        double Opacity, double EdgeStrength, double CenterX, double SpreadX,
        double BaseY, double RiseHeight, double Taper, double BaseGlow, bool Rising)
    {
        internal static PreparedEffect From(ThemeEffect e) => new(
            e.Kind, e.Seed, e.Count,
            e.Number("Speed", .35, .01, 3), e.Number("Scale", 4, 1, 8),
            e.Number("Opacity", .75, 0, 1), e.Number("EdgeStrength", 1.3, .6, 3.5),
            e.Number("CenterX", .5, 0, 1), e.Number("SpreadX", .38, .08, .48),
            e.Number("BaseHeightRatio", .98, .55, 1),
            e.Number("RiseHeightRatio", .82, .15, 1),
            e.Number("Taper", .35, 0, 1.5), e.Number("BaseGlow", .2, 0, 1),
            e.Kind is ThemeEffectKind.Ember or ThemeEffectKind.Flame ||
            (e.Kind == ThemeEffectKind.Smoke &&
                e.Text("Layout").Equals("Campfire", StringComparison.OrdinalIgnoreCase)));
    }

    public void Dispose()
    {
        _disposed = true;
        _first = [];
        _second = [];
        _interpolated = [];
        _frame = null;
    }
}
