// Semantic Anchor [Visual theme source reference]: Original GlueDock theme model and generated effect calculations; JSON presets are the SSOT.
using System.Text.Json;

namespace EasyVersionBackup.VisualThemes;

public sealed class DockTheme
{
    public string Name { get; set; } = "Default";

    public string DockBackgroundColor { get; set; } = "#12161C";

    public string ItemBackgroundColor { get; set; } = "#22FFFFFF";

    public string ItemBorderColor { get; set; } = "#22FFFFFF";

    public string TextColor { get; set; } = "#FFFFFF";

    public string SubmenuIndicatorColor { get; set; } = "#FFFFFF";

    public string DragGhostBackgroundColor { get; set; } = "#22FFFFFF";

    public string DragGhostBorderColor { get; set; } = "#66FFFFFF";

    public bool GlassSurfaceEnabled { get; set; }

    public string GlassTopColor { get; set; } = "#00FFFFFF";

    public string GlassBottomColor { get; set; } = "#00FFFFFF";

    public string GlassHighlightColor { get; set; } = "#00FFFFFF";

    public string GlassShadowColor { get; set; } = "#000000";

    public double GlassGradientStartX { get; set; } = 0.5;

    public double GlassGradientStartY { get; set; }

    public double GlassGradientEndX { get; set; } = 0.5;

    public double GlassGradientEndY { get; set; } = 1;

    public bool GlassGradientAnimationEnabled { get; set; }

    public double GlassGradientAnimationDurationSeconds { get; set; } = 6;

    public bool GlassGradientAnimationAutoReverse { get; set; } = true;

    public string GlassGradientAnimationEasing { get; set; } = "Linear";

    public string GlassGradientAnimationEasingMode { get; set; } = "EaseInOut";

    public string GlassGradientAnimationMode { get; set; } = "Points";

    public double GlassGradientAnimationRotationDegrees { get; set; } = 360;

    public bool GlassGradientAnimationAspectCorrect { get; set; }

    public double GlassGradientAnimationToStartX { get; set; } = 0.5;

    public double GlassGradientAnimationToStartY { get; set; }

    public double GlassGradientAnimationToEndX { get; set; } = 0.5;

    public double GlassGradientAnimationToEndY { get; set; } = 1;

    public List<DockThemeGradientKeyFrame> GlassGradientAnimationKeyFrames { get; set; } = new();

    public bool GlassFlameEffectEnabled { get; set; }

    public bool GlassFlameBaseGlowEnabled { get; set; }

    public string GlassFlameBaseGlowColor { get; set; } = "#80FF5A00";

    public double GlassFlameBaseGlowHeightRatio { get; set; } = 0.22;

    public double GlassFlameBaseGlowOpacity { get; set; } = 0.5;

    public double GlassFlameTopFadeStartRatio { get; set; } = 0.08;

    public double GlassFlameTopFadeEndRatio { get; set; } = 0.42;

    public List<DockThemeFlameLayer> GlassFlameLayers { get; set; } = new();

    public bool GlassStarEffectEnabled { get; set; }

    public List<DockThemeStarLayer> GlassStarLayers { get; set; } = new();

    public int GlassStarGeneratedCount { get; set; }

    public int GlassStarGeneratedSeed { get; set; } = 1;

    public double GlassStarGeneratedPaddingXRatio { get; set; } = 0.04;

    public double GlassStarGeneratedTopPaddingRatio { get; set; } = 0.08;

    public double GlassStarGeneratedBottomPaddingRatio { get; set; } = 0.18;

    public double GlassStarGeneratedMinimumSize { get; set; } = 1.4;

    public double GlassStarGeneratedMaximumSize { get; set; } = 5.8;

    public double GlassStarGeneratedMinimumOpacity { get; set; } = 0.24;

    public double GlassStarGeneratedMaximumOpacity { get; set; } = 0.98;

    public double GlassStarGeneratedMinimumDurationSeconds { get; set; } = 2.4;

    public double GlassStarGeneratedMaximumDurationSeconds { get; set; } = 6.8;

    public double GlassStarGeneratedMaximumDelaySeconds { get; set; } = 2.8;

    public double GlassStarGeneratedMinimumGlowRadius { get; set; } = 1.2;

    public double GlassStarGeneratedMaximumGlowRadius { get; set; } = 4.6;

    public double GlassStarGeneratedMinimumScale { get; set; } = 0.72;

    public double GlassStarGeneratedMaximumScale { get; set; } = 1.28;

    public double GlassStarGeneratedMaximumDriftXRatio { get; set; } = 0.0035;

    public double GlassStarGeneratedMaximumDriftYRatio { get; set; } = 0.0025;

    public List<string> GlassStarGeneratedPalette { get; set; } = new();

    public int GlassEffectSchemaVersion { get; set; } = 1;

    public List<DockThemeEffectDefinition> GlassEffects { get; set; } = new();

    public double GlassCornerRadius { get; set; } = 20;

    public double GlassExtraThickness { get; set; }
}

public sealed class DockThemeGradientKeyFrame
{
    public double Progress { get; set; }

    public double StartX { get; set; } = 0.5;

    public double StartY { get; set; }

    public double EndX { get; set; } = 0.5;

    public double EndY { get; set; } = 1;
}


public sealed class DockThemeFlameLayer
{
    public double X { get; set; } = 0.5;

    public double WidthRatio { get; set; } = 0.08;

    public double HeightRatio { get; set; } = 0.65;

    public double RiseRatio { get; set; } = 0.22;

    public double DriftRatio { get; set; } = 0.02;

    public double DurationSeconds { get; set; } = 2.4;

    public double DelaySeconds { get; set; }

    public double Opacity { get; set; } = 0.8;

    public string TipColor { get; set; } = "#00FF3A00";

    public string MidColor { get; set; } = "#DFFF6500";

    public string CoreColor { get; set; } = "#FFFFD35A";

    public string MotionMode { get; set; } = "Rise";

    public double BaseYRatio { get; set; } = 1;

    public double MinimumScaleX { get; set; } = 0.72;

    public double MaximumScaleX { get; set; } = 1.18;

    public double MinimumScaleY { get; set; } = 0.62;

    public double MaximumScaleY { get; set; } = 1.08;

    public double SwayDegrees { get; set; } = 8;

    public double FlickerRatio { get; set; } = 0.22;

    public double BlurRadius { get; set; }

    public double TipStop { get; set; }

    public double MidStop { get; set; } = 0.48;

    public double CoreStop { get; set; } = 1;
}

public sealed class DockThemeStarLayer
{
    public double X { get; set; } = 0.5;

    public double Y { get; set; } = 0.5;

    public double Size { get; set; } = 2;

    public string Color { get; set; } = "#FFFFFFFF";

    public double MinimumOpacity { get; set; } = 0.18;

    public double MaximumOpacity { get; set; } = 0.95;

    public double DurationSeconds { get; set; } = 4;

    public double DelaySeconds { get; set; }

    public double GlowRadius { get; set; } = 2;

    public double MinimumScale { get; set; } = 0.72;

    public double MaximumScale { get; set; } = 1.18;

    public double DriftXRatio { get; set; }

    public double DriftYRatio { get; set; }
}

public sealed class DockThemeEffectDefinition
{
    public string Type { get; set; } = string.Empty;

    public string Renderer { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    public int Count { get; set; }

    public int Seed { get; set; } = 1;

    public Dictionary<string, JsonElement> Parameters { get; set; } =
        new(
            StringComparer.OrdinalIgnoreCase);
}

public sealed record DockThemeOption(
    string Id,
    string DisplayName)
{
    public override string ToString()
    {
        return DisplayName;
    }
}
