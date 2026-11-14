// SEMANTIC ANCHOR [HOST SESSION OWNERSHIP]: Every window owns its ThemeSession and stops
// it on close. Host settings persist the selected ID; Apply is preview-only and never saves.
using System.Diagnostics;

namespace PortableThemes.Core;

public sealed class ThemeSession : IDisposable
{
    private readonly Stopwatch _clock = new();
    private ThemeFrameEngine _engine;
    private bool _disposed;

    public ThemeSession(ThemeDefinition definition, ThemeOptions? options = null)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        _engine = new ThemeFrameEngine(definition, options);
    }

    public ThemeDefinition Definition { get; private set; }
    public ThemeOptions Options => _engine.Options;
    public ThemeContrastPalette Contrast => _engine.Contrast;
    public ThemeDiagnostics Diagnostics => _engine.Diagnostics;
    public bool IsRunning => _clock.IsRunning;

    public void Apply(ThemeDefinition definition)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(definition);
        // Prepare first: invalid selections must not destroy the current visual state.
        var replacement = new ThemeFrameEngine(definition, _engine.Options);
        _engine.Dispose();
        Definition = definition;
        _engine = replacement;
        _clock.Restart();
    }

    public void SetOptions(ThemeOptions options)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _engine.SetOptions(options);
    }

    public void Resume()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!_clock.IsRunning) _clock.Start();
    }

    public void Pause() => _clock.Stop();

    public ThemeFrame Render(int surfaceWidth, int surfaceHeight) =>
        _engine.Render(_clock.Elapsed.TotalSeconds, surfaceWidth, surfaceHeight);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _clock.Stop();
        _engine.Dispose();
    }
}
