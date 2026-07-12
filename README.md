> [!WARNING]
> This software can permanently delete source and destination data.
> Read the [Data-Loss Warning and Disclaimer](DISCLAIMER.md) before use.


# EasyVersionBackup

EasyVersionBackup is a lightweight Windows tool for creating versioned backups of folders. 
Back up save-game folders, office-document folders, Visual C# project folders, and other local directories.

It supports manual and automatic backups, multiple source → target configurations, ZIP backups, per-entry exclusions, skipped-file reporting, system tray integration, and a clean WinForms interface.

<br>

---

<br>

## ❤️ Support

EasyVersionBackup is free to use.
If this tool saves you time, you can support development here:

<a href="https://ko-fi.com/uncleriot">
  <img src="https://storage.ko-fi.com/cdn/kofi2.png?v=3" height="36" alt="Support EasyVersionBackup on Ko-fi" />
</a>

<br>
<br>

---

<br>
<br>


> [!IMPORTANT]
> If Windows SmartScreen blocks the app, right-click the EXE → **Properties** → check **Unblock** → **Apply** → **OK**, then start it again.
>

<br>

---


Main-Window

<br>
<img width="956" height="326" alt="grafik" src="https://github.com/user-attachments/assets/fa575d05-ad64-47d0-b571-23e36ca8c9a0" />
<br>


Backup Pair Settings

<br>
<img width="420" height="545" alt="grafik" src="https://github.com/user-attachments/assets/1c40f1c8-8850-43b3-942b-96adca495d70" />
<br>


Source Cleanup

<br>
<img width="620" height="470" alt="grafik" src="https://github.com/user-attachments/assets/5d085162-6e20-4bb3-a952-b72d406892bd" />
<br>

<br>
<img width="520" height="353" alt="grafik" src="https://github.com/user-attachments/assets/0eb99537-3643-44b5-a4f5-a3ecc3d5efd3" />
<br>

<br>
<img width="760" height="430" alt="grafik" src="https://github.com/user-attachments/assets/caee9f1d-9609-41fb-b16b-bf744374f555" />
<br>


<br>
<br>
<br>

---



## 🚀 Features

### Versioned Backups

- Create structured backups with version numbers
- Supports manual input, auto-increment, and timestamp-based versions
- Remembers manually adjusted versions per source → target pair
- Detects existing versions and suggests the next logical version
- Updated versions are stored only after a successful backup
- Canceling a backup does not advance or persist the version number

### Multiple Backup Configurations

- Configure multiple source → target pairs
- Enable or disable each entry individually
- Edit source and target paths directly in the main window
- Browse folders directly from the main table
- Configure versioning, retention, source cleanup, timers, and error handling per backup pair

### Per-Entry Exclusions

- Define excluded files or folders per backup entry
- Edit exclusions directly from the main window or Settings
- Add and remove exclusions through a table-based editor
- Confirmation prompt before removing existing exclusion entries
- Supports source-relative paths and glob-style patterns

Examples:

    bin
    bin\
    .sav
    *.sav
    Saved\SaveGames\*.sav
    **\SaveGames\*.sav
    Saved\**\*.sav

Pattern behavior:

