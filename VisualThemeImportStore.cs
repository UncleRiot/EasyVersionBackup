// SEMANTIC ANCHOR - IMPORTED THEME CATALOG / SSOT:
// Immutable JSON files are the sole imported-preset source; built-in presets remain untouched.
// Treat theme files as untrusted data: no XAML, code execution, path traversal or network access.
// Preview/selection is not settings persistence. A failed import leaves the catalog unchanged.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace EasyVersionBackup.VisualThemes;

public sealed record ImportedTheme(string Id, string DisplayName, string Path);

public sealed class ThemeImportStore
{
    private const int MaxFileBytes = 512 * 1024;
    private readonly string _directory;
    private static readonly HashSet<string> ColorFields = new(StringComparer.Ordinal)
    {
        "DockBackgroundColor", "ItemBackgroundColor", "ItemBorderColor", "TextColor",
        "SubmenuIndicatorColor", "DragGhostBackgroundColor", "DragGhostBorderColor",
        "GlassTopColor", "GlassBottomColor", "GlassHighlightColor", "GlassShadowColor",
        "GlassFlameBaseGlowColor"
    };

    public ThemeImportStore(string appId)
    {
        if (!IsSafeId(appId))
            throw new ArgumentException("The application ID must use 1-64 ASCII letters, digits, '-' or '_'.", nameof(appId));
        _directory = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            appId, "Themes");
    }

    public string DirectoryPath => _directory;

    // No destructive overwrite: a new import gets a stable, unique ID. A built-in ID is reserved.
    // Atomic rename occurs only after the entire JSON document has passed validation.
    public ImportedTheme ImportFile(string sourcePath, IEnumerable<string> reservedIds,
        Action<JsonElement>? platformValidate = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentNullException.ThrowIfNull(reservedIds);
        if (!string.Equals(System.IO.Path.GetExtension(sourcePath), ".json", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Only .json theme files are supported.");

        var info = new FileInfo(sourcePath);
        if (!info.Exists || info.Length is <= 0 or > MaxFileBytes)
            throw new InvalidDataException("Theme file is missing, empty or too large (maximum 512 KiB).");
        byte[] bytes = File.ReadAllBytes(sourcePath);
        if (bytes.Length is <= 0 or > MaxFileBytes)
            throw new InvalidDataException("Invalid theme file length.");

        string displayName = Validate(bytes, platformValidate);
        System.IO.Directory.CreateDirectory(_directory);
        var used = new HashSet<string>(reservedIds, StringComparer.OrdinalIgnoreCase) { "Default", "Dark" };
        foreach (var item in GetInstalledThemes()) used.Add(item.Id);
        string baseId = MakeSafeId(System.IO.Path.GetFileNameWithoutExtension(sourcePath));
        string id = baseId;
        for (var index = 2; used.Contains(id) || File.Exists(GetPathForId(id)); index++)
            id = baseId[..Math.Min(baseId.Length, 54)] + "-" + index;

        string destination = GetPathForId(id);
        string staging = System.IO.Path.Combine(_directory, "." + Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            File.WriteAllBytes(staging, bytes);
            File.Move(staging, destination); // fails rather than replacing an existing preset
            return new ImportedTheme(id, displayName, destination);
        }
        finally
        {
            if (File.Exists(staging)) File.Delete(staging);
        }
    }

    public IReadOnlyList<ImportedTheme> GetInstalledThemes()
    {
        if (!System.IO.Directory.Exists(_directory)) return Array.Empty<ImportedTheme>();
        var result = new List<ImportedTheme>();
        foreach (string path in System.IO.Directory.EnumerateFiles(_directory, "*.json", SearchOption.TopDirectoryOnly))
        {
            string id = System.IO.Path.GetFileNameWithoutExtension(path);
            if (!IsSafeId(id)) continue;
            try
            {
                var info = new FileInfo(path);
                if (info.Length is <= 0 or > MaxFileBytes) continue;
                string name = Validate(File.ReadAllBytes(path));
                result.Add(new ImportedTheme(id, name, path));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException
                                             or InvalidDataException or ArgumentException)
            {
                // Invalid user-edited files are not selectable; the application default remains intact.
            }
        }
        return result.OrderBy(item => item.DisplayName, StringComparer.CurrentCultureIgnoreCase).ToArray();
    }

    public string? FindInstalledFile(string? id)
    {
        if (id is null || !IsSafeId(id)) return null;
        // Keep IDs case-insensitive on both Windows and portable hosts.
        var item = GetInstalledThemes().FirstOrDefault(item =>
            string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase));
        return item?.Path;
    }

    private string GetPathForId(string id)
    {
        if (!IsSafeId(id)) throw new ArgumentException("Invalid theme ID.", nameof(id));
        return System.IO.Path.Combine(_directory, id + ".json");
    }

    private static bool IsSafeId(string id) =>
        id.Length is >= 1 and <= 64 && id.All(c =>
            (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
            (c >= '0' && c <= '9') || c is '-' or '_');

    private static string MakeSafeId(string value)
    {
        var sb = new StringBuilder(value.Length);
        foreach (char c in value)
        {
            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
                (c >= '0' && c <= '9') || c is '-' or '_') sb.Append(c);
            else if (sb.Length > 0 && sb[^1] != '-') sb.Append('-');
            if (sb.Length >= 54) break;
        }
        string result = sb.ToString().Trim('-');
        return result.Length == 0 ? "ImportedTheme" : result;
    }

    private static string Validate(byte[] bytes, Action<JsonElement>? platformValidate = null)
    {
        using var document = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 48 });
        JsonElement root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("A theme must be a JSON object.");
        if (!root.TryGetProperty("Name", out var name) || name.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(name.GetString()) || name.GetString()!.Length > 100)
            throw new InvalidDataException("A theme requires a non-empty Name (maximum 100 characters).");

        foreach (JsonProperty field in root.EnumerateObject())
        {
            if (ColorFields.Contains(field.Name))
            {
                if (field.Value.ValueKind != JsonValueKind.String || !IsHexColor(field.Value.GetString()))
                    throw new InvalidDataException($"Invalid color: {field.Name}.");
            }
        }
        if (root.TryGetProperty("GlassEffectSchemaVersion", out var schema) &&
            (schema.ValueKind != JsonValueKind.Number || !schema.TryGetInt32(out var version) || version != 1))
            throw new InvalidDataException("Unsupported effect schema; this toolkit supports version 1.");
        if (root.TryGetProperty("GlassGradientAnimationDurationSeconds", out var duration) &&
            (duration.ValueKind != JsonValueKind.Number || !duration.TryGetDouble(out double d) ||
             !double.IsFinite(d) || d < 0.1 || d > 120))
            throw new InvalidDataException("Gradient duration must be between 0.1 and 120 seconds.");
        if (root.TryGetProperty("GlassStarGeneratedCount", out var generated) &&
            (generated.ValueKind != JsonValueKind.Number || !generated.TryGetInt32(out var count) ||
             count is < 0 or > 1024))
            throw new InvalidDataException("Too many generated stars.");
        foreach (string arrayName in new[] { "GlassFlameLayers", "GlassStarLayers", "GlassGradientAnimationKeyFrames" })
            if (root.TryGetProperty(arrayName, out var layers) &&
                (layers.ValueKind != JsonValueKind.Array || layers.GetArrayLength() > 1024))
                throw new InvalidDataException($"Invalid or excessive {arrayName}.");
        if (root.TryGetProperty("GlassEffects", out var effects))
        {
            if (effects.ValueKind != JsonValueKind.Array || effects.GetArrayLength() > 32)
                throw new InvalidDataException("Invalid effect list.");
            foreach (var effect in effects.EnumerateArray())
            {
                if (effect.ValueKind != JsonValueKind.Object)
                    throw new InvalidDataException("Invalid effect definition.");
                if (effect.TryGetProperty("Count", out var amount) &&
                    (amount.ValueKind != JsonValueKind.Number || !amount.TryGetInt32(out var n) || n is < 0 or > 1024))
                    throw new InvalidDataException("Effect count exceeds the supported limit (1024).");
                if (effect.TryGetProperty("Enabled", out var enabled) && enabled.ValueKind == JsonValueKind.False)
                    continue;
                // Semantic Anchor [Effect allowlist / import SSOT]: Renderer names are strictly
                // allowlisted data, never native code, file paths or executable plugins. Mirror
                // Core's Background effect names; preserve existing legacy particle imports.
                if (!effect.TryGetProperty("Type", out var type) || type.ValueKind != JsonValueKind.String ||
                    !effect.TryGetProperty("Renderer", out var renderer) || renderer.ValueKind != JsonValueKind.String)
                    throw new InvalidDataException("Invalid effect renderer.");
                bool particle = string.Equals(type.GetString(), "Particles", StringComparison.OrdinalIgnoreCase) &&
                    (string.Equals(renderer.GetString(), "Flame", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(renderer.GetString(), "GlowDot", StringComparison.OrdinalIgnoreCase));
                bool backdrop = string.Equals(type.GetString(), "Background", StringComparison.OrdinalIgnoreCase) &&
                    (string.Equals(renderer.GetString(), "Smoke", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(renderer.GetString(), "Aurora", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(renderer.GetString(), "Ember", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(renderer.GetString(), "DeepCurrent", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(renderer.GetString(), "Gradient", StringComparison.OrdinalIgnoreCase));
                if (!particle && !backdrop)
                    throw new InvalidDataException("Unsupported active effect renderer.");
            }
        }
        // Semantic Anchor [Untrusted theme numeric bounds]: Imported JSON cannot request
        // non-finite or unbounded rendering coordinates, timers or particle geometry.
        ValidateNumericBounds(root);
        platformValidate?.Invoke(root);
        return name.GetString()!.Trim();
    }

    private static void ValidateNumericBounds(JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (JsonProperty property in value.EnumerateObject()) ValidateNumericBounds(property.Value);
                break;
            case JsonValueKind.Array:
                foreach (JsonElement child in value.EnumerateArray()) ValidateNumericBounds(child);
                break;
            case JsonValueKind.Number:
                if (!value.TryGetDouble(out double number) || !double.IsFinite(number) ||
                    Math.Abs(number) > 1_000_000)
                    throw new InvalidDataException("Theme contains an unsafe numeric value.");
                break;
        }
    }

    private static bool IsHexColor(string? value) =>
        value is { Length: 7 or 9 } && value[0] == '#' &&
        value.Skip(1).All(c => (c >= '0' && c <= '9') ||
                               (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'));
}
