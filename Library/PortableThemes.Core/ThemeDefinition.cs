// SEMANTIC ANCHOR [THEME JSON SSOT]: Preset JSON remains the authority for colors, seeds,
// effect types and effect parameters. Import is data-only and cannot instantiate code.
// Schema version 1 stays compatible with the supplied GlueDock presets; new renderers are
// explicitly whitelisted, not dynamically loaded from JSON.
using System.Text.Json;

namespace PortableThemes.Core;

public enum ThemeEffectKind { Smoke, Aurora, Ember, DeepCurrent, GlowDot, Flame, Gradient }

public sealed class ThemeEffect
{
    internal ThemeEffect(ThemeEffectKind kind, int seed, int count,
        IReadOnlyDictionary<string, JsonElement> parameters)
    {
        Kind = kind;
        Seed = seed;
        Count = count;
        Parameters = parameters;
    }

    public ThemeEffectKind Kind { get; }
    public int Seed { get; }
    public int Count { get; }
    public IReadOnlyDictionary<string, JsonElement> Parameters { get; }

    public double Number(string name, double defaultValue, double minimum, double maximum)
    {
        return Parameters.TryGetValue(name, out var element) && element.ValueKind == JsonValueKind.Number &&
               element.TryGetDouble(out double value) && double.IsFinite(value)
            ? Math.Clamp(value, minimum, maximum) : defaultValue;
    }

    public string Text(string name, string defaultValue = "") =>
        Parameters.TryGetValue(name, out var element) && element.ValueKind == JsonValueKind.String
            ? element.GetString() ?? defaultValue : defaultValue;
}

public sealed class ThemeDefinition
{
    private ThemeDefinition(string id, string name, ThemeColor background, ThemeColor surface,
        ThemeColor border, ThemeColor text, ThemeColor accent, ThemeColor hover,
        ThemeColor top, ThemeColor bottom, ThemeColor highlight, ThemeColor shadow,
        ThemeColor flameBase, bool glassSurface, IReadOnlyList<ThemeEffect> effects)
    {
        Id = id; Name = name; Background = background; Surface = surface;
        Border = border; Text = text; Accent = accent; Hover = hover;
        GlassTop = top; GlassBottom = bottom; Highlight = highlight;
        Shadow = shadow; FlameBase = flameBase; GlassSurfaceEnabled = glassSurface;
        Effects = effects;
    }

    public string Id { get; }
    public string Name { get; }
    public ThemeColor Background { get; }
    public ThemeColor Surface { get; }
    public ThemeColor Border { get; }
    public ThemeColor Text { get; }
    public ThemeColor Accent { get; }
    public ThemeColor Hover { get; }
    public ThemeColor GlassTop { get; }
    public ThemeColor GlassBottom { get; }
    public ThemeColor Highlight { get; }
    public ThemeColor Shadow { get; }
    public ThemeColor FlameBase { get; }
    public bool GlassSurfaceEnabled { get; }
    public IReadOnlyList<ThemeEffect> Effects { get; }
    public bool IsDefault => string.Equals(Id, "Default", StringComparison.OrdinalIgnoreCase);
    public bool HasAnimation => Effects.Count != 0;

