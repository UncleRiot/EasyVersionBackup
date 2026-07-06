# EasyVersionBackup

EasyVersionBackup is a lightweight, portable Windows tool for creating versioned backups of folders. 
Backup your savegame-folders, office-document-folders, VisualC project folders, every folder.

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
> VirusTotal scan: [https://www.virustotal.com/gui/file/f6b9a2e7084680248d013ac2d0de8cd8c88d7aece87dee745a26e25aee0967d6?nocache=1](https://www.virustotal.com/gui/file/4a8a5f940efd69f19dc4cb00eab5841c687e8cefff3e9b5c22b2bca593eeae03?nocache=1)

<br>

---

<br>
<img width="806" height="262" alt="grafik" src="https://github.com/user-attachments/assets/b09bffdb-9053-456b-8596-d1ca3fd8e6ba" />
<br>

<br>
<img width="417" height="446" alt="grafik" src="https://github.com/user-attachments/assets/6f5feaea-6cf6-45f4-bc0b-8e354f53cbba" />
<br>

<br>
<img width="523" height="190" alt="grafik" src="https://github.com/user-attachments/assets/5a92894d-5d78-4d0f-a877-355fb9efddd2" />
<br>

<br>
<img width="1232" height="358" alt="grafik"  src="https://github.com/user-attachments/assets/387485ed-64b3-43f4-a9be-f33864569674" />
<br>

<br>
<img width="501" height="300" alt="grafik" src="https://github.com/user-attachments/assets/d10fa442-f473-41ec-b1b6-d947ea1e5624" />
<br>

<br>
<img width="507" height="236" alt="grafik"src="https://github.com/user-attachments/assets/80810de1-43a3-431f-b2c8-2a82b75e1682" />
<br>

<br>
<img width="497" height="320" alt="grafik"  src="https://github.com/user-attachments/assets/6a0f8e1f-7582-42b8-aacf-82d454b6e2a7" />
<br>

<br>
<img width="499" height="169" alt="grafik"  src="https://github.com/user-attachments/assets/a4b50590-0f0a-43f6-8243-d30cb866a259" />
<br>

<br>
<img width="504" height="172" alt="grafik" src="https://github.com/user-attachments/assets/20bbdb84-0a3e-479e-8633-7dffccc1ddd4" />
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

  * Runs fully in the background
  * Works while you use other applications
  * No interaction required once configured
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

* **Zip Destination Files**

  * Store backups as `.zip` archives instead of normal folders

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

* **Per-Path Exclusions**

  * Exclude folders or files from individual backup entries

---

## 💡 Use Cases

* Backup Visual Studio projects
* Create safe snapshots before refactoring
* Save versions before deployments or releases
* Run automatic backups while working