- `.sav` and `*.sav` match `.sav` files
- `*` matches characters within one path level
- `**` matches any number of folder levels
- A trailing `\` restricts the rule to folders
- Matching is case-insensitive
- Wildcard-only exclusions are rejected for safety

### Backup Retention

- Configure retention separately for every backup pair
- Keep the newest number of backups
- Keep backups for a configured number of days
- Combine retention rules using `AND` or `OR`
- Protect tagged backups such as `Final` or `OK`
- Preview affected backups before permanent deletion
- Choose between:
  - Canceling the complete backup
  - Continuing without deleting old backups
  - Creating the backup and deleting confirmed retention candidates
- Destructive actions are clearly highlighted

### Source Cleanup

- Remove old source files after a successful backup
- Configure cleanup rules separately for every backup pair
- Supports multiple file rules, for example:

    .sav
    Saved\SaveGames\.sav
    Saved\SaveGames\*.sav

- Keep only the newest configured number of files
- Or delete files modified before a selected date
- Cleanup is restricted to explicitly configured file types and source-relative folders
- Directories are never deleted
- Files outside the configured rules remain untouched
- Cleanup does not run when the backup fails or is canceled

### Flexible Backup Timer

- Define intervals like `30s`, `5m`, or `1h`
- Configure a global timer or a custom timer per backup pair
- Live countdown visible:
  - In the main UI
  - In the window title
  - In the system tray tooltip

### Silent Backup Progress

- Backup progress is displayed directly inside the main table
- Source-related operations fill the source-directory cell
- Target-related operations fill the target-directory cell
- The timer column temporarily shows the current percentage
- Indeterminate phases use a subtle moving progress fill
- The normal timer value is restored after completion
- Multiple backup pairs can show progress independently
- No additional progress window or focus change is required

### Automatic Background Backups

- File and ZIP creation runs outside the UI thread
- Works while you use other applications
- Automatic backups remain silent unless user interaction is required
- Destination-conflict and retention confirmations can still require interaction
- Optional minimize-to-system-tray behavior

### Status & Feedback System

- Per-entry status indicator:
  - 🟢 OK
  - 🟡 Warning
  - 🔴 Error
- Hover for quick details:
  - Last backup time
  - Error messages
  - Skipped-file summary
- Click the info icon to open a copyable and scrollable details dialog
- Skipped files are listed individually when available
- Skipped files do not automatically mark a successful backup as failed
- Retention is indicated by a red `R` marker
- Source cleanup is indicated by a cyan `C` marker
- Both markers are displayed when both features are active

### ZIP Support

- Optional compression into `.zip` archives
- Uses the same versioning logic as folder-based backups
- Temporary ZIP files are finalized only after successful creation
- No additional metadata sidecar files are created

### Robust Error Handling

- Option to ignore locked or in-use files
- Backup can continue without interruption
- Skipped files are reported per backup entry
- Skipped-file summaries distinguish between files and folders
- Failed or canceled backups do not trigger source cleanup
- Useful for projects with temporary build files, IDE locks, or active services

### System Tray Integration

- Minimize to tray
- Tray tooltip shows the next scheduled backup:

    Next backup in 2 minutes (19:52)

- Manual and automatic backups can show tray notifications
- Canceled backups are reported as canceled instead of completed

### Modernized UI

- Clean, compact main window
- Toolbar with quick actions:
  - Exit
  - Add path
  - Remove path
  - Settings
  - About
  - Backup
- Modern themed dialogs and scrollbars
- Clear visual distinction between normal and destructive actions
- Main window initially shows only a small number of rows
- Window remains resizable
- Resize indicator included
- Tooltips and contextual help added for important controls

---

## 🧩 How It Works

1. Configure one or more **source → target** paths
2. Optionally define exclusions per backup pair
3. Choose a versioning method
4. Optionally configure:
   - Retention
   - Source cleanup
   - Custom backup timer
   - Error handling
5. Click **Backup** or let the configured timer start it automatically
6. The app will:
   - Validate the source and target paths
   - Create a versioned directory or ZIP archive
   - Report progress directly in the backup-pair row
   - Skip problematic files if configured
   - Apply source cleanup only after a successful backup
   - Apply retention only after user confirmation
   - Store the final backup status per entry
   - Persist the selected version only after success

---

## ⚙️ Settings Overview

### Backup Timer

Flexible input:

- `30s` → 30 seconds
- `5m` → 5 minutes
- `1h` → 1 hour
- `15` → 15 minutes by default

### Default Versioning

Starting version or pattern, for example:

- `none`
- `v1.0`
- `1.0`
- `yyyy-MM-dd`
- `yyyyMMdd`
- `yyyy-MM-dd-HH-mm`
- `yyyyMMddHHmm`

### Auto Increment

- Automatically increases compatible version numbers
- Updated versions are saved only after a successful backup

### Retention

- Keep the latest configured number of backups
- Keep backups for a configured number of days
- Combine enabled rules with `AND` or `OR`
- Exclude tagged backups from deletion

### Source Cleanup

- Delete only explicitly configured file types
- Restrict cleanup to specific source-relative folders
- Keep the newest files or delete files older than a selected date

### Minimize to Systray

- Keeps the app running quietly in the background

### Ignore Copy Errors

- Skips locked or problematic files instead of stopping the backup

### ZIP Backups

- Enable to create one `.zip` archive per backup
- Disable to create a versioned backup directory

### Per-Path Exclusions

- Exclude folders or files from individual backup entries
- Supports simple names, file extensions, relative paths, `*`, and `**`

---

## 🖥️ Runtime Requirement

The published application targets Windows x64 and requires the .NET 8 Desktop Runtime because the project is framework-dependent (`SelfContained=false`).

Application settings are stored in:

    <application directory>\Settings\EasyVersionBackup.settings.json

The application no longer creates `.evbmeta` sidecar files beside ZIP backups.

---

## 💡 Use Cases

- Backup Visual Studio projects
- Create safe snapshots before refactoring
- Preserve game saves while automatically removing older save files
- Keep only a defined number of recent backups
- Protect release backups using retention tags
- Save versions before deployments or releases
- Run automatic backups while working