    public static ThemeDefinition Parse(ReadOnlyMemory<byte> json, string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        if (json.Length is < 2 or > 512 * 1024)
            throw new InvalidDataException("Theme JSON must be 2 to 524288 bytes.");
        using JsonDocument doc = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 48 });
        var root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("Theme root must be an object.");
        string name = RequiredString(root, "Name", 100);
        int schema = Integer(root, "GlassEffectSchemaVersion", 1, 1, 1);
        _ = schema;
        // Validate known GlueDock fields even when this renderer does not consume them.
        // Future adapters must never interpret an unvalidated color or excessive layer count.
        foreach (string field in new[]
        {
            "DockBackgroundColor", "ItemBackgroundColor", "ItemBorderColor", "TextColor",
            "SubmenuIndicatorColor", "DragGhostBackgroundColor", "DragGhostBorderColor",
            "GlassTopColor", "GlassBottomColor", "GlassHighlightColor", "GlassShadowColor",
            "GlassFlameBaseGlowColor", "GlassGradientStartColor", "GlassGradientEndColor"
        })
            if (root.TryGetProperty(field, out var color) &&
                (color.ValueKind != JsonValueKind.String || !ThemeColor.TryParse(color.GetString(), out _)))
                throw new InvalidDataException($"Invalid color: {field}.");
        foreach (string field in new[] { "GlassFlameLayers", "GlassStarLayers", "GlassGradientAnimationKeyFrames" })
            if (root.TryGetProperty(field, out var layers) &&
                (layers.ValueKind != JsonValueKind.Array || layers.GetArrayLength() > 1024))
                throw new InvalidDataException($"Invalid or excessive {field}.");
        if (root.TryGetProperty("GlassGradientAnimationDurationSeconds", out var duration) &&
            (duration.ValueKind != JsonValueKind.Number || !duration.TryGetDouble(out double d) ||
             !double.IsFinite(d) || d < 0.1 || d > 120))
            throw new InvalidDataException("Invalid gradient animation duration.");
        _ = Integer(root, "GlassStarGeneratedCount", 0, 0, 1024);
        ThemeColor Pick(string field, string fallback) =>
            root.TryGetProperty(field, out var value)
                ? value.ValueKind == JsonValueKind.String && ThemeColor.TryParse(value.GetString(), out var result)
                    ? result : throw new InvalidDataException($"Invalid color: {field}.")
                : ThemeColor.Parse(fallback);

        var effects = new List<ThemeEffect>();
        if (root.TryGetProperty("GlassEffects", out var collection))
        {
            if (collection.ValueKind != JsonValueKind.Array || collection.GetArrayLength() > 32)
                throw new InvalidDataException("GlassEffects must be an array with up to 32 effects.");
            foreach (var item in collection.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                    throw new InvalidDataException("Invalid effect definition.");
                if (!Boolean(item, "Enabled", true)) continue;
                string renderer = RequiredString(item, "Renderer", 48);
                string type = RequiredString(item, "Type", 48);
                ThemeEffectKind kind = renderer.ToLowerInvariant() switch
                {
                    "smoke" when type.Equals("Background", StringComparison.OrdinalIgnoreCase)
                        => ThemeEffectKind.Smoke,
                    "aurora" when type.Equals("Background", StringComparison.OrdinalIgnoreCase)
                        => ThemeEffectKind.Aurora,
                    "ember" when type.Equals("Background", StringComparison.OrdinalIgnoreCase)
                        => ThemeEffectKind.Ember,
                    "deepcurrent" when type.Equals("Background", StringComparison.OrdinalIgnoreCase)
                        => ThemeEffectKind.DeepCurrent,
                    "glowdot" when type.Equals("Particles", StringComparison.OrdinalIgnoreCase)
                        => ThemeEffectKind.GlowDot,
                    "flame" when type.Equals("Particles", StringComparison.OrdinalIgnoreCase)
                        => ThemeEffectKind.Flame,
                    "gradient" when type.Equals("Background", StringComparison.OrdinalIgnoreCase)
                        => ThemeEffectKind.Gradient,
                    _ => throw new InvalidDataException($"Unsupported effect: {type}/{renderer}.")
                };
                int seed = Integer(item, "Seed", 1, int.MinValue, int.MaxValue);
                int count = Integer(item, "Count", 0, 0, 1024);
                var args = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
                if (item.TryGetProperty("Parameters", out var props))
                {
                    if (props.ValueKind != JsonValueKind.Object ||
                        props.EnumerateObject().Count() > 64)
                        throw new InvalidDataException("Invalid or excessive effect parameters.");
                    foreach (var prop in props.EnumerateObject())
                    {
                        if (prop.Name.Length > 64)
                            throw new InvalidDataException("Invalid effect parameter name.");
                        ValidateParameter(prop.Value);
                        args.Add(prop.Name, prop.Value.Clone());
                    }
                }
                effects.Add(new ThemeEffect(kind, seed, count, args));
            }
        }
        // Legacy booleans from GlueDock remain valid. Add a fallback only when no renderer
        // for the corresponding effect is already active (avoids double-drawn lava blobs).
        if (Boolean(root, "GlassGradientAnimationEnabled", false) &&
            !effects.Any(e => e.Kind == ThemeEffectKind.Gradient))
            effects.Add(new ThemeEffect(ThemeEffectKind.Gradient, 1, 0,
                new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase)));
        if (Boolean(root, "GlassStarEffectEnabled", false) &&
            !effects.Any(e => e.Kind == ThemeEffectKind.GlowDot))
            effects.Add(new ThemeEffect(ThemeEffectKind.GlowDot,
                Integer(root, "GlassStarGeneratedSeed", 341, int.MinValue, int.MaxValue),
                Integer(root, "GlassStarGeneratedCount", 60, 0, 1024),
                new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase)));
        if (Boolean(root, "GlassFlameEffectEnabled", false) &&
            !effects.Any(e => e.Kind == ThemeEffectKind.Flame))
            effects.Add(new ThemeEffect(ThemeEffectKind.Flame, 1, 0,
                new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase)));
        return new ThemeDefinition(id, name,
            Pick("DockBackgroundColor", "#12161C"),
            Pick("ItemBackgroundColor", "#22FFFFFF"),
            Pick("ItemBorderColor", "#22FFFFFF"),
            Pick("TextColor", "#FFFFFFFF"),
            Pick("DragGhostBorderColor", "#FF6CB7E6"),
            Pick("DragGhostBackgroundColor", "#405D8FA5"),
            Pick("GlassTopColor", "#00000000"),
            Pick("GlassBottomColor", "#00000000"),
            Pick("GlassHighlightColor", "#4094D4FF"),
            Pick("GlassShadowColor", "#80000000"),
            Pick("GlassFlameBaseGlowColor", "#80FF5A00"),
            Boolean(root, "GlassSurfaceEnabled", false), effects.AsReadOnly());
    }

    private static string RequiredString(JsonElement obj, string property, int length)
    {
        if (!obj.TryGetProperty(property, out var v) || v.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(v.GetString()) || v.GetString()!.Length > length)
            throw new InvalidDataException($"Missing or invalid string: {property}.");
        return v.GetString()!.Trim();
    }

    private static bool Boolean(JsonElement obj, string property, bool fallback)
    {
        if (!obj.TryGetProperty(property, out var v)) return fallback;
        return v.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => throw new InvalidDataException($"Invalid boolean: {property}.")
        };
    }

    private static int Integer(JsonElement obj, string property, int fallback, int min, int max)
    {
        if (!obj.TryGetProperty(property, out var v)) return fallback;
        if (v.ValueKind != JsonValueKind.Number || !v.TryGetInt32(out int result) ||
            result < min || result > max)
            throw new InvalidDataException($"Out of range integer: {property}.");
        return result;
    }

    private static void ValidateParameter(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Number:
                if (!element.TryGetDouble(out double value) || !double.IsFinite(value) ||
                    Math.Abs(value) > 1_000_000)
                    throw new InvalidDataException("Invalid numeric effect parameter.");
                break;
            case JsonValueKind.String:
                if (element.GetString()!.Length > 128)
                    throw new InvalidDataException("Effect string too long.");
                break;
            case JsonValueKind.True:
            case JsonValueKind.False:
                break;
            case JsonValueKind.Array:
                if (element.GetArrayLength() > 1024)
                    throw new InvalidDataException("Effect array too large.");
                foreach (var child in element.EnumerateArray()) ValidateParameter(child);
                break;
            case JsonValueKind.Object:
                if (element.EnumerateObject().Count() > 16)
                    throw new InvalidDataException("Effect parameter object too large.");
                foreach (var child in element.EnumerateObject()) ValidateParameter(child.Value);
                break;
            default:
                throw new InvalidDataException("Unsupported effect parameter value.");
        }
    }
}
