// Semantic Anchor [Visual theme source reference]: Original GlueDock theme model and generated effect calculations; JSON presets are the SSOT.
using System.IO;
using System.Text.Json;
using EasyVersionBackup.VisualThemes;

namespace EasyVersionBackup.VisualThemes;

public static class DockThemeService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    // SEMANTIC ANCHOR - THEME RESOLUTION / SSOT:
    // Built-in JSON and user-imported JSON are distinct stores. Imported IDs never
    // overwrite built-ins; invalid/missing imports fall back to Default unchanged.
    private static readonly ThemeImportStore Imported = new(
        typeof(DockThemeService).Assembly.GetName().Name ?? "ThemeKitWpf");

    // Built-in presets are assembly resources so framework-dependent single-file publishing
    // needs no new writable application folder or external JSON deployment.
    private const string ResourcePrefix = "EasyVersionBackup.Themes.";

    // Semantic Anchor [Built-in theme catalog visibility]: Deprecated or superseded built-in
    // variants stay embedded for backward compatibility only. They are hidden from selection,
    // and persisted legacy IDs are normalized to the maintained replacement preset instead of
    // silently changing to Default.
    private static readonly HashSet<string> HiddenBuiltInIds = new(StringComparer.OrdinalIgnoreCase)
    {
        "Fireplace2",
        "Fireplace3",
        "CyanAbyssSmoke"
    };

    private static readonly Dictionary<string, string> BuiltInAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Fireplace2"] = "Campfire",
        ["Fireplace3"] = "Campfire",
        ["CyanAbyssSmoke"] = "Cyan_Abyss_Animated"
    };

    public static DockThemeOption ImportTheme(string filePath)
    {
        var imported = Imported.ImportFile(filePath, GetBuiltInIds(), root =>
        {
            var validated = JsonSerializer.Deserialize<DockTheme>(root.GetRawText(), JsonOptions)
                ?? throw new InvalidDataException("Theme object is empty.");
            if (validated.GlassEffects is null || validated.GlassStarLayers is null ||
                validated.GlassFlameLayers is null || validated.GlassGradientAnimationKeyFrames is null)
                throw new InvalidDataException("Invalid theme layers.");
            if (EasyVersionBackup.VisualThemeRuntime.NeedsPortableRenderer(validated))
                _ = PortableThemes.Core.ThemeDefinition.Parse(
                    System.Text.Encoding.UTF8.GetBytes(root.GetRawText()), "ImportedEffect");
        });
        return new DockThemeOption(imported.Id, imported.DisplayName);
    }

    private static IEnumerable<string> GetBuiltInIds() =>
        typeof(DockThemeService).Assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(ResourcePrefix, StringComparison.Ordinal) &&
                           name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .Select(name => name[ResourcePrefix.Length..^5])
            .Where(id => !HiddenBuiltInIds.Contains(id));

    private static string NormalizeBuiltInId(string id) =>
        BuiltInAliases.TryGetValue(id, out string? alias) ? alias : id;

    private static DockTheme? LoadBuiltIn(string id)
    {
        string normalizedId = NormalizeBuiltInId(id);
        if (!GetBuiltInIds().Contains(normalizedId, StringComparer.OrdinalIgnoreCase)) return null;
        using Stream? stream = typeof(DockThemeService).Assembly.GetManifestResourceStream(
            ResourcePrefix + normalizedId + ".json");
        if (stream is null) return null;
        try
        {
            using StreamReader reader = new(stream);
            return ReadTheme(reader.ReadToEnd());
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidDataException)
        {
            return null;
        }
    }

    public static IReadOnlyList<DockThemeOption> GetAvailableThemes()
    {
        List<DockThemeOption> themes = [];

        foreach (string id in GetBuiltInIds())
        {
            DockTheme? theme = LoadBuiltIn(id);
            if (theme != null)
                themes.Add(new DockThemeOption(id, string.IsNullOrWhiteSpace(theme.Name) ? id : theme.Name.Trim()));
        }

        // User themes are listed alongside built-ins, never replacing them.
        var builtInIds = new HashSet<string>(themes.Select(theme => theme.Id),
            StringComparer.OrdinalIgnoreCase);
        foreach (var imported in Imported.GetInstalledThemes())
            if (!builtInIds.Contains(imported.Id) && LoadFile(imported.Path) is not null)
                themes.Add(new DockThemeOption(imported.Id, imported.DisplayName));

        return themes
            .OrderBy(
                theme =>
                    string.Equals(
                        theme.Id,
                        "Default",
                        StringComparison.OrdinalIgnoreCase)
                        ? 0
                        : 1)
            .ThenBy(
                theme => theme.DisplayName,
                StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    public static DockTheme Load(
        string? themeName)
    {
        string normalizedName =
            string.IsNullOrWhiteSpace(
                themeName)
                ? "Default"
                : Path.GetFileNameWithoutExtension(
                    themeName.Trim());

        normalizedName =
            NormalizeBuiltInId(
                normalizedName);

        // Resolve built-ins first, then validated user imports; never permit path fragments.
        DockTheme? selectedTheme =
            GetBuiltInIds().Contains(normalizedName, StringComparer.OrdinalIgnoreCase)
                ? LoadBuiltIn(normalizedName)
                : LoadFile(Imported.FindInstalledFile(normalizedName) ?? string.Empty);

        if (selectedTheme is not null)
        {
            return selectedTheme;
        }

        DockTheme? defaultTheme =
            LoadBuiltIn("Default");

        return defaultTheme ??
               new DockTheme();
    }

    private static void PopulateGeneratedThemeLayers(
        DockTheme? theme)
    {
        if (theme is null)
        {
            return;
        }

        foreach (DockThemeEffectDefinition effect
            in theme.GlassEffects)
        {
            if (!effect.Enabled)
            {
                continue;
            }

            ApplyGeneratedThemeEffect(
                theme,
                effect);
        }

        if (theme.GlassStarEffectEnabled &&
            theme.GlassStarLayers.Count == 0 &&
            theme.GlassStarGeneratedCount > 0)
        {
            theme.GlassStarLayers =
                GenerateStarLayers(
                    theme);
        }
    }

    private static void ApplyGeneratedThemeEffect(
        DockTheme theme,
        DockThemeEffectDefinition effect)
    {
        if (!string.Equals(
                effect.Type,
                "Particles",
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (string.Equals(
                effect.Renderer,
                "GlowDot",
                StringComparison.OrdinalIgnoreCase))
        {
            if (theme.GlassStarLayers.Count == 0)
            {
                theme.GlassStarEffectEnabled =
                    true;

                theme.GlassStarLayers =
                    GenerateGlowDotLayers(
                        effect);
            }

            return;
        }

        if (string.Equals(
                effect.Renderer,
                "Flame",
                StringComparison.OrdinalIgnoreCase))
        {
            if (theme.GlassFlameLayers.Count == 0)
            {
                theme.GlassFlameEffectEnabled =
                    true;

                theme.GlassFlameLayers =
                    GenerateFlameLayers(
                        effect);
            }
        }
    }

    private static List<DockThemeStarLayer> GenerateGlowDotLayers(
        DockThemeEffectDefinition effect)
    {
        int count =
            Math.Clamp(
                effect.Count,
                1,
                256);

        Random random =
            new(
                NormalizeSeed(
                    effect.Seed));

        double paddingX =
            GetEffectDouble(
                effect,
                "PaddingXRatio",
                0.04,
                0,
                0.45);

        double topPadding =
            GetEffectDouble(
                effect,
                "TopPaddingRatio",
                0.08,
                0,
                0.80);

        double bottomPadding =
            GetEffectDouble(
                effect,
                "BottomPaddingRatio",
                0.18,
                0,
                0.80);

        if (topPadding + bottomPadding > 0.92)
        {
            bottomPadding =
                Math.Max(
                    0,
                    0.92 - topPadding);
        }

        (double minimumSize, double maximumSize) =
            GetEffectRange(
                effect,
                "Size",
                1.4,
                5.8,
                0.5,
                24);

        (double minimumOpacity, double maximumOpacity) =
            GetEffectRange(
                effect,
                "Opacity",
                0.24,
                0.98,
                0.02,
                1);

        (double minimumDuration, double maximumDuration) =
            GetEffectRange(
                effect,
                "DurationSeconds",
                2.4,
                6.8,
                0.2,
                120);

        double maximumDelay =
            GetEffectDouble(
                effect,
                "MaximumDelaySeconds",
                2.8,
                0,
                120);

        (double minimumGlowRadius, double maximumGlowRadius) =
            GetEffectRange(
                effect,
                "GlowRadius",
                1.2,
                4.6,
                0,
                32);

        (double minimumScale, double maximumScale) =
            GetEffectRange(
                effect,
                "Scale",
                0.72,
                1.28,
                0.1,
                4);

        double maximumDriftXRatio =
            GetEffectDouble(
                effect,
                "MaximumDriftXRatio",
                0.0035,
                0,
                0.10);

        double maximumDriftYRatio =
            GetEffectDouble(
                effect,
                "MaximumDriftYRatio",
                0.0025,
                0,
                0.10);

        double highlightChance =
            GetEffectDouble(
                effect,
                "HighlightChance",
                0.16,
                0,
                1);

        double highlightScale =
            GetEffectDouble(
                effect,
                "HighlightScale",
                1.28,
                1,
                4);

        List<string> palette =
            GetEffectStringList(
                effect,
                "Palette");

        if (palette.Count == 0)
        {
            palette.Add(
                "#FFF8FEFF");
            palette.Add(
                "#FFDDEBFF");
            palette.Add(
                "#FFB7D9FF");
            palette.Add(
                "#FFF8F6D8");
        }

        List<DockThemeStarLayer> stars =
            new(
                count);

        for (int index = 0;
             index < count;
             index++)
        {
            double x =
                Lerp(
                    paddingX,
                    1 - paddingX,
                    random.NextDouble());

            double y =
                Lerp(
                    topPadding,
                    1 - bottomPadding,
                    random.NextDouble());

            double sizeBias =
                random.NextDouble();

            sizeBias *=
                sizeBias;

            double size =
                Lerp(
                    minimumSize,
                    maximumSize,
                    sizeBias);

            if (random.NextDouble() <
                highlightChance)
            {
                size =
                    Math.Min(
                        maximumSize,
                        size * highlightScale);
            }

            double minimumStarOpacity =
                Lerp(
                    minimumOpacity,
                    maximumOpacity,
                    random.NextDouble() *
                    0.55);

            double maximumStarOpacity =
                Lerp(
                    Math.Max(
                        minimumStarOpacity,
                        minimumOpacity + 0.08),
                    maximumOpacity,
                    random.NextDouble());

            if (maximumStarOpacity <
                minimumStarOpacity)
            {
                maximumStarOpacity =
                    minimumStarOpacity;
            }

            stars.Add(
                new DockThemeStarLayer
                {
                    X = x,
                    Y = y,
                    Size = size,
                    Color =
                        palette[
                            random.Next(
                                palette.Count)],
                    MinimumOpacity =
                        minimumStarOpacity,
                    MaximumOpacity =
                        maximumStarOpacity,
                    DurationSeconds =
                        Lerp(
                            minimumDuration,
                            maximumDuration,
                            random.NextDouble()),
                    DelaySeconds =
                        maximumDelay *
                        random.NextDouble(),
                    GlowRadius =
                        Lerp(
                            minimumGlowRadius,
                            maximumGlowRadius,
                            random.NextDouble()),
                    MinimumScale =
                        minimumScale,
                    MaximumScale =
                        maximumScale,
                    DriftXRatio =
                        Lerp(
                            -maximumDriftXRatio,
                            maximumDriftXRatio,
                            random.NextDouble()),
                    DriftYRatio =
                        Lerp(
                            -maximumDriftYRatio,
                            maximumDriftYRatio,
                            random.NextDouble())
                });
        }

        return stars;
    }

    private static List<DockThemeFlameLayer> GenerateFlameLayers(
        DockThemeEffectDefinition effect)
    {
        int count =
            Math.Clamp(
                effect.Count,
                1,
                256);

        if (HasCompleteIndexedFlameLayers(
                effect,
                count))
        {
            return GenerateIndexedFlameLayers(
                effect,
                count);
        }

        Random random =
            new(
                NormalizeSeed(
                    effect.Seed));

        double paddingX =
            GetEffectDouble(
                effect,
                "PaddingXRatio",
                0.012,
                0,
                0.45);

        (double minimumWidth, double maximumWidth) =
            GetEffectRange(
                effect,
                "WidthRatio",
                0.026,
                0.046,
                0.002,
                0.50);

        (double minimumHeight, double maximumHeight) =
            GetEffectRange(
                effect,
                "HeightRatio",
                0.34,
                0.86,
                0.01,
                4);

        (double minimumRise, double maximumRise) =
            GetEffectRange(
                effect,
                "RiseRatio",
                0.20,
                0.52,
                0,
                4);

        (double minimumDrift, double maximumDrift) =
            GetEffectRange(
                effect,
                "DriftRatio",
                0.001,
                0.003,
                0,
                0.20);

        (double minimumDuration, double maximumDuration) =
            GetEffectRange(
                effect,
                "DurationSeconds",
                2.2,
                3.8,
                0.2,
                120);

        double maximumDelay =
            GetEffectDouble(
                effect,
                "MaximumDelaySeconds",
                2.8,
                0,
                120);

        (double minimumOpacity, double maximumOpacity) =
            GetEffectRange(
                effect,
                "Opacity",
                0.36,
                0.64,
                0,
                1);

        (double minimumScaleX, double maximumScaleX) =
            GetEffectRange(
                effect,
                "ScaleX",
                0.58,
                1.08,
                0.1,
                4);

        (double minimumScaleY, double maximumScaleY) =
            GetEffectRange(
                effect,
                "ScaleY",
                0.66,
                1.14,
                0.1,
                4);

        (double minimumSway, double maximumSway) =
            GetEffectRange(
                effect,
                "SwayDegrees",
                1.4,
                3.4,
                0,
                45);

        (double minimumFlicker, double maximumFlicker) =
            GetEffectRange(
                effect,
                "FlickerRatio",
                0.05,
                0.13,
                0,
                1);

        (double minimumBlur, double maximumBlur) =
            GetEffectRange(
                effect,
                "BlurRadius",
                0.18,
                0.50,
                0,
                32);

        double baseYRatio =
            GetEffectDouble(
                effect,
                "BaseYRatio",
                1,
                -4,
                4);

        double tipStop =
            GetEffectDouble(
                effect,
                "TipStop",
                0,
                0,
                1);

        double midStop =
            GetEffectDouble(
                effect,
                "MidStop",
                0.43,
                0,
                1);

        double coreStop =
            GetEffectDouble(
                effect,
                "CoreStop",
                1,
                0,
                1);

        string motionMode =
            GetEffectString(
                effect,
                "MotionMode",
                "NaturalFire");

        double rarePeakChance =
            GetEffectDouble(
                effect,
                "RarePeakChance",
                0,
                0,
                1);

        double rarePeakMultiplier =
            GetEffectDouble(
                effect,
                "RarePeakMultiplier",
                1,
                1,
                4);

        List<string> tipPalette =
            GetEffectStringList(
                effect,
                "TipPalette");

        List<string> midPalette =
            GetEffectStringList(
                effect,
                "MidPalette");

        List<string> corePalette =
            GetEffectStringList(
                effect,
                "CorePalette");

        EnsurePalette(
            tipPalette,
            "#00B91000");

        EnsurePalette(
            midPalette,
            "#C8E32200");

        EnsurePalette(
            corePalette,
            "#E8FF580B");

        List<DockThemeFlameLayer> flames =
            new(
                count);

        double availableWidth =
            Math.Max(
                0,
                1 - (paddingX * 2));

        for (int index = 0;
             index < count;
             index++)
        {
            double position =
                count == 1
                    ? 0.5
                    : index /
                      (double)(count - 1);

            double x =
                paddingX +
                (availableWidth * position);

            double height =
                Lerp(
                    minimumHeight,
                    maximumHeight,
                    random.NextDouble());

            double rise =
                Lerp(
                    minimumRise,
                    maximumRise,
                    random.NextDouble());

            if (random.NextDouble() <
                rarePeakChance)
            {
                height *=
                    rarePeakMultiplier;

                rise *=
                    Math.Sqrt(
                        rarePeakMultiplier);
            }

            double scaleXLow =
                Math.Min(
                    minimumScaleX,
                    maximumScaleX);

            double scaleXHigh =
                Math.Max(
                    minimumScaleX,
                    maximumScaleX);

            double scaleYLow =
                Math.Min(
                    minimumScaleY,
                    maximumScaleY);

            double scaleYHigh =
                Math.Max(
                    minimumScaleY,
                    maximumScaleY);

            flames.Add(
                new DockThemeFlameLayer
                {
                    X = x,
                    WidthRatio =
                        Lerp(
                            minimumWidth,
                            maximumWidth,
                            random.NextDouble()),
                    HeightRatio =
                        height,
                    RiseRatio =
                        rise,
                    DriftRatio =
                        Lerp(
                            minimumDrift,
                            maximumDrift,
                            random.NextDouble()),
                    DurationSeconds =
                        Lerp(
                            minimumDuration,
                            maximumDuration,
                            random.NextDouble()),
                    DelaySeconds =
                        maximumDelay *
                        random.NextDouble(),
                    Opacity =
                        Lerp(
                            minimumOpacity,
                            maximumOpacity,
                            random.NextDouble()),
                    TipColor =
                        tipPalette[
                            random.Next(
                                tipPalette.Count)],
                    MidColor =
                        midPalette[
                            random.Next(
                                midPalette.Count)],
                    CoreColor =
                        corePalette[
                            random.Next(
                                corePalette.Count)],
                    MotionMode =
                        motionMode,
                    BaseYRatio =
                        baseYRatio,
                    MinimumScaleX =
                        scaleXLow,
                    MaximumScaleX =
                        scaleXHigh,
                    MinimumScaleY =
                        scaleYLow,
                    MaximumScaleY =
                        scaleYHigh,
                    SwayDegrees =
                        Lerp(
                            minimumSway,
                            maximumSway,
                            random.NextDouble()),
                    FlickerRatio =
                        Lerp(
                            minimumFlicker,
                            maximumFlicker,
                            random.NextDouble()),
                    BlurRadius =
                        Lerp(
                            minimumBlur,
                            maximumBlur,
                            random.NextDouble()),
                    TipStop =
                        tipStop,
                    MidStop =
                        midStop,
                    CoreStop =
                        coreStop
                });
        }

        return flames;
    }

    private static bool HasCompleteIndexedFlameLayers(
        DockThemeEffectDefinition effect,
        int count)
    {
        string[] requiredParameters =
        [
            "XValues",
            "WidthRatioValues",
            "HeightRatioValues",
            "RiseRatioValues",
            "DriftRatioValues",
            "DurationSecondsValues",
            "DelaySecondsValues",
            "OpacityValues",
            "TipColorValues",
            "MidColorValues",
            "CoreColorValues",
            "MinimumScaleXValues",
            "MaximumScaleXValues",
            "MinimumScaleYValues",
            "MaximumScaleYValues",
            "SwayDegreesValues",
            "FlickerRatioValues",
            "BlurRadiusValues",
            "MidStopValues"
        ];

        foreach (string parameterName
            in requiredParameters)
        {
            if (!TryGetEffectParameter(
                    effect,
                    parameterName,
                    out JsonElement value) ||
                value.ValueKind !=
                    JsonValueKind.Array ||
                value.GetArrayLength() <
                    count)
            {
                return false;
            }
        }

        return true;
    }

    private static List<DockThemeFlameLayer> GenerateIndexedFlameLayers(
        DockThemeEffectDefinition effect,
        int count)
    {
        string motionMode =
            GetEffectString(
                effect,
                "MotionMode",
                "NaturalFire");

        double baseYRatio =
            GetEffectDouble(
                effect,
                "BaseYRatio",
                1,
                -4,
                4);

        double tipStop =
            GetEffectDouble(
                effect,
                "TipStop",
                0,
                0,
                1);

        double coreStop =
            GetEffectDouble(
                effect,
                "CoreStop",
                1,
                0,
                1);

        List<DockThemeFlameLayer> flames =
            new(
                count);

        for (int index = 0;
             index < count;
             index++)
        {
            double minimumScaleX =
                GetEffectIndexedDouble(
                    effect,
                    "MinimumScaleXValues",
                    index,
                    0.72,
                    0.1,
                    4);

            double maximumScaleX =
                GetEffectIndexedDouble(
                    effect,
                    "MaximumScaleXValues",
                    index,
                    1.18,
                    0.1,
                    4);

            double minimumScaleY =
                GetEffectIndexedDouble(
                    effect,
                    "MinimumScaleYValues",
                    index,
                    0.62,
                    0.1,
                    4);

            double maximumScaleY =
                GetEffectIndexedDouble(
                    effect,
                    "MaximumScaleYValues",
                    index,
                    1.08,
                    0.1,
                    4);

            if (maximumScaleX <
                minimumScaleX)
            {
                (minimumScaleX, maximumScaleX) =
                    (maximumScaleX, minimumScaleX);
            }

            if (maximumScaleY <
                minimumScaleY)
            {
                (minimumScaleY, maximumScaleY) =
                    (maximumScaleY, minimumScaleY);
            }

            flames.Add(
                new DockThemeFlameLayer
                {
                    X =
                        GetEffectIndexedDouble(
                            effect,
                            "XValues",
                            index,
                            0.5,
                            0,
                            1),
                    WidthRatio =
                        GetEffectIndexedDouble(
                            effect,
                            "WidthRatioValues",
                            index,
                            0.08,
                            0.002,
                            0.50),
                    HeightRatio =
                        GetEffectIndexedDouble(
                            effect,
                            "HeightRatioValues",
                            index,
                            0.65,
                            0.01,
                            4),
                    RiseRatio =
                        GetEffectIndexedDouble(
                            effect,
                            "RiseRatioValues",
                            index,
                            0.22,
                            0,
                            4),
                    DriftRatio =
                        GetEffectIndexedDouble(
                            effect,
                            "DriftRatioValues",
                            index,
                            0.02,
                            0,
                            0.20),
                    DurationSeconds =
                        GetEffectIndexedDouble(
                            effect,
                            "DurationSecondsValues",
                            index,
                            2.4,
                            0.2,
                            120),
                    DelaySeconds =
                        GetEffectIndexedDouble(
                            effect,
                            "DelaySecondsValues",
                            index,
                            0,
                            0,
                            120),
                    Opacity =
                        GetEffectIndexedDouble(
                            effect,
                            "OpacityValues",
                            index,
                            0.8,
                            0,
                            1),
                    TipColor =
                        GetEffectIndexedString(
                            effect,
                            "TipColorValues",
                            index,
                            "#00FF3A00"),
                    MidColor =
                        GetEffectIndexedString(
                            effect,
                            "MidColorValues",
                            index,
                            "#DFFF6500"),
                    CoreColor =
                        GetEffectIndexedString(
                            effect,
                            "CoreColorValues",
                            index,
                            "#FFFFD35A"),
                    MotionMode =
                        motionMode,
                    BaseYRatio =
                        baseYRatio,
                    MinimumScaleX =
                        minimumScaleX,
                    MaximumScaleX =
                        maximumScaleX,
                    MinimumScaleY =
                        minimumScaleY,
                    MaximumScaleY =
                        maximumScaleY,
                    SwayDegrees =
                        GetEffectIndexedDouble(
                            effect,
                            "SwayDegreesValues",
                            index,
                            8,
                            0,
                            45),
                    FlickerRatio =
                        GetEffectIndexedDouble(
                            effect,
                            "FlickerRatioValues",
                            index,
                            0.22,
                            0,
                            1),
                    BlurRadius =
                        GetEffectIndexedDouble(
                            effect,
                            "BlurRadiusValues",
                            index,
                            0,
                            0,
                            32),
                    TipStop =
                        tipStop,
                    MidStop =
                        GetEffectIndexedDouble(
                            effect,
                            "MidStopValues",
                            index,
                            0.48,
                            0,
                            1),
                    CoreStop =
                        coreStop
                });
        }

        return flames;
    }

    private static double GetEffectIndexedDouble(
        DockThemeEffectDefinition effect,
        string name,
        int index,
        double fallback,
        double minimum,
        double maximum)
    {
        if (!TryGetEffectParameter(
                effect,
                name,
                out JsonElement value) ||
            value.ValueKind !=
                JsonValueKind.Array ||
            index < 0 ||
            index >=
                value.GetArrayLength())
        {
            return Math.Clamp(
                fallback,
                minimum,
                maximum);
        }

        JsonElement item =
            value[index];

        if (item.ValueKind !=
                JsonValueKind.Number ||
            !item.TryGetDouble(
                out double result))
        {
            return Math.Clamp(
                fallback,
                minimum,
                maximum);
        }

        return Math.Clamp(
            result,
            minimum,
            maximum);
    }

    private static string GetEffectIndexedString(
        DockThemeEffectDefinition effect,
        string name,
        int index,
        string fallback)
    {
        if (!TryGetEffectParameter(
                effect,
                name,
                out JsonElement value) ||
            value.ValueKind !=
                JsonValueKind.Array ||
            index < 0 ||
            index >=
                value.GetArrayLength())
        {
            return fallback;
        }

        JsonElement item =
            value[index];

        if (item.ValueKind !=
            JsonValueKind.String)
        {
            return fallback;
        }

        string? result =
            item.GetString();

        return string.IsNullOrWhiteSpace(
                   result)
            ? fallback
            : result.Trim();
    }

    private static int NormalizeSeed(
        int seed)
    {
        return seed == 0
            ? 1
            : seed;
    }

    private static bool TryGetEffectParameter(
        DockThemeEffectDefinition effect,
        string name,
        out JsonElement value)
    {
        if (effect.Parameters.TryGetValue(
                name,
                out value))
        {
            return true;
        }

        foreach (KeyValuePair<string, JsonElement> parameter
            in effect.Parameters)
        {
            if (string.Equals(
                    parameter.Key,
                    name,
                    StringComparison.OrdinalIgnoreCase))
            {
                value =
                    parameter.Value;

                return true;
            }
        }

        value =
            default;

        return false;
    }

    private static double GetEffectDouble(
        DockThemeEffectDefinition effect,
        string name,
        double fallback,
        double minimum,
        double maximum)
    {
        if (!TryGetEffectParameter(
                effect,
                name,
                out JsonElement value) ||
            value.ValueKind !=
                JsonValueKind.Number ||
            !value.TryGetDouble(
                out double result))
        {
            return Math.Clamp(
                fallback,
                minimum,
                maximum);
        }

        return Math.Clamp(
            result,
            minimum,
            maximum);
    }

    private static string GetEffectString(
        DockThemeEffectDefinition effect,
        string name,
        string fallback)
    {
        if (!TryGetEffectParameter(
                effect,
                name,
                out JsonElement value) ||
            value.ValueKind !=
                JsonValueKind.String)
        {
            return fallback;
        }

        string? result =
            value.GetString();

        return string.IsNullOrWhiteSpace(
                   result)
            ? fallback
            : result.Trim();
    }

    private static (double Minimum, double Maximum) GetEffectRange(
        DockThemeEffectDefinition effect,
        string name,
        double fallbackMinimum,
        double fallbackMaximum,
        double absoluteMinimum,
        double absoluteMaximum)
    {
        double minimum =
            fallbackMinimum;

        double maximum =
            fallbackMaximum;

        if (TryGetEffectParameter(
                effect,
                name,
                out JsonElement value))
        {
            if (value.ValueKind ==
                    JsonValueKind.Number &&
                value.TryGetDouble(
                    out double scalar))
            {
                minimum =
                    scalar;
                maximum =
                    scalar;
            }
            else if (value.ValueKind ==
                     JsonValueKind.Object)
            {
                if (value.TryGetProperty(
                        "Min",
                        out JsonElement minElement) &&
                    minElement.ValueKind ==
                        JsonValueKind.Number &&
                    minElement.TryGetDouble(
                        out double parsedMinimum))
                {
                    minimum =
                        parsedMinimum;
                }

                if (value.TryGetProperty(
                        "Max",
                        out JsonElement maxElement) &&
                    maxElement.ValueKind ==
                        JsonValueKind.Number &&
                    maxElement.TryGetDouble(
                        out double parsedMaximum))
                {
                    maximum =
                        parsedMaximum;
                }
            }
        }

        minimum =
            Math.Clamp(
                minimum,
                absoluteMinimum,
                absoluteMaximum);

        maximum =
            Math.Clamp(
                maximum,
                absoluteMinimum,
                absoluteMaximum);

        if (maximum <
            minimum)
        {
            (minimum, maximum) =
                (maximum, minimum);
        }

        return (minimum, maximum);
    }

    private static List<string> GetEffectStringList(
        DockThemeEffectDefinition effect,
        string name)
    {
        List<string> values =
            new();

        if (!TryGetEffectParameter(
                effect,
                name,
                out JsonElement value) ||
            value.ValueKind !=
                JsonValueKind.Array)
        {
            return values;
        }

        foreach (JsonElement item
            in value.EnumerateArray())
        {
            if (item.ValueKind !=
                JsonValueKind.String)
            {
                continue;
            }

            string? text =
                item.GetString();

            if (!string.IsNullOrWhiteSpace(
                    text))
            {
                values.Add(
                    text.Trim());
            }
        }

        return values;
    }

    private static void EnsurePalette(
        List<string> palette,
        string fallback)
    {
        if (palette.Count == 0)
        {
            palette.Add(
                fallback);
        }
    }

    private static List<DockThemeStarLayer> GenerateStarLayers(
        DockTheme theme)
    {
        int count =
            Math.Clamp(
                theme.GlassStarGeneratedCount,
                1,
                256);

        int seed =
            theme.GlassStarGeneratedSeed == 0
                ? 1
                : theme.GlassStarGeneratedSeed;

        Random random =
            new(seed);

        double paddingX =
            Math.Clamp(
                theme.GlassStarGeneratedPaddingXRatio,
                0,
                0.45);

        double topPadding =
            Math.Clamp(
                theme.GlassStarGeneratedTopPaddingRatio,
                0,
                0.80);

        double bottomPadding =
            Math.Clamp(
                theme.GlassStarGeneratedBottomPaddingRatio,
                0,
                0.80);

        if (topPadding + bottomPadding > 0.92)
        {
            bottomPadding =
                Math.Max(
                    0,
                    0.92 - topPadding);
        }

        double minimumSize =
            Math.Max(
                0.5,
                theme.GlassStarGeneratedMinimumSize);

        double maximumSize =
            Math.Max(
                minimumSize,
                theme.GlassStarGeneratedMaximumSize);

        double minimumOpacity =
            Math.Clamp(
                theme.GlassStarGeneratedMinimumOpacity,
                0.02,
                1);

        double maximumOpacity =
            Math.Clamp(
                theme.GlassStarGeneratedMaximumOpacity,
                minimumOpacity,
                1);

        double minimumDuration =
            Math.Max(
                0.2,
                theme.GlassStarGeneratedMinimumDurationSeconds);

        double maximumDuration =
            Math.Max(
                minimumDuration,
                theme.GlassStarGeneratedMaximumDurationSeconds);

        double maximumDelay =
            Math.Max(
                0,
                theme.GlassStarGeneratedMaximumDelaySeconds);

        double minimumGlowRadius =
            Math.Max(
                0,
                theme.GlassStarGeneratedMinimumGlowRadius);

        double maximumGlowRadius =
            Math.Max(
                minimumGlowRadius,
                theme.GlassStarGeneratedMaximumGlowRadius);

        double minimumScale =
            Math.Max(
                0.1,
                theme.GlassStarGeneratedMinimumScale);

        double maximumScale =
            Math.Max(
                minimumScale,
                theme.GlassStarGeneratedMaximumScale);

        double maximumDriftXRatio =
            Math.Clamp(
                theme.GlassStarGeneratedMaximumDriftXRatio,
                0,
                0.10);

        double maximumDriftYRatio =
            Math.Clamp(
                theme.GlassStarGeneratedMaximumDriftYRatio,
                0,
                0.10);

        List<string> palette =
            new();

        foreach (string color
            in theme.GlassStarGeneratedPalette)
        {
            if (!string.IsNullOrWhiteSpace(
                    color))
            {
                palette.Add(
                    color);
            }
        }

        if (palette.Count == 0)
        {
            palette.Add(
                "#FFF8FEFF");
            palette.Add(
                "#FFDDEBFF");
            palette.Add(
                "#FFB7D9FF");
            palette.Add(
                "#FFF8F6D8");
        }

        List<DockThemeStarLayer> stars =
            new(
                count);

        for (int index = 0;
             index < count;
             index++)
        {
            double x =
                Lerp(
                    paddingX,
                    1 - paddingX,
                    random.NextDouble());

            double y =
                Lerp(
                    topPadding,
                    1 - bottomPadding,
                    random.NextDouble());

            double sizeBias =
                random.NextDouble();

            sizeBias *= sizeBias;

            double size =
                Lerp(
                    minimumSize,
                    maximumSize,
                    sizeBias);

            if (random.NextDouble() < 0.16)
            {
                size =
                    Math.Min(
                        maximumSize,
                        size * 1.28);
            }

            double minimumStarOpacity =
                Lerp(
                    minimumOpacity,
                    maximumOpacity,
                    random.NextDouble() * 0.55);

            double maximumStarOpacity =
                Lerp(
                    Math.Max(
                        minimumStarOpacity,
                        minimumOpacity + 0.08),
                    maximumOpacity,
                    random.NextDouble());

            if (maximumStarOpacity <
                minimumStarOpacity)
            {
                maximumStarOpacity =
                    minimumStarOpacity;
            }

            double duration =
                Lerp(
                    minimumDuration,
                    maximumDuration,
                    random.NextDouble());

            double delay =
                maximumDelay * random.NextDouble();

            double glowRadius =
                Lerp(
                    minimumGlowRadius,
                    maximumGlowRadius,
                    random.NextDouble());

            double driftXRatio =
                Lerp(
                    -maximumDriftXRatio,
                    maximumDriftXRatio,
                    random.NextDouble());

            double driftYRatio =
                Lerp(
                    -maximumDriftYRatio,
                    maximumDriftYRatio,
                    random.NextDouble());

            stars.Add(
                new DockThemeStarLayer
                {
                    X = x,
                    Y = y,
                    Size = size,
                    Color =
                        palette[
                            random.Next(
                                palette.Count)],
                    MinimumOpacity =
                        minimumStarOpacity,
                    MaximumOpacity =
                        maximumStarOpacity,
                    DurationSeconds =
                        duration,
                    DelaySeconds =
                        delay,
                    GlowRadius =
                        glowRadius,
                    MinimumScale =
                        minimumScale,
                    MaximumScale =
                        maximumScale,
                    DriftXRatio =
                        driftXRatio,
                    DriftYRatio =
                        driftYRatio
                });
        }

        return stars;
    }

    private static double Lerp(
        double start,
        double end,
        double progress)
    {
        return start +
               ((end - start) *
                progress);
    }

    private static DockTheme? ReadTheme(string json)
    {
        DockTheme? theme = JsonSerializer.Deserialize<DockTheme>(json, JsonOptions);
        // Semantic Anchor [Portable effect validation]: The existing catalog and JSON remain
        // the only application theme SSOT. Validate new effects using the portable Core
        // allowlist before making them selectable; do not migrate or rewrite imported JSON.
        if (theme is not null && VisualThemeRuntime.NeedsPortableRenderer(theme))
            _ = PortableThemes.Core.ThemeDefinition.Parse(
                System.Text.Encoding.UTF8.GetBytes(json), "ValidatedEffect");
        PopulateGeneratedThemeLayers(theme);
        return theme;
    }

    private static DockTheme? LoadFile(
        string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return null;
            }

            string json =
                File.ReadAllText(
                    path);

            return ReadTheme(json);
        }
        catch
        {
            return null;
        }
    }
}
