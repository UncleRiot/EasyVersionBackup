# Corrections

This package contains the corrected source version.

## Implemented

- Safe temporary backup creation and rollback-safe replacement
- Source/target and cross-entry path validation
- Reparse-point protection
- File and ZIP work outside the WinForms UI thread
- Consistent copy/enumeration error handling
- Separation of configured date patterns from generated versions
- Per-entry continuation after failures
- Correct automatic-backup result counts
- Retention ownership metadata, safe tag matching, and confirmed-candidate deletion
- Atomic settings writes, legacy-settings migration, and normalized pair keys
- Applied log level, structured backup start/end markers, and log cleanup
- Lossless settings cloning and a ZIP-backup setting in the UI
- Timer parsing without integer overflow
- Missing-project-resource references removed
- Solution, license, documentation, and machine-specific project files cleaned up

## Intentional exception

`SkipDialogs` behavior for destination-conflict and retention dialogs was intentionally left unchanged, as requested.

## Validation

The source tree was statically checked for balanced C# delimiters, valid project XML, missing Designer event-handler definitions, stale removed identifiers, and archive consistency.

A real Visual Studio/.NET build and runtime UI test could not be executed in the available environment because no .NET SDK, MSBuild, or C# compiler is installed.
