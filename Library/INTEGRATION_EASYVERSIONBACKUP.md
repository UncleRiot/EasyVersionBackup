# EasyVersionBackup optional Core integration

This patch adds an SDK-style `Library/PortableThemes.Core` class library as a normal
project reference. Only new Background renderers `Aurora`, `Ember`, `DeepCurrent`,
`Gradient` are rendered through Core in EasyVersionBackup. Existing WinForms,
WPF-inspired glass, star, smoke and Campfire remain on the prior renderer.

- The host's `DockThemeService` + `Themes/*.json` are the application preset SSOT.
- `AppSettings` owns selected theme ID and motion preferences; Save/Cancel is unchanged.
- `ModernTheme` owns all WinForms colors.
- `ModernWindowFrame.ThemeSurface` owns one cached image and one timer per form; a
  `VisualThemePortableCanvas` exists only for portable-only effects and has NO timer.
- The preview gets one independent canvas, driven by its already existing timer.
- The portable Core `ThemeCatalog` remains available to other applications but is not
  used in EVB to avoid a second, conflicting imported-theme catalog.
- Existing imported theme paths, IDs, settings, popup behavior, and backups are not migrated.

Build from VS2026 (Windows, .NET 10 SDK): build **entire solution** to include Core,
then test: new presets, existing presets, import, Save/Cancel, reduced motion, speed,
contrast, inactive/minimized form pause, resize, DPI, perf/CPU/RAM and single-file publish.

The Core source originates from the user-provided Theme Toolkit 2.0 and carries
its original provenance/license caveats; review GPLv3 compatibility before distribution.
