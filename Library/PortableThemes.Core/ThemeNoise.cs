// SEMANTIC ANCHOR [DETERMINISTIC PROCEDURAL NOISE]: No runtime Random instances, no
// per-pixel allocations, no dependence on wall-clock time. Normalized sample inputs yield
// matching motion in WPF, Avalonia and WinForms via the shared engine.
namespace PortableThemes.Core;

internal static class ThemeNoise
{
    internal static double Fractal(double x, double y, int seed, int octaves = 3)
    {
        double sum = 0, gain = 0.5, total = 0;
        for (int i = 0; i < octaves; i++)
        {
            sum += Value(x, y, seed + i * 31) * gain;
            total += gain;
            x *= 2.02;
            y *= 2.02;
            gain *= 0.5;
        }
        return sum / total;
    }

    internal static double Value(double x, double y, int seed)
    {
        int ix = (int)Math.Floor(x), iy = (int)Math.Floor(y);
        double tx = Smooth(x - ix), ty = Smooth(y - iy);
        double top = Hash(ix, iy, seed) * (1 - tx) + Hash(ix + 1, iy, seed) * tx;
        double bottom = Hash(ix, iy + 1, seed) * (1 - tx) + Hash(ix + 1, iy + 1, seed) * tx;
        return top * (1 - ty) + bottom * ty;
    }

    internal static double Hash(int x, int y, int seed)
    {
        uint n = unchecked((uint)x * 374761393u + (uint)y * 668265263u + (uint)seed * 1442695041u);
        n = unchecked((n ^ (n >> 13)) * 1274126177u);
        return (n ^ (n >> 16)) / (double)uint.MaxValue;
    }

    internal static double Smooth(double x) { x = Math.Clamp(x, 0, 1); return x * x * (3 - 2 * x); }
    internal static double SmoothStep(double start, double end, double x) => Smooth((x - start) / (end - start));
}
