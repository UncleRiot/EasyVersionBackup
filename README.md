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

<br>
<img width="956" height="326" alt="grafik" src="https://github.com/user-attachments/assets/c0c3a094-8d77-407e-8464-49fcc665ce75" />

<br>

<br>
<img width="720" height="480" alt="grafik" src="https://github.com/user-attachments/assets/6215d6ef-fdb2-48dc-8213-ba0d02ee1113" />
<br>

<br>
<img width="420" height="485" alt="grafik" src="https://github.com/user-attachments/assets/fb3fd8db-0252-4f1e-8241-79683586c527" />
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

* **Versioned Backups**

  * Create structured backups with version numbers
  * Supports manual input, auto-increment, and timestamp-based versions
  * Remembers manually adjusted versions per source → target pair
  * Detects existing versions and suggests the next logical version

* **Multiple Backup Configurations**

  * Configure multiple source → target pairs
  * Enable or disable each entry individually
  * Edit source and target paths directly in the main window
  * Browse folders directly from the main table

* **Per-Entry Exclusions**

  * Define excluded files or folders per backup entry
  * Edit exclusions directly from the main window or Settings
  * Add and remove exclusions via a table-based editor
  * Confirmation prompt before removing existing exclusion entries

* **Flexible Backup Timer**

  * Define intervals like `30s`, `5m`, `1h`
  * Live countdown visible:
    * In the main UI
    * In the window title
    * In the system tray tooltip

* **Automatic Background Backups**

  * File and ZIP creation runs outside the UI thread
  * Works while you use other applications
  * Destination-conflict and retention confirmations can still require interaction
  * Optional minimize-to-system-tray behavior

* **Status & Feedback System**

  * Per-entry status indicator:
    * 🟢 OK
    * 🟡 Warning
    * 🔴 Error
  * Hover for quick details:
    * Last backup time
    * Error messages
    * Skipped-file summary
  * Click the info icon to open a copyable details dialog
  * Skipped files are listed individually when available

* **ZIP Support**

  * Optional compression into `.zip` archives
  * Uses the same versioning logic as folder-based backups

* **Robust Error Handling**

  * Option to ignore locked or in-use files
  * Backup can continue without interruption
  * Skipped files are reported per backup entry
  * Useful for projects with temporary build files, IDE locks, or active services

* **System Tray Integration**

  * Minimize to tray
  * Tray tooltip shows the next scheduled backup:

    ```text
    Next backup in 2 minutes (19:52)
    ```

  * Manual and automatic backups can show tray notifications

* **Modernized UI**

  * Clean, compact main window
  * Toolbar with quick actions:
    * Exit
    * Add path
    * Remove path
    * Settings
    * About
    * Backup
  * Main window initially shows only a small number of rows
  * Window remains resizable
  * Resize indicator included
  * Tooltips/hints added for important controls

---

## 🧩 How It Works

1. Configure one or more **source → target** paths
2. Optionally define exclusions per source path
3. Choose versioning behavior
4. Optionally enable the **Backup Timer**
5. Click **Backup** or let it run automatically
6. The app will:
   * Create versioned copies or ZIP archives
   * Skip problematic files if configured
   * Store backup status per entry
   * Remember the last used version per source → target pair

---

## ⚙️ Settings Overview

* **Backup Timer**

  * Flexible input:
    * `30s` → 30 seconds
    * `5m` → 5 minutes
    * `1h` → 1 hour
    * `15` → 15 minutes by default

* **Default Versioning**

  * Starting version or pattern, for example:
    * `none`
    * `v1.0`
    * `1.0`
    * `yyyy-MM-dd`
    * `yyyyMMdd`
    * `yyyy-MM-dd-HH-mm`
    * `yyyyMMddHHmm`

* **Auto Increment**

  * Automatically increases compatible version numbers

* **Minimize to Systray**

  * Keeps the app running quietly in the background

* **Ignore Copy Errors**

  * Skips locked or problematic files instead of stopping the backup

* **ZIP Backups**

  * Enable to create one `.zip` archive per backup
  * Disable to create a versioned backup directory

* **Per-Path Exclusions**

  * Exclude folders or files from individual backup entries

---

## 🖥️ Runtime requirement

The published application targets Windows x64 and requires the .NET 8 Desktop Runtime because the project is framework-dependent (`SelfContained=false`).

Application settings are stored in `%LOCALAPPDATA%\EasyVersionBackup`. Existing settings from the legacy `Settings` folder beside the executable are imported automatically on first start.

ZIP backups created by this version receive a small `.evbmeta` sidecar file. Retention only deletes ZIP backups whose metadata matches the configured source directory; legacy or unrelated ZIP files are left untouched.

---

## 💡 Use Cases

* Backup Visual Studio projects
* Create safe snapshots before refactoring
* Save versions before deployments or releases
* Run automatic backups while working
