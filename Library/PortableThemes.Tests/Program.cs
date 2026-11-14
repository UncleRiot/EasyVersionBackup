// Standalone .NET 10 regression runner: dotnet run --project Library/PortableThemes.Tests
// These are executable test cases; don't mark them as passed without actually running.
using System.Text;
using PortableThemes.Core;

int tests = 0;
void Check(bool condition, string reason)
{
    if (!condition) throw new Exception(reason);
    tests++;
}
void Reject(string json)
{
    try { ThemeDefinition.Parse(Encoding.UTF8.GetBytes(json), "Imported"); }
    catch (Exception e) when (e is InvalidDataException or System.Text.Json.JsonException) { tests++; return; }
    throw new Exception("Rejected import was accepted: " + json);
}

var catalog = new ThemeCatalog("PortableThemesTests", Path.Combine(Path.GetTempPath(),
    "PortableThemeTests-" + Guid.NewGuid().ToString("N")));
try
{
    var options = catalog.List();
    Check(options.Count >= 12, "Missing curated presets");
    Check(options.Any(x => x.Id == "Default"), "Missing default preset");
    // Semantic Anchor [EVB renderer opt-in]: Only these new procedural presets may
    // use the portable Core in EasyVersionBackup; legacy presets stay on their host engine.
    foreach (string id in new[] { "Aurora", "EmberFlow", "DeepCurrent" })
    {
        ThemeDefinition definition = catalog.Load(id);
        Check(definition.Effects.Count == 1, "Expected one procedural effect: " + id);
        using var normal = new ThemeFrameEngine(definition, new ThemeOptions(Quality: ThemeRenderQuality.Low));
        using var reduced = new ThemeFrameEngine(definition, new ThemeOptions(
            ReducedMotion: true, Quality: ThemeRenderQuality.Low));
        var first = normal.Render(0, 160, 90).Pixels.ToArray();
        var later = normal.Render(1.125, 160, 90).Pixels.ToArray();
        Check(!first.SequenceEqual(later), "Animation remains static: " + id);
        var staticFirst = reduced.Render(0, 160, 90).Pixels.ToArray();
        var staticLater = reduced.Render(12, 160, 90).Pixels.ToArray();
        Check(staticFirst.SequenceEqual(staticLater), "Reduced Motion changed frame: " + id);
    }

    foreach (ThemeOption option in options)
    {
        ThemeDefinition theme = catalog.Load(option.Id);
        Check(theme.Id == option.Id, "Preset resolution mismatch: " + option.Id);
        ThemeContrastPalette palette = ThemeContrastPalette.From(theme);
        Check(palette.Text.ContrastRatio(palette.Background) >= 4.5,
            "Background contrast: " + theme.Id);
        Check(palette.OnSurface.ContrastRatio(palette.Surface) >= 4.5,
            "Surface contrast: " + theme.Id);
        Check(palette.OnAccent.ContrastRatio(palette.Accent) >= 4.5,
            "Accent contrast: " + theme.Id);
        using var engine = new ThemeFrameEngine(theme, new ThemeOptions(Quality: ThemeRenderQuality.Low));
        var frame0 = engine.Render(0.01, 400, 200);
        var frame1 = engine.Render(0.03, 400, 200);
        Check(frame0.Width == frame1.Width && frame1.Pixels.Length == frame1.Width * frame1.Height * 4,
            "Frame geometry: " + theme.Id);
        Check(frame1.Pixels.Span[3] == 255, "Output alpha must be opaque");
        Check(engine.Diagnostics.CachedBytes == frame1.Pixels.Length * 3,
            "Expected three shared reusable frame buffers");
    }
    Check(ThemeColor.Parse("#80112233").A == 0x80, "ARGB alpha parsing");
    Check(ThemeColor.Parse("#112233").A == 255, "RGB alpha default");
    Check(ThemeColor.Black.ContrastRatio(ThemeColor.White) > 20.9, "Luminance calculation");
    Check(catalog.Load("unknown-id").IsDefault, "Unknown ID must fall back to Default");
    Reject("{}");
    Reject("{\"Name\":\"Unknown\",\"GlassEffects\":[{\"Type\":\"Background\",\"Renderer\":\"ArbitraryCode\"} ]}");
    Reject("{\"Name\":\"Bad\",\"TextColor\":\"not-a-color\"}");
    Reject("{\"Name\":\"Bad\",\"GlassEffectSchemaVersion\":2}");
    Reject("{\"Name\":\"Bad\",\"GlassEffects\":[{\"Type\":\"Background\",\"Renderer\":\"Smoke\",\"Parameters\":{\"Speed\":1e200}]}");
    string source = Path.Combine(Path.GetTempPath(), "portable-demo-" + Guid.NewGuid().ToString("N") + ".json");
    try
    {
        File.WriteAllText(source, "{\"Name\":\"Imported Smoke\",\"GlassEffects\":[{\"Type\":\"Background\",\"Renderer\":\"Smoke\",\"Enabled\":true}]}");
        var first = catalog.Import(source);
        var second = catalog.Import(source);
        Check(first.Id != second.Id, "Import must never overwrite");
        Check(catalog.Load(first.Id).Name == "Imported Smoke", "Imported theme selectable");
        Check(catalog.List().Count == options.Count + 2, "Import count");
    }
    finally { if (File.Exists(source)) File.Delete(source); }
    Console.WriteLine($"PASS: {tests} regression checks");
}
finally
{
    if (Directory.Exists(catalog.ImportedThemeDirectory))
        Directory.Delete(catalog.ImportedThemeDirectory, recursive: true);
}
