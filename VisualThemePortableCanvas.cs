// Semantic Anchor [Portable Core bridge / one-frame ownership]: A canvas owns exactly one
// portable ThemeSession and one reusable low-resolution GDI bitmap. The existing form-wide
// ThemeSurface owns the visible full-window bitmap and its timer. Child controls only sample
// that buffer; never create an additional WinForms host, input overlay, timer or settings store.
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using PortableThemes.Core;

namespace EasyVersionBackup
{
    internal sealed class VisualThemePortableCanvas : IDisposable
    {
        private readonly ThemeSession _session;
        private Bitmap? _texture;
        private bool _disposed;

        internal VisualThemePortableCanvas(ThemeDefinition definition, ThemeOptions options)
        {
            _session = new ThemeSession(definition, options);
        }

        internal ThemeDiagnostics Diagnostics => _session.Diagnostics;

        internal void SetOptions(ThemeOptions options)
        {
            if (_disposed) return;
            if (_session.Options != options)
                _session.SetOptions(options);
        }

        internal void SetRunning(bool running)
        {
            if (_disposed) return;
            if (running && !_session.Options.ReducedMotion) _session.Resume();
            else _session.Pause();
        }

        internal void Draw(Graphics graphics, Rectangle bounds)
        {
            if (_disposed || bounds.Width < 2 || bounds.Height < 2) return;
            ThemeFrame frame = _session.Render(bounds.Width, bounds.Height);
            if (_texture is null || _texture.Width != frame.Width || _texture.Height != frame.Height)
            {
                _texture?.Dispose();
                _texture = new Bitmap(frame.Width, frame.Height, PixelFormat.Format32bppArgb);
            }

            BitmapData data = _texture.LockBits(
                new Rectangle(0, 0, frame.Width, frame.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                if (!MemoryMarshal.TryGetArray(frame.Pixels, out ArraySegment<byte> pixels) ||
                    pixels.Array is null)
                    throw new InvalidOperationException("Theme frame buffer is unavailable.");
                for (int y = 0; y < frame.Height; y++)
                    Marshal.Copy(pixels.Array, pixels.Offset + y * frame.Stride,
                        IntPtr.Add(data.Scan0, y * data.Stride), frame.Stride);
            }
            finally { _texture.UnlockBits(data); }

            InterpolationMode original = graphics.InterpolationMode;
            try
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
                graphics.DrawImage(_texture, bounds);
            }
            finally { graphics.InterpolationMode = original; }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _texture?.Dispose();
            _texture = null;
            _session.Dispose();
        }
    }
}
