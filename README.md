# EasyVersionBackup

EasyVersionBackup is a lightweight Windows application for creating **versioned backups** of your directories — fast, reliable, and focused on real-world usage without unnecessary complexity.
Use it for your save-games, projects, everything!
---

<img width="755" height="234" alt="grafik" src="https://github.com/user-attachments/assets/37353745-12ab-4665-8edd-4b73ca6fc58d" />

<img width="892" height="350" alt="grafik" src="https://github.com/user-attachments/assets/8ceaa7b0-7e99-4a26-a4e1-e9d4dd37109d" />


## 🚀 Features

* **Versioned Backups**

  * Create structured backups with version numbers
  * Supports manual input, auto-increment, and timestamp-based versions

* **Flexible Backup Timer**

  * Define intervals like `30s`, `5m`, `1h`
  * Live countdown visible:
    * In the UI
    * In the window title
    * In the system tray tooltip

* **Automatic Background Backups**

  * Runs fully in the background
  * Works while you use other applications (e.g. games)
  * No interaction required once configured

* **Multiple Backup Configurations**

  * Configure multiple source → target pairs
  * Enable/disable each entry individually

* **Status & Feedback System**

  * Per-entry status indicator:

    * 🟢 OK
    * 🟡 Warning
    * 🔴 Error
  * Hover for details:

    * Last backup time
    * Error messages (if any)

* **ZIP Support**

  * Optional compression into `.zip` archives

* **Robust Error Handling**

  * Option to ignore locked/in-use files
  * Backup continues without interruption
  * Reports skipped files

* **System Tray Integration**

  * Minimize to tray
  * Hover shows:

    ```
    Next Backup in 2 minutes (19:52)
    ```

* **Modernized UI**

  * Clean, minimal layout
  * Focus on clarity and speed
  * No clutter, no unnecessary dialogs

---

## 🧩 How It Works

1. Configure one or more **source → target** paths
2. (Optional) Enable **Backup Timer**
3. Click **Backup** or let it run automatically
4. The app will:

   * Create versioned copies
   * Skip problematic files if configured
   * Store status per entry

---

## ⚙️ Settings Overview

* **Backup Timer**

  * Flexible input:

    * `30s` → 30 seconds
    * `5m` → 5 minutes
    * `1h` → 1 hour
    * `15` → 15 minutes (default)

* **Zip Destination Files**

  * Store backups as `.zip`

* **Default Versioning**

  * Starting version (e.g. `0.0.1`)

* **Auto Increment**

  * Automatically increase version numbers

* **Minimize to Systray**

  * Run silently in background

* **Ignore Copy Errors**

  * Skip locked/problematic files

---

## 💡 Use Cases

* Backup Visual Studio projects
* Safe snapshots before deployments
* Background backups while working or gaming
* Handling projects with locked files (services, builds, etc.)

---

## 🛠️ Tech Stack

* C#
* .NET (Windows Forms)
* No external dependencies

---

## 🎯 Philosophy

EasyVersionBackup focuses on:

* **Simplicity over complexity**
* **Reliability over features**
* **Clarity over configuration overload**

It does one thing — **versioned backups** — and does it right.

---

## 📦 Installation

Build and run — no installer required.

---

## 📄 License

(c) Daniel Capilla
Version: 0.0.7
