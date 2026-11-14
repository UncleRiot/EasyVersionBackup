// SEMANTIC ANCHOR [CATALOG/IMPORT SSOT]: The embedded preset and its validated imported JSON
// are the only theme definitions. Import never changes the selected theme, overwrites an
// existing theme, or writes settings. Host applications own selection persistence.
using System.Reflection;
using System.Text;

namespace PortableThemes.Core;

public sealed record ThemeOption(string Id, string DisplayName, bool IsImported);

public sealed class ThemeCatalog
{
    private const int MaxFileBytes = 512 * 1024;
    private const string ResourcePrefix = "PortableThemes.Core.Presets.";
    private readonly string _directory;
    private readonly Assembly _assembly = typeof(ThemeCatalog).Assembly;

    public ThemeCatalog(string applicationId, string? importedThemeDirectory = null)
    {
        if (!SafeId(applicationId))
            throw new ArgumentException("Application ID must use 1-64 ASCII letters, digits, '_' or '-'.", nameof(applicationId));
        _directory = importedThemeDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            applicationId, "Themes");
    }

    public string ImportedThemeDirectory => _directory;

    public IReadOnlyList<ThemeOption> List()
    {
        var options = new List<ThemeOption>();
        foreach (string resource in _assembly.GetManifestResourceNames())
        {
            if (!resource.StartsWith(ResourcePrefix, StringComparison.Ordinal) ||
                !resource.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) continue;
            string id = resource[ResourcePrefix.Length..^5];
            try
            {
                var theme = ReadBuiltIn(id);
                if (theme is not null) options.Add(new ThemeOption(id, theme.Name, false));
            }
            catch (InvalidDataException) { /* Invalid built-in is not listed. */ }
        }
        if (Directory.Exists(_directory))
        {
            foreach (string file in Directory.EnumerateFiles(_directory, "*.json", SearchOption.TopDirectoryOnly))
            {
                string id = Path.GetFileNameWithoutExtension(file);
                if (!SafeId(id) || options.Any(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase)))
                    continue;
                try
                {
                    var theme = ReadFile(file, id);
                    options.Add(new ThemeOption(id, theme.Name, true));
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException
                                              or System.Text.Json.JsonException)
                {
                    // Invalid or user-edited imported JSON cannot become a selectable theme.
                }
            }
        }
        return options.OrderBy(x => x.Id == "Default" ? 0 : 1)
                      .ThenBy(x => x.DisplayName, StringComparer.CurrentCultureIgnoreCase).ToArray();
    }

    public ThemeDefinition Load(string? id)
    {
        string safe = id is not null && SafeId(id) ? id : "Default";
        if (safe.Equals("Fireplace2", StringComparison.OrdinalIgnoreCase) ||
            safe.Equals("Fireplace3", StringComparison.OrdinalIgnoreCase)) safe = "Campfire";
        var builtIn = ReadBuiltIn(safe);
        if (builtIn is not null) return builtIn;
        if (Directory.Exists(_directory))
        {
            foreach (string file in Directory.EnumerateFiles(_directory, "*.json", SearchOption.TopDirectoryOnly))
            {
                if (!Path.GetFileNameWithoutExtension(file).Equals(safe, StringComparison.OrdinalIgnoreCase))
                    continue;
                try { return ReadFile(file, safe); }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException
                                              or System.Text.Json.JsonException) { break; }
            }
        }
        return ReadBuiltIn("Default") ?? throw new InvalidDataException("Missing built-in Default preset.");
    }

    public ThemeOption Import(string jsonPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonPath);
        if (!Path.GetExtension(jsonPath).Equals(".json", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Only .json themes can be imported.");
        string displayName;
        byte[] data = ReadBytes(jsonPath);
        // Validation must finish *before* creating or changing any imported file.
        displayName = ThemeDefinition.Parse(data, "ValidatedImport").Name;
        Directory.CreateDirectory(_directory);
        var used = new HashSet<string>(List().Select(x => x.Id), StringComparer.OrdinalIgnoreCase);
        string original = SafeSlug(Path.GetFileNameWithoutExtension(jsonPath));
        string id = original;
        for (int n = 2; used.Contains(id) || File.Exists(Path.Combine(_directory, id + ".json")); n++)
            id = original[..Math.Min(original.Length, 50)] + "-" + n;
        string destination = Path.Combine(_directory, id + ".json");
        string temporary = Path.Combine(_directory, "." + Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            File.WriteAllBytes(temporary, data);
            File.Move(temporary, destination); // Never overwrite an existing file.
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
        return new ThemeOption(id, displayName, true);
    }

    private ThemeDefinition? ReadBuiltIn(string id)
    {
        if (!SafeId(id)) return null;
        string? resource = _assembly.GetManifestResourceNames().FirstOrDefault(x =>
            x.Equals(ResourcePrefix + id + ".json", StringComparison.OrdinalIgnoreCase));
        if (resource is null) return null;
        using Stream stream = _assembly.GetManifestResourceStream(resource)
            ?? throw new InvalidDataException($"Missing preset resource: {resource}.");
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        if (memory.Length > MaxFileBytes) throw new InvalidDataException("Built-in preset too large.");
        return ThemeDefinition.Parse(memory.ToArray(), id);
    }

    private static ThemeDefinition ReadFile(string file, string id) => ThemeDefinition.Parse(ReadBytes(file), id);

    private static byte[] ReadBytes(string path)
    {
        var info = new FileInfo(path);
        if (!info.Exists || info.Length is <= 0 or > MaxFileBytes)
            throw new InvalidDataException("Theme is missing, empty or exceeds 512 KiB.");
        byte[] bytes = File.ReadAllBytes(path);
        if (bytes.Length is <= 0 or > MaxFileBytes)
            throw new InvalidDataException("Theme size changed during reading.");
        return bytes;
    }

    private static bool SafeId(string id) => id.Length is >= 1 and <= 64 && id.All(c =>
        c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '-' or '_');

    private static string SafeSlug(string name)
    {
        var text = new StringBuilder();
        foreach (char c in name)
        {
            if (char.IsAsciiLetterOrDigit(c) || c is '_' or '-') text.Append(c);
            else if (text.Length > 0 && text[^1] != '-') text.Append('-');
            if (text.Length >= 50) break;
        }
        string id = text.ToString().Trim('-');
        return id.Length == 0 ? "ImportedTheme" : id;
    }
}
