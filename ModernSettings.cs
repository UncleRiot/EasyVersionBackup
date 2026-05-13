// Design-Rule / UI consistency:
// Keep layout, spacing, colors, sizes, and fonts aligned with ModernTheme.
// Add new shared visual values to ModernTheme instead of hardcoding local exceptions here.
// 03.05.2026 /dc

using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;

namespace EasyVersionBackup
{
    public class ModernSettings : Form
    {
        private const int WmNclButtonDown = 0xA1;
        private const int HtCaption = 0x2;

        private readonly ToolTip settingsToolTip = new ToolTip();
        private PictureBox? activeHintOwner;

        private readonly Panel panelTitleBar;
        private readonly PictureBox pictureBoxTitleIcon;
        private readonly Label labelTitle;
        private readonly Panel panelTabs;
        private readonly Panel panelContent;
        private readonly Button buttonOk;
        private readonly Button buttonCancel;

        private readonly ComboBox comboBoxDefaultVersioning;
        private readonly ComboBox comboBoxBackupDestinationConflictHandling;
        private readonly ComboBox comboBoxLogLevel;
        private readonly CheckBox checkBoxAutoIncrementVersion;
        private readonly CheckBox checkBoxMinimizeToSystray;
        private readonly CheckBox checkBoxAutoUpdateCheck;
        private readonly CheckBox checkBoxStartWithWindows;
        private readonly CheckBox checkBoxIgnoreCopyErrors;
        private readonly CheckBox checkBoxAutoPurgeEnabled;
        private readonly CheckBox checkBoxAutoBackupEnabled;
        private readonly TextBox textBoxAutoBackupInterval;
        private readonly TextBox textBoxTag;
        private readonly Panel panelTagsList;
        private readonly ModernTheme.ModernScrollBar scrollBarTags;
        private readonly Button buttonAddTag;
        private readonly Button buttonRemoveTag;
        private readonly List<string> tags = new List<string>();
        private int selectedTagIndex = -1;
        private int tagListFirstVisibleIndex;
        private bool _isApplyingSettingsToUi;

        private readonly List<Button> tabButtons = new List<Button>();
        private readonly List<Panel> tabPages = new List<Panel>();

        public AppSettings ResultSettings { get; private set; }

        public ModernSettings(AppSettings settings)
        {
            ResultSettings = CloneSettings(settings);

            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            Text = "Settings";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(720, 480);
            MinimumSize = new Size(600, 360);
            FormBorderStyle = FormBorderStyle.None;
            BackColor = ModernTheme.WindowBackColor;
            Font = new Font(ModernTheme.FontFamilyName, ModernTheme.DefaultFontSize);
            DoubleBuffered = true;

            ModernWindowFrame.Apply(this);

            panelTitleBar = new Panel
            {
                Name = "panelTitleBar",
                Height = ModernTheme.TitleBarHeight,
                Dock = DockStyle.Top,
                BackColor = ModernTheme.TitleBarBackColor,
                Cursor = Cursors.SizeAll
            };

            pictureBoxTitleIcon = new PictureBox
            {
                Name = "pictureBoxTitleIcon",
                Location = new Point(ModernTheme.TitleBarIconLeft, ModernTheme.TitleBarIconTop),
                Size = new Size(ModernTheme.TitleBarIconSize, ModernTheme.TitleBarIconSize),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = Icon?.ToBitmap(),
                BackColor = Color.Transparent
            };

            labelTitle = new Label
            {
                Name = "labelTitle",
                Text = "Settings",
                AutoSize = false,
                Location = new Point(ModernTheme.TitleBarTextLeft, 0),
                Size = new Size(ClientSize.Width - 66, ModernTheme.TitleBarHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = ModernTheme.TextColor,
                Font = new Font(ModernTheme.FontFamilyName, ModernTheme.TitleFontSize, FontStyle.Bold),
                BackColor = Color.Transparent,
                Cursor = Cursors.SizeAll
            };

            panelTabs = new Panel
            {
                Name = "panelTabs",
                Location = new Point(12, ModernTheme.TitleBarHeight + 12),
                Size = new Size(ClientSize.Width - 24, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = ModernTheme.WindowBackColor
            };

            panelContent = new Panel
            {
                Name = "panelContent",
                Location = new Point(12, panelTabs.Bottom),
                Size = new Size(ClientSize.Width - 24, ClientSize.Height - panelTabs.Bottom - 67),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = ModernTheme.WindowBackColor,
                BorderStyle = BorderStyle.FixedSingle
            };

            comboBoxDefaultVersioning = new ComboBox
            {
                Name = "comboBoxDefaultVersioning",
                Location = new Point(ModernTheme.SettingsControlLeft, ModernTheme.SettingsRowTop(0)),
                Size = new Size(180, ModernTheme.SettingsControlHeight),
                DropDownStyle = ComboBoxStyle.DropDown,
                FlatStyle = FlatStyle.Flat,
                BackColor = ModernTheme.TitleBarBackColor,
                ForeColor = ModernTheme.TextColor
            };

            checkBoxAutoIncrementVersion = CreateCheckBox("checkBoxAutoIncrementVersion", 1);
            checkBoxMinimizeToSystray = CreateCheckBox("checkBoxMinimizeToSystray", 2);
            checkBoxAutoUpdateCheck = CreateCheckBox("checkBoxAutoUpdateCheck", 3);
            checkBoxStartWithWindows = CreateCheckBox("checkBoxStartWithWindows", 4);
            checkBoxIgnoreCopyErrors = CreateCheckBox("checkBoxIgnoreCopyErrors", 0);
            checkBoxAutoBackupEnabled = CreateCheckBox("checkBoxAutoBackupEnabled", 1);
            checkBoxAutoPurgeEnabled = CreateCheckBox("checkBoxAutoPurgeEnabled", 5);

            textBoxAutoBackupInterval = new TextBox
            {
                Name = "textBoxAutoBackupInterval",
                Location = new Point(ModernTheme.SettingsInputLeft, ModernTheme.SettingsRowTop(1)),
                Size = new Size(70, ModernTheme.SettingsControlHeight),
                BackColor = ModernTheme.TitleBarBackColor,
                ForeColor = ModernTheme.TextColor,
                BorderStyle = BorderStyle.FixedSingle
            };

            comboBoxBackupDestinationConflictHandling = new ComboBox
            {
                Name = "comboBoxBackupDestinationConflictHandling",
                Location = new Point(ModernTheme.SettingsControlLeft, ModernTheme.SettingsRowTop(2)),
                Size = new Size(textBoxAutoBackupInterval.Right - ModernTheme.SettingsControlLeft, ModernTheme.SettingsControlHeight),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = ModernTheme.TitleBarBackColor,
                ForeColor = ModernTheme.TextColor
            };

            comboBoxLogLevel = new ComboBox
            {
                Name = "comboBoxLogLevel",
                Location = new Point(ModernTheme.SettingsControlLeft, ModernTheme.SettingsRowTop(0)),
                Size = new Size(180, ModernTheme.SettingsControlHeight),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = ModernTheme.TitleBarBackColor,
                ForeColor = ModernTheme.TextColor
            };

            buttonAddTag = CreateSmallToolButton("buttonAddTag", "+", new Point(ModernTheme.SettingsLabelLeft, ModernTheme.SettingsRowTop(0)));
            buttonRemoveTag = CreateSmallToolButton("buttonRemoveTag", "-", new Point(buttonAddTag.Right + ModernTheme.ToolbarButtonSpacing, buttonAddTag.Top));

            textBoxTag = new TextBox
            {
                Name = "textBoxTag",
                Location = new Point(buttonRemoveTag.Right + ModernTheme.ToolbarButtonSpacing, buttonAddTag.Top + 3),
                Size = new Size(260, ModernTheme.SettingsControlHeight),
                BackColor = ModernTheme.TitleBarBackColor,
                ForeColor = ModernTheme.TextColor,
                BorderStyle = BorderStyle.FixedSingle
            };

            panelTagsList = new Panel
            {
                Name = "panelTagsList",
                Location = new Point(ModernTheme.SettingsLabelLeft, textBoxTag.Bottom + 10),
                Size = new Size(360, 170),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BackColor = ModernTheme.TitleBarBackColor,
                BorderStyle = BorderStyle.FixedSingle
            };

            scrollBarTags = new ModernTheme.ModernScrollBar
            {
                Name = "scrollBarTags",
                Orientation = Orientation.Vertical,
                Location = new Point(panelTagsList.Right, panelTagsList.Top),
                Size = new Size(ModernTheme.DataGridViewScrollBarSize, panelTagsList.Height),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            Label labelDefaultVersioning = CreateLabel("labelDefaultVersioning", "Default Versioning", new Point(ModernTheme.SettingsLabelLeft, ModernTheme.SettingsLabelTop(0)), new Size(ModernTheme.SettingsLabelWidth, 20));
            Label labelAutoIncrementVersion = CreateLabel("labelAutoIncrementVersion", "Auto increment", new Point(ModernTheme.SettingsLabelLeft, ModernTheme.SettingsLabelTop(1)), new Size(ModernTheme.SettingsLabelWidth, 20));
            Label labelMinimizeToSystray = CreateLabel("labelMinimizeToSystray", "Minimize to Systray", new Point(ModernTheme.SettingsLabelLeft, ModernTheme.SettingsLabelTop(2)), new Size(ModernTheme.SettingsLabelWidth, 20));
            Label labelAutoUpdateCheck = CreateLabel("labelAutoUpdateCheck", "Auto Update-Check", new Point(ModernTheme.SettingsLabelLeft, ModernTheme.SettingsLabelTop(3)), new Size(ModernTheme.SettingsLabelWidth, 20));
            Label labelStartWithWindows = CreateLabel("labelStartWithWindows", "Start with Windows", new Point(ModernTheme.SettingsLabelLeft, ModernTheme.SettingsLabelTop(4)), new Size(ModernTheme.SettingsLabelWidth, 20));
            Label labelIgnoreCopyErrors = CreateLabel("labelIgnoreCopyErrors", "Ignore Copy-Errors", new Point(ModernTheme.SettingsLabelLeft, ModernTheme.SettingsLabelTop(0)), new Size(ModernTheme.SettingsLabelWidth, 20));
            Label labelAutoBackupEnabled = CreateLabel("labelAutoBackupEnabled", "Backup Timer", new Point(ModernTheme.SettingsLabelLeft, ModernTheme.SettingsLabelTop(1)), new Size(ModernTheme.SettingsLabelWidth, 20));
            Label labelBackupDestinationConflictHandling = CreateLabel("labelBackupDestinationConflictHandling", "Destination Conflict", new Point(ModernTheme.SettingsLabelLeft, ModernTheme.SettingsLabelTop(2)), new Size(ModernTheme.SettingsLabelWidth, 20));
            Label labelLogLevel = CreateLabel("labelLogLevel", "Log level", new Point(ModernTheme.SettingsLabelLeft, ModernTheme.SettingsLabelTop(0)), new Size(ModernTheme.SettingsLabelWidth, 20));

            Label labelAutoPurgeEnabled = CreateLabel("labelAutoPurgeEnabled", "Retention", new Point(ModernTheme.SettingsLabelLeft, ModernTheme.SettingsLabelTop(5)), new Size(ModernTheme.SettingsLabelWidth, 20));
            labelAutoPurgeEnabled.ForeColor = ModernTheme.BackupInfoErrorColor;

            PictureBox pictureBoxDefaultVersioningHint = CreateSettingsHintIcon(
                "pictureBoxDefaultVersioningHint",
                "Default Versioning examples:" + Environment.NewLine +
                "none = no version suffix" + Environment.NewLine +
                "v1.0 = prefix + version number" + Environment.NewLine +
                "version1.0 = custom prefix + version number" + Environment.NewLine +
                "1.0 = version number without prefix" + Environment.NewLine +
                "yyyy-MM-dd = date, e.g. 2026-05-01" + Environment.NewLine +
                "yyyyMMdd = date, e.g. 20260501" + Environment.NewLine +
                "yyyy-MM-dd-HH-mm = date and time" + Environment.NewLine +
                "yyyyMMddHHmm = compact date and time" + Environment.NewLine +
                "... and much more, just experiment ;)");

            pictureBoxDefaultVersioningHint.Left = comboBoxDefaultVersioning.Right + ModernTheme.SettingsHintSpacing;
            pictureBoxDefaultVersioningHint.Top = comboBoxDefaultVersioning.Top + (comboBoxDefaultVersioning.Height - pictureBoxDefaultVersioningHint.Height) / 2;

            PictureBox pictureBoxAutoBackupTimerHint = CreateSettingsHintIcon(
                "pictureBoxAutoBackupTimerHint",
                "Backup Timer examples:" + Environment.NewLine +
                "30s = 30 seconds" + Environment.NewLine +
                "5m = 5 minutes" + Environment.NewLine +
                "1h = 1 hour" + Environment.NewLine +
                "15 = 15 minutes");

            pictureBoxAutoBackupTimerHint.Left = textBoxAutoBackupInterval.Right + ModernTheme.SettingsHintSpacing;
            pictureBoxAutoBackupTimerHint.Top = textBoxAutoBackupInterval.Top + (textBoxAutoBackupInterval.Height - pictureBoxAutoBackupTimerHint.Height) / 2;

            PictureBox pictureBoxAutoPurgeHint = CreateSettingsHintIcon(
                "pictureBoxAutoPurgeHint",
                "Experimental feature!" + Environment.NewLine +
                "Retention / Auto-Purge can permanently delete old ZIP backups after a successful backup." + Environment.NewLine +
                "Use at your own risk." + Environment.NewLine +
                "No warranty, no guarantee, no liability." + Environment.NewLine +
                "Only enable this if you understand the risk of data loss.");

            pictureBoxAutoPurgeHint.Left = checkBoxAutoPurgeEnabled.Right + ModernTheme.SettingsHintSpacing;
            pictureBoxAutoPurgeHint.Top = checkBoxAutoPurgeEnabled.Top + (checkBoxAutoPurgeEnabled.Height - pictureBoxAutoPurgeHint.Height) / 2;

            Button buttonExportSettings = CreateToolButton("buttonExportSettings", "Export Settings", new Point(ModernTheme.SettingsLabelLeft, ModernTheme.SettingsRowTop(0)));
            Button buttonImportSettings = CreateToolButton("buttonImportSettings", "Import Settings", new Point(buttonExportSettings.Right + ModernTheme.ToolbarButtonSpacing, buttonExportSettings.Top));

            buttonExportSettings.Click += buttonExportSettings_Click;
            buttonImportSettings.Click += buttonImportSettings_Click;

            settingsToolTip.SetToolTip(comboBoxDefaultVersioning, "Default version or date pattern for new backups");
            settingsToolTip.SetToolTip(checkBoxAutoIncrementVersion, "Automatically increment the suggested version");
            settingsToolTip.SetToolTip(checkBoxMinimizeToSystray, "Hide the window in the system tray when minimized");
            settingsToolTip.SetToolTip(checkBoxAutoUpdateCheck, "Automatically check GitHub for new versions");
            settingsToolTip.SetToolTip(checkBoxStartWithWindows, "Start EasyVersionBackup when Windows starts");
            settingsToolTip.SetToolTip(checkBoxIgnoreCopyErrors, "Skip files that cannot be copied");
            settingsToolTip.SetToolTip(checkBoxAutoBackupEnabled, "Run backups automatically / timer based");
            settingsToolTip.SetToolTip(textBoxAutoBackupInterval, "Time between automatic backups");
            settingsToolTip.SetToolTip(comboBoxBackupDestinationConflictHandling, "Default action when the backup destination already exists");
            settingsToolTip.SetToolTip(comboBoxLogLevel, "Minimal = smallest log, Normal = useful decisions, Verbose = every file entry");
            settingsToolTip.SetToolTip(checkBoxAutoPurgeEnabled, "Experimental feature. Permanently deletes old ZIP backups. Use at your own risk.");
            settingsToolTip.SetToolTip(buttonExportSettings, "Export current settings to " + ToolsHelper.SettingsFileName);
            settingsToolTip.SetToolTip(buttonImportSettings, "Import settings from " + ToolsHelper.SettingsFileName);
            settingsToolTip.SetToolTip(textBoxTag, "New backup tag");
            settingsToolTip.SetToolTip(buttonAddTag, "Add tag");
            settingsToolTip.SetToolTip(buttonRemoveTag, "Remove selected tag");

            buttonAddTag.Click += buttonAddTag_Click;
            buttonRemoveTag.Click += buttonRemoveTag_Click;
            panelTagsList.Paint += panelTagsList_Paint;
            panelTagsList.MouseDown += panelTagsList_MouseDown;
            panelTagsList.MouseWheel += panelTagsList_MouseWheel;
            panelTagsList.Resize += panelTagsList_Resize;
            scrollBarTags.ScrollValueChanged += scrollBarTags_ScrollValueChanged;

            checkBoxAutoBackupEnabled.CheckedChanged += checkBoxAutoBackupEnabled_CheckedChanged;
            checkBoxAutoPurgeEnabled.CheckedChanged += checkBoxAutoPurgeEnabled_CheckedChanged;

            Panel tabPageGeneral = CreateTabPage("tabPageGeneral");
            tabPageGeneral.Controls.Add(labelDefaultVersioning);
            tabPageGeneral.Controls.Add(comboBoxDefaultVersioning);
            tabPageGeneral.Controls.Add(pictureBoxDefaultVersioningHint);
            tabPageGeneral.Controls.Add(labelAutoIncrementVersion);
            tabPageGeneral.Controls.Add(checkBoxAutoIncrementVersion);
            tabPageGeneral.Controls.Add(labelMinimizeToSystray);
            tabPageGeneral.Controls.Add(checkBoxMinimizeToSystray);
            tabPageGeneral.Controls.Add(labelAutoUpdateCheck);
            tabPageGeneral.Controls.Add(checkBoxAutoUpdateCheck);
            tabPageGeneral.Controls.Add(labelStartWithWindows);
            tabPageGeneral.Controls.Add(checkBoxStartWithWindows);
            tabPageGeneral.Controls.Add(labelAutoPurgeEnabled);
            tabPageGeneral.Controls.Add(checkBoxAutoPurgeEnabled);
            tabPageGeneral.Controls.Add(pictureBoxAutoPurgeHint);

            Panel tabPageBackup = CreateTabPage("tabPageBackup");
            tabPageBackup.Controls.Add(labelIgnoreCopyErrors);
            tabPageBackup.Controls.Add(checkBoxIgnoreCopyErrors);
            tabPageBackup.Controls.Add(labelAutoBackupEnabled);
            tabPageBackup.Controls.Add(checkBoxAutoBackupEnabled);
            tabPageBackup.Controls.Add(textBoxAutoBackupInterval);
            tabPageBackup.Controls.Add(pictureBoxAutoBackupTimerHint);
            tabPageBackup.Controls.Add(labelBackupDestinationConflictHandling);
            tabPageBackup.Controls.Add(comboBoxBackupDestinationConflictHandling);

            Panel tabPageTags = CreateTabPage("tabPageTags");
            tabPageTags.Controls.Add(buttonAddTag);
            tabPageTags.Controls.Add(buttonRemoveTag);
            tabPageTags.Controls.Add(textBoxTag);
            tabPageTags.Controls.Add(panelTagsList);
            tabPageTags.Controls.Add(scrollBarTags);

            Panel tabPageLog = CreateTabPage("tabPageLog");
            tabPageLog.Controls.Add(labelLogLevel);
            tabPageLog.Controls.Add(comboBoxLogLevel);

            Panel tabPageTools = CreateTabPage("tabPageTools");
            tabPageTools.Controls.Add(buttonExportSettings);
            tabPageTools.Controls.Add(buttonImportSettings);

            ApplyDynamicSettingsLayout(tabPageGeneral);
            ApplyDynamicSettingsLayout(tabPageBackup);
            ApplyDynamicSettingsLayout(tabPageLog);

            AddSettingsTab("General", tabPageGeneral);
            AddSettingsTab("Backup", tabPageBackup);
            AddSettingsTab("Tags", tabPageTags);
            AddSettingsTab("Log", tabPageLog);
            AddSettingsTab("Tools", tabPageTools);

            buttonOk = ModernTheme.CreateDialogPrimaryButton("buttonOk", "OK");
            buttonCancel = ModernTheme.CreateDialogSecondaryButton("buttonCancel", "Cancel", DialogResult.Cancel);
            ModernTheme.PositionDialogButtons(this, buttonOk, buttonCancel);

            buttonOk.Click += buttonOk_Click;

            panelTitleBar.MouseDown += titleBar_MouseDown;
            labelTitle.MouseDown += titleBar_MouseDown;
            pictureBoxTitleIcon.MouseDown += titleBar_MouseDown;
            MouseDown += ModernSettings_MouseDown;

            panelTitleBar.Controls.Add(pictureBoxTitleIcon);
            panelTitleBar.Controls.Add(labelTitle);

            Controls.Add(panelTitleBar);
            Controls.Add(panelTabs);
            Controls.Add(panelContent);
            Controls.Add(buttonOk);
            Controls.Add(buttonCancel);

            ApplySettingsToUi(ResultSettings);
            SelectTab(0);

            AcceptButton = buttonOk;
            CancelButton = buttonCancel;
        }

        private void ApplyDynamicSettingsLayout(Panel tabPage)
        {
            int controlLeft = GetDynamicSettingsControlLeft(tabPage);
            int inputLeft = controlLeft + (ModernTheme.SettingsInputLeft - ModernTheme.SettingsControlLeft);

            foreach (Control control in tabPage.Controls)
            {
                if (control is Label label && label.Name.StartsWith("label", StringComparison.OrdinalIgnoreCase))
                {
                    Size textSize = TextRenderer.MeasureText(label.Text, label.Font);
                    label.Width = textSize.Width + 8;
                }
            }

            foreach (Control control in tabPage.Controls)
            {
                if (control is CheckBox || control is ComboBox)
                {
                    control.Left = controlLeft;
                }
                else if (control is TextBox)
                {
                    control.Left = inputLeft;
                }
            }

            RepositionSettingsHint(tabPage, "pictureBoxDefaultVersioningHint", comboBoxDefaultVersioning);
            RepositionSettingsHint(tabPage, "pictureBoxAutoBackupTimerHint", textBoxAutoBackupInterval);
            RepositionSettingsHint(tabPage, "pictureBoxAutoPurgeHint", checkBoxAutoPurgeEnabled);
        }

        private int GetDynamicSettingsControlLeft(Panel tabPage)
        {
            int maxLabelRight = ModernTheme.SettingsLabelLeft + ModernTheme.SettingsLabelWidth;

            foreach (Control control in tabPage.Controls)
            {
                if (control is Label label && label.Name.StartsWith("label", StringComparison.OrdinalIgnoreCase))
                {
                    Size textSize = TextRenderer.MeasureText(label.Text, label.Font);
                    maxLabelRight = Math.Max(maxLabelRight, label.Left + textSize.Width + 8);
                }
            }

            return maxLabelRight + ModernTheme.SettingsHintSpacing + 18;
        }

        private void RepositionSettingsHint(Panel tabPage, string hintName, Control ownerControl)
        {
            if (tabPage.Controls[hintName] is not PictureBox hintIcon)
            {
                return;
            }

            hintIcon.Left = ownerControl.Right + ModernTheme.SettingsHintSpacing;
            hintIcon.Top = ownerControl.Top + (ownerControl.Height - hintIcon.Height) / 2;
        }

        private Button CreateToolButton(string name, string text, Point location)
        {
            Button button = new Button
            {
                Name = name,
                Text = text,
                Location = location,
                Size = new Size(ModernTheme.SettingsComboBoxWidth, ModernTheme.DialogButtonSize.Height),
                FlatStyle = FlatStyle.Flat,
                BackColor = ModernTheme.ControlBackColor,
                ForeColor = ModernTheme.TextColor,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = ModernTheme.DefaultButtonTextPadding,
                UseVisualStyleBackColor = false
            };

            button.FlatAppearance.BorderColor = ModernTheme.AccentColor;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = ModernTheme.ControlHoverBackColor;
            button.FlatAppearance.MouseDownBackColor = ModernTheme.AccentColor;

            return button;
        }


        private Button CreateSmallToolButton(string name, string text, Point location)
        {
            Button button = new Button
            {
                Name = name,
                Text = text,
                Location = location,
                Size = ModernTheme.ToolbarButtonSize,
                FlatStyle = FlatStyle.Flat,
                BackColor = ModernTheme.ControlBackColor,
                ForeColor = ModernTheme.TextColor,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = text == "+"
                    ? ModernTheme.ToolbarPlusButtonTextPadding
                    : ModernTheme.ToolbarMinusButtonTextPadding,
                UseVisualStyleBackColor = false
            };

            button.FlatAppearance.BorderColor = ModernTheme.AccentColor;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = ModernTheme.ControlHoverBackColor;
            button.FlatAppearance.MouseDownBackColor = ModernTheme.AccentColor;

            return button;
        }

        private void buttonExportSettings_Click(object? sender, EventArgs e)
        {
            if (!TryBuildSettingsFromUi(out AppSettings settings))
            {
                return;
            }

            if (!ModernFolderBrowserDialog.ShowFile(this, "Export Settings", string.Empty, ToolsHelper.SettingsFileName, out string exportFilePath))
            {
                return;
            }

            try
            {
                SettingsStorage.ExportToFile(settings, exportFilePath);

                ModernMessageDialog.Show(
                    this,
                    "Export Settings",
                    "Settings exported successfully:" + Environment.NewLine +
                    exportFilePath);
            }
            catch (Exception exception)
            {
                ModernMessageDialog.Show(
                    this,
                    "Export Settings",
                    "Settings could not be exported:" + Environment.NewLine +
                    exportFilePath + Environment.NewLine + Environment.NewLine +
                    exception.Message);
            }
        }

        private void buttonImportSettings_Click(object? sender, EventArgs e)
        {
            DialogResult confirmationResult = ModernConfirmationDialog.Show(
                this,
                "Import Settings",
                "Importing settings will replace the current settings shown in this dialog." + Environment.NewLine + Environment.NewLine +
                "Do you want to continue?");

            if (confirmationResult != DialogResult.Yes)
            {
                return;
            }

            if (!ModernFolderBrowserDialog.ShowFile(this, "Import Settings", string.Empty, ToolsHelper.SettingsFileName, out string importFilePath))
            {
                return;
            }

            if (!SettingsStorage.TryImportFromFile(importFilePath, out AppSettings importedSettings, out string errorMessage))
            {
                ModernMessageDialog.Show(
                    this,
                    "Import Settings",
                    "Settings could not be imported:" + Environment.NewLine +
                    importFilePath + Environment.NewLine + Environment.NewLine +
                    errorMessage);

                return;
            }

            ResultSettings = CloneSettings(importedSettings);
            ApplySettingsToUi(ResultSettings);

            ModernMessageDialog.Show(
                this,
                "Import Settings",
                "Settings imported successfully." + Environment.NewLine + Environment.NewLine +
                "Click OK to apply them.");
        }

        private void AddSettingsTab(string text, Panel tabPage)
        {
            int left = tabButtons.Count == 0
                ? 0
                : tabButtons[tabButtons.Count - 1].Right + 4;

            Button tabButton = CreateTabButton("buttonTab" + text, text, new Point(left, 0));
            int tabIndex = tabButtons.Count;

            tabButton.Click += (sender, e) => SelectTab(tabIndex);

            tabButtons.Add(tabButton);
            tabPages.Add(tabPage);

            panelTabs.Controls.Add(tabButton);
            panelContent.Controls.Add(tabPage);
        }

        private Panel CreateTabPage(string name)
        {
            return new Panel
            {
                Name = name,
                Dock = DockStyle.Fill,
                BackColor = ModernTheme.WindowBackColor,
                Visible = false
            };
        }

        private Button CreateTabButton(string name, string text, Point location)
        {
            Button button = new Button
            {
                Name = name,
                Text = text,
                Location = location,
                Size = new Size(90, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = ModernTheme.ControlBackColor,
                ForeColor = ModernTheme.TextColor,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = ModernTheme.DefaultButtonTextPadding,
                UseVisualStyleBackColor = false
            };

            button.FlatAppearance.BorderColor = ModernTheme.WindowBorderColor;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = ModernTheme.ControlHoverBackColor;
            button.FlatAppearance.MouseDownBackColor = ModernTheme.AccentColor;

            return button;
        }

        private Label CreateLabel(string name, string text, Point location, Size size)
        {
            return new Label
            {
                Name = name,
                Text = text,
                Location = location,
                Size = size,
                ForeColor = ModernTheme.TextColor,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private CheckBox CreateCheckBox(string name, int rowIndex)
        {
            return ModernTheme.CreateSettingsCheckBox(
                name,
                new Point(
                    ModernTheme.SettingsControlLeft,
                    ModernTheme.SettingsRowTop(rowIndex)));
        }

        private PictureBox CreateSettingsHintIcon(string name, string hintText)
        {
            PictureBox pictureBox = new PictureBox
            {
                Name = name,
                Size = new Size(18, 18),
                Image = CreateSettingsHintIconBitmap(),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Cursor = Cursors.Help,
                BackColor = Color.Transparent
            };

            settingsToolTip.SetToolTip(pictureBox, hintText);

            pictureBox.Click += (sender, e) =>
            {
                settingsToolTip.Hide(pictureBox);

                activeHintOwner = pictureBox;
                settingsToolTip.Show(hintText, pictureBox, pictureBox.Width + 5, 0, 8000);
            };

            return pictureBox;
        }

        private Bitmap CreateSettingsHintIconBitmap()
        {
            Bitmap bitmap = new Bitmap(18, 18);

            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Transparent);

            using SolidBrush brush = new SolidBrush(ModernTheme.AccentColor);
            graphics.FillEllipse(brush, 1, 1, 16, 16);

            using Font font = new Font(ModernTheme.FontFamilyName, 8F, FontStyle.Regular);
            Size textSize = TextRenderer.MeasureText("?", font);

            int x = (18 - textSize.Width) / 2 + 5;
            int y = (18 - textSize.Height) / 2;

            TextRenderer.DrawText(
                graphics,
                "?",
                font,
                new Point(x, y),
                ModernTheme.DarkTextColor,
                TextFormatFlags.NoPadding);

            return bitmap;
        }

        private void SelectTab(int selectedIndex)
        {
            for (int index = 0; index < tabButtons.Count; index++)
            {
                bool isSelected = index == selectedIndex;

                tabButtons[index].BackColor = isSelected
                    ? ModernTheme.AccentColor
                    : ModernTheme.ControlBackColor;

                tabButtons[index].ForeColor = isSelected
                    ? ModernTheme.DarkTextColor
                    : ModernTheme.TextColor;

                tabPages[index].Visible = isSelected;
            }
        }

        private void ApplySettingsToUi(AppSettings settings)
        {
            _isApplyingSettingsToUi = true;

            comboBoxDefaultVersioning.Items.Clear();
            comboBoxDefaultVersioning.Items.Add("none");
            comboBoxDefaultVersioning.Items.Add("v1.0");
            comboBoxDefaultVersioning.Items.Add("1.0");
            comboBoxDefaultVersioning.Items.Add("yyyy-MM-dd");
            comboBoxDefaultVersioning.Items.Add("yyyyMMdd");
            comboBoxDefaultVersioning.Items.Add("yyyy-MM-dd-HH-mm");
            comboBoxDefaultVersioning.Items.Add("yyyyMMddHHmm");

            comboBoxDefaultVersioning.Text = string.IsNullOrWhiteSpace(settings.DefaultVersioning)
                ? "v1.0"
                : settings.DefaultVersioning;

            comboBoxBackupDestinationConflictHandling.Items.Clear();
            comboBoxBackupDestinationConflictHandling.Items.Add(BackupHelper.DestinationConflictAsk);
            comboBoxBackupDestinationConflictHandling.Items.Add(BackupHelper.DestinationConflictCancel);
            comboBoxBackupDestinationConflictHandling.Items.Add(BackupHelper.DestinationConflictOverwrite);
            comboBoxBackupDestinationConflictHandling.Items.Add(BackupHelper.DestinationConflictAppend);

            comboBoxBackupDestinationConflictHandling.Text = BackupHelper.NormalizeDestinationConflictHandling(settings.BackupDestinationConflictHandling);

            comboBoxLogLevel.Items.Clear();
            comboBoxLogLevel.Items.Add(BackupLogger.LogLevelMinimal);
            comboBoxLogLevel.Items.Add(BackupLogger.LogLevelNormal);
            comboBoxLogLevel.Items.Add(BackupLogger.LogLevelVerbose);
            comboBoxLogLevel.Text = BackupLogger.NormalizeLogLevel(settings.LogLevel);

            checkBoxAutoIncrementVersion.Checked = settings.AutoIncrementVersion;
            checkBoxMinimizeToSystray.Checked = settings.MinimizeToSystray;
            checkBoxAutoUpdateCheck.Checked = settings.AutoUpdateCheck;
            checkBoxStartWithWindows.Checked = settings.StartWithWindows;
            checkBoxIgnoreCopyErrors.Checked = settings.IgnoreCopyErrors;
            checkBoxAutoPurgeEnabled.Checked = settings.AutoPurgeEnabled;
            checkBoxAutoBackupEnabled.Checked = settings.AutoBackupEnabled;
            textBoxAutoBackupInterval.Text = FormatAutoBackupIntervalText(GetAutoBackupIntervalSeconds(settings));

            tags.Clear();

            tags.AddRange((settings.Tags ?? new List<string>())
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(tag => tag, StringComparer.OrdinalIgnoreCase));

            selectedTagIndex = -1;
            tagListFirstVisibleIndex = 0;
            textBoxTag.Clear();
            UpdateTagsListUi();

            _isApplyingSettingsToUi = false;

            UpdateAutoBackupControls();
        }

        private AppSettings ReadSettingsFromUi()
        {
            AppSettings settings = CloneSettings(ResultSettings);
            int autoBackupIntervalSeconds = ParseAutoBackupIntervalSeconds(textBoxAutoBackupInterval.Text.Trim());

            settings.DefaultVersioning = comboBoxDefaultVersioning.Text.Trim();
            settings.AutoIncrementVersion = checkBoxAutoIncrementVersion.Checked;
            settings.MinimizeToSystray = checkBoxMinimizeToSystray.Checked;
            settings.AutoUpdateCheck = checkBoxAutoUpdateCheck.Checked;
            settings.StartWithWindows = checkBoxStartWithWindows.Checked;
            settings.IgnoreCopyErrors = checkBoxIgnoreCopyErrors.Checked;
            settings.AutoPurgeEnabled = checkBoxAutoPurgeEnabled.Checked;
            settings.BackupDestinationConflictHandling = BackupHelper.NormalizeDestinationConflictHandling(comboBoxBackupDestinationConflictHandling.Text);
            settings.LogLevel = BackupLogger.NormalizeLogLevel(comboBoxLogLevel.Text);
            settings.AutoBackupEnabled = checkBoxAutoBackupEnabled.Checked;
            settings.AutoBackupIntervalSeconds = autoBackupIntervalSeconds;
            settings.AutoBackupIntervalMinutes = Math.Max(1, (int)Math.Ceiling(autoBackupIntervalSeconds / 60.0));
            settings.Tags = tags
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(tag => tag, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return settings;
        }

        private void checkBoxAutoPurgeEnabled_CheckedChanged(object? sender, EventArgs e)
        {
            if (_isApplyingSettingsToUi)
            {
                return;
            }

            if (!checkBoxAutoPurgeEnabled.Checked)
            {
                return;
            }

            if (!ShowAutoPurgeSafetyConfirmation())
            {
                checkBoxAutoPurgeEnabled.Checked = false;
            }
        }

        private bool ShowAutoPurgeSafetyConfirmation()
        {
            using AutoPurgeSafetyConfirmationDialog form = new AutoPurgeSafetyConfirmationDialog(this);

            return form.ShowDialog(this) == DialogResult.Yes;
        }

        private sealed class AutoPurgeSafetyConfirmationDialog : Form
        {
            private const int WmNclButtonDown = 0xA1;
            private const int HtCaption = 0x2;

            private readonly Panel panelTitleBar;
            private readonly PictureBox pictureBoxTitleIcon;
            private readonly Label labelTitle;
            private readonly Label labelWarningIcon;
            private readonly Label labelMessage;
            private readonly Button buttonEnable;
            private readonly Button buttonCancel;

            public AutoPurgeSafetyConfirmationDialog(Form owner)
            {
                Icon = owner.Icon;
                Text = "Experimental Retention/Autopurge";
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.None;
                ClientSize = new Size(520, 260);
                MinimumSize = new Size(520, 260);
                BackColor = ModernTheme.WindowBackColor;
                Font = new Font(ModernTheme.FontFamilyName, ModernTheme.DefaultFontSize);
                ShowInTaskbar = false;
                DoubleBuffered = true;

                ModernWindowFrame.Apply(this);

                panelTitleBar = new Panel
                {
                    Name = "panelTitleBar",
                    Height = ModernTheme.TitleBarHeight,
                    Dock = DockStyle.Top,
                    BackColor = ModernTheme.TitleBarBackColor,
                    Cursor = Cursors.SizeAll
                };

                pictureBoxTitleIcon = new PictureBox
                {
                    Name = "pictureBoxTitleIcon",
                    Location = new Point(ModernTheme.TitleBarIconLeft, ModernTheme.TitleBarIconTop),
                    Size = new Size(ModernTheme.TitleBarIconSize, ModernTheme.TitleBarIconSize),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Image = Icon?.ToBitmap(),
                    BackColor = Color.Transparent
                };

                labelTitle = new Label
                {
                    Name = "labelTitle",
                    Text = "Experimental Retention/autopurge",
                    AutoSize = false,
                    Location = new Point(ModernTheme.TitleBarTextLeft, 0),
                    Size = new Size(ClientSize.Width - ModernTheme.TitleBarTextLeft - 12, ModernTheme.TitleBarHeight),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = ModernTheme.TextColor,
                    Font = new Font(ModernTheme.FontFamilyName, ModernTheme.TitleFontSize, FontStyle.Bold),
                    BackColor = Color.Transparent,
                    Cursor = Cursors.SizeAll
                };

                labelWarningIcon = new Label
                {
                    Name = "labelWarningIcon",
                    Text = "!",
                    Location = new Point(24, 58),
                    Size = new Size(34, 34),
                    BackColor = ModernTheme.BackupInfoWarningColor,
                    ForeColor = ModernTheme.DarkTextColor,
                    Font = new Font(ModernTheme.FontFamilyName, 18F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                labelMessage = new Label
                {
                    Name = "labelMessage",
                    Text =
                        "Experimental feature!" + Environment.NewLine + Environment.NewLine +
                        "Retention/autopurge can permanently delete old ZIP backups after a successful backup." + Environment.NewLine +
                        "Deleted backups may not be recoverable." + Environment.NewLine + Environment.NewLine +
                        "Use at your own risk. No warranty, no guarantee, no liability." + Environment.NewLine +
                        "Do you really want to enable Retention/autopurge?",
                    Location = new Point(72, 54),
                    Size = new Size(ClientSize.Width - 96, 130),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    ForeColor = ModernTheme.TextColor,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.TopLeft
                };

                buttonEnable = ModernTheme.CreateDialogPrimaryButton("buttonEnable", "Enable");
                buttonEnable.DialogResult = DialogResult.Yes;

                buttonCancel = ModernTheme.CreateDialogSecondaryButton("buttonCancel", "Cancel", DialogResult.No);

                ModernTheme.PositionDialogButtons(this, buttonEnable, buttonCancel);

                panelTitleBar.MouseDown += titleBar_MouseDown;
                pictureBoxTitleIcon.MouseDown += titleBar_MouseDown;
                labelTitle.MouseDown += titleBar_MouseDown;

                panelTitleBar.Controls.Add(pictureBoxTitleIcon);
                panelTitleBar.Controls.Add(labelTitle);

                Controls.Add(panelTitleBar);
                Controls.Add(labelWarningIcon);
                Controls.Add(labelMessage);
                Controls.Add(buttonEnable);
                Controls.Add(buttonCancel);

                AcceptButton = buttonEnable;
                CancelButton = buttonCancel;
            }

            private void titleBar_MouseDown(object? sender, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left)
                {
                    return;
                }

                ReleaseCapture();
                SendMessage(Handle, WmNclButtonDown, HtCaption, 0);
            }

            [DllImport("user32.dll")]
            private static extern bool ReleaseCapture();

            [DllImport("user32.dll")]
            private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        }

        private void UpdateAutoBackupControls()
        {
            textBoxAutoBackupInterval.Enabled = checkBoxAutoBackupEnabled.Checked;
        }

        private bool TryBuildSettingsFromUi(out AppSettings settings)
        {
            settings = ResultSettings;

            if (checkBoxAutoBackupEnabled.Checked &&
                !TryParseAutoBackupIntervalSeconds(textBoxAutoBackupInterval.Text.Trim(), out int autoBackupIntervalSeconds))
            {
                ModernMessageDialog.Show(this, "Error", "Backup Timer must be valid. Examples: 30s, 5m, 1h, 15.");
                return false;
            }

            string defaultVersioning = comboBoxDefaultVersioning.Text.Trim();

            if (!string.Equals(defaultVersioning, "none", StringComparison.OrdinalIgnoreCase) &&
                !VersionPatternHelper.IsDatePattern(defaultVersioning) &&
                !VersionPatternHelper.IsValidVersionValue(defaultVersioning))
            {
                ModernMessageDialog.Show(this, "Error", "Default Versioning contains invalid filename characters.");
                return false;
            }

            foreach (string tag in tags)
            {
                if (string.IsNullOrWhiteSpace(tag) || !VersionPatternHelper.IsValidVersionValue(tag))
                {
                    ModernMessageDialog.Show(this, "Error", "Tags must be usable as part of a file name.");
                    return false;
                }
            }

            settings = ReadSettingsFromUi();
            return true;
        }

        private void buttonAddTag_Click(object? sender, EventArgs e)
        {
            string tag = textBoxTag.Text.Trim();

            if (string.IsNullOrWhiteSpace(tag))
            {
                return;
            }

            if (!VersionPatternHelper.IsValidVersionValue(tag))
            {
                ModernMessageDialog.Show(this, "Error", "Tag must be usable as part of a file name.");
                return;
            }

            foreach (string existingTag in tags)
            {
                if (string.Equals(existingTag, tag, StringComparison.OrdinalIgnoreCase))
                {
                    textBoxTag.Clear();
                    return;
                }
            }

            tags.Add(tag);
            tags.Sort(StringComparer.OrdinalIgnoreCase);
            selectedTagIndex = tags.FindIndex(existingTag => string.Equals(existingTag, tag, StringComparison.OrdinalIgnoreCase));
            EnsureSelectedTagVisible();
            textBoxTag.Clear();
            UpdateTagsListUi();
        }

        private void buttonRemoveTag_Click(object? sender, EventArgs e)
        {
            if (selectedTagIndex < 0 || selectedTagIndex >= tags.Count)
            {
                return;
            }

            tags.RemoveAt(selectedTagIndex);

            if (selectedTagIndex >= tags.Count)
            {
                selectedTagIndex = tags.Count - 1;
            }

            if (tags.Count == 0)
            {
                selectedTagIndex = -1;
                tagListFirstVisibleIndex = 0;
            }

            EnsureSelectedTagVisible();
            UpdateTagsListUi();
        }

        private void panelTagsList_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.Clear(ModernTheme.TitleBarBackColor);

            using Pen borderPen = new Pen(ModernTheme.WindowBorderColor);
            e.Graphics.DrawRectangle(borderPen, 0, 0, panelTagsList.Width - 1, panelTagsList.Height - 1);

            int visibleCount = GetVisibleTagCount();
            int rowLeft = 1;
            int rowWidth = Math.Max(0, panelTagsList.ClientSize.Width - 2);
            int rowTop = 1;

            for (int visibleIndex = 0; visibleIndex < visibleCount; visibleIndex++)
            {
                int tagIndex = tagListFirstVisibleIndex + visibleIndex;

                if (tagIndex >= tags.Count)
                {
                    break;
                }

                Rectangle rowBounds = new Rectangle(rowLeft, rowTop + visibleIndex * ModernTheme.SettingsControlHeight, rowWidth, ModernTheme.SettingsControlHeight);
                bool isSelected = tagIndex == selectedTagIndex;

                using SolidBrush rowBrush = new SolidBrush(isSelected ? ModernTheme.AccentColor : ModernTheme.TitleBarBackColor);
                e.Graphics.FillRectangle(rowBrush, rowBounds);

                TextRenderer.DrawText(
                    e.Graphics,
                    tags[tagIndex],
                    Font,
                    new Rectangle(rowBounds.Left + 4, rowBounds.Top, rowBounds.Width - 8, rowBounds.Height),
                    isSelected ? ModernTheme.DarkTextColor : ModernTheme.TextColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }

        private void panelTagsList_MouseDown(object? sender, MouseEventArgs e)
        {
            panelTagsList.Focus();

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            int clickedIndex = tagListFirstVisibleIndex + Math.Max(0, (e.Y - 1) / ModernTheme.SettingsControlHeight);

            if (clickedIndex < 0 || clickedIndex >= tags.Count)
            {
                return;
            }

            selectedTagIndex = clickedIndex;
            UpdateTagsListUi();
        }

        private void panelTagsList_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (!scrollBarTags.Visible)
            {
                return;
            }

            scrollBarTags.Value += e.Delta > 0 ? -1 : 1;
        }

        private void panelTagsList_Resize(object? sender, EventArgs e)
        {
            UpdateTagsListUi();
        }

        private void scrollBarTags_ScrollValueChanged(object? sender, EventArgs e)
        {
            tagListFirstVisibleIndex = scrollBarTags.Value;
            panelTagsList.Invalidate();
        }

        private void UpdateTagsListUi()
        {
            int visibleCount = GetVisibleTagCount();
            int maximumFirstVisibleIndex = Math.Max(0, tags.Count - visibleCount);

            if (tagListFirstVisibleIndex > maximumFirstVisibleIndex)
            {
                tagListFirstVisibleIndex = maximumFirstVisibleIndex;
            }

            if (tagListFirstVisibleIndex < 0)
            {
                tagListFirstVisibleIndex = 0;
            }

            scrollBarTags.Minimum = 0;
            scrollBarTags.Maximum = maximumFirstVisibleIndex;
            scrollBarTags.LargeChange = Math.Max(1, visibleCount);
            scrollBarTags.Visible = maximumFirstVisibleIndex > 0;

            if (scrollBarTags.Value != tagListFirstVisibleIndex)
            {
                scrollBarTags.Value = tagListFirstVisibleIndex;
            }
            else
            {
                scrollBarTags.Invalidate();
            }

            panelTagsList.Invalidate();
        }

        private int GetVisibleTagCount()
        {
            return Math.Max(1, (panelTagsList.ClientSize.Height - 2) / ModernTheme.SettingsControlHeight);
        }

        private void EnsureSelectedTagVisible()
        {
            if (selectedTagIndex < 0)
            {
                return;
            }

            int visibleCount = GetVisibleTagCount();

            if (selectedTagIndex < tagListFirstVisibleIndex)
            {
                tagListFirstVisibleIndex = selectedTagIndex;
            }
            else if (selectedTagIndex >= tagListFirstVisibleIndex + visibleCount)
            {
                tagListFirstVisibleIndex = selectedTagIndex - visibleCount + 1;
            }
        }

        private void buttonOk_Click(object? sender, EventArgs e)
        {
            if (!TryBuildSettingsFromUi(out AppSettings settings))
            {
                return;
            }

            if (!TryApplyStartWithWindowsSetting(settings.StartWithWindows, out string errorMessage))
            {
                ModernMessageDialog.Show(
                    this,
                    "Error",
                    "Start with Windows could not be updated:" + Environment.NewLine + Environment.NewLine +
                    errorMessage);

                return;
            }

            ResultSettings = settings;
            DialogResult = DialogResult.OK;
            Close();
        }

        private bool TryApplyStartWithWindowsSetting(bool startWithWindows, out string errorMessage)
        {
            const string runKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            const string applicationName = "EasyVersionBackup";

            errorMessage = string.Empty;

            try
            {
                using Microsoft.Win32.RegistryKey? registryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(runKeyPath);

                if (registryKey == null)
                {
                    errorMessage = "Windows startup registry key could not be opened.";
                    return false;
                }

                if (startWithWindows)
                {
                    registryKey.SetValue(applicationName, "\"" + Application.ExecutablePath + "\" --start-minimized");
                }
                else
                {
                    registryKey.DeleteValue(applicationName, false);
                }

                return true;
            }
            catch (Exception exception)
            {
                errorMessage = exception.Message;
                return false;
            }
        }

        private void checkBoxAutoBackupEnabled_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateAutoBackupControls();
        }

        private int GetAutoBackupIntervalSeconds(AppSettings settings)
        {
            if (settings.AutoBackupIntervalSeconds >= 1)
            {
                return settings.AutoBackupIntervalSeconds;
            }

            return Math.Max(1, settings.AutoBackupIntervalMinutes) * 60;
        }

        private string FormatAutoBackupIntervalText(int seconds)
        {
            if (seconds % 3600 == 0)
            {
                return (seconds / 3600).ToString() + "h";
            }

            if (seconds % 60 == 0)
            {
                return (seconds / 60).ToString() + "m";
            }

            return seconds.ToString() + "s";
        }

        private int ParseAutoBackupIntervalSeconds(string value)
        {
            if (!TryParseAutoBackupIntervalSeconds(value, out int seconds))
            {
                return 900;
            }

            return seconds;
        }

        private bool TryParseAutoBackupIntervalSeconds(string value, out int seconds)
        {
            seconds = 0;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string normalizedValue = value.Trim().ToLowerInvariant();

            if (normalizedValue.EndsWith("s"))
            {
                return int.TryParse(normalizedValue[..^1], out seconds) && seconds >= 1;
            }

            if (normalizedValue.EndsWith("m"))
            {
                if (!int.TryParse(normalizedValue[..^1], out int minutes) || minutes < 1)
                {
                    return false;
                }

                seconds = minutes * 60;
                return true;
            }

            if (normalizedValue.EndsWith("h"))
            {
                if (!int.TryParse(normalizedValue[..^1], out int hours) || hours < 1)
                {
                    return false;
                }

                seconds = hours * 3600;
                return true;
            }

            if (!int.TryParse(normalizedValue, out int defaultMinutes) || defaultMinutes < 1)
            {
                return false;
            }

            seconds = defaultMinutes * 60;
            return true;
        }

        private AppSettings CloneSettings(AppSettings settings)
        {
            AppSettings clone = new AppSettings
            {
                ZipDestinationFiles = settings.ZipDestinationFiles,
                DefaultVersioning = settings.DefaultVersioning,
                AutoIncrementVersion = settings.AutoIncrementVersion,
                MinimizeToSystray = settings.MinimizeToSystray,
                AutoUpdateCheck = settings.AutoUpdateCheck,
                StartWithWindows = settings.StartWithWindows,
                IgnoreCopyErrors = settings.IgnoreCopyErrors,
                AutoPurgeEnabled = settings.AutoPurgeEnabled,
                BackupDestinationConflictHandling = settings.BackupDestinationConflictHandling,
                LogLevel = BackupLogger.NormalizeLogLevel(settings.LogLevel),
                AutoBackupEnabled = settings.AutoBackupEnabled,
                AutoBackupIntervalMinutes = settings.AutoBackupIntervalMinutes,
                AutoBackupIntervalSeconds = settings.AutoBackupIntervalSeconds,
                BackupPathPairs = new List<BackupPathPair>(),
                LastUsedVersionsByPair = settings.LastUsedVersionsByPair != null
                    ? new Dictionary<string, string>(settings.LastUsedVersionsByPair)
                    : new Dictionary<string, string>(),
                BackupStatusesByPair = settings.BackupStatusesByPair != null
                    ? new Dictionary<string, BackupPathStatus>(settings.BackupStatusesByPair)
                    : new Dictionary<string, BackupPathStatus>(),
                MainWindowWidth = settings.MainWindowWidth,
                MainWindowHeight = settings.MainWindowHeight,
                MainWindowLeft = settings.MainWindowLeft,
                MainWindowTop = settings.MainWindowTop,
                Tags = settings.Tags != null
                    ? new List<string>(settings.Tags)
                    : new List<string>(),
                BackupVersionDialogWidth = settings.BackupVersionDialogWidth,
                BackupVersionDialogHeight = settings.BackupVersionDialogHeight
            };

            foreach (BackupPathPair pair in settings.BackupPathPairs)
            {
                clone.BackupPathPairs.Add(new BackupPathPair
                {
                    IsEnabled = pair.IsEnabled,
                    SourceDirectory = pair.SourceDirectory,
                    TargetDirectory = pair.TargetDirectory,
                    Versioning = pair.Versioning,
                    IgnoreCopyErrors = pair.IgnoreCopyErrors,
                    SkipDialogs = pair.SkipDialogs,
                    RetentionKeepLastEnabled = pair.RetentionKeepLastEnabled,
                    RetentionKeepLastCount = pair.RetentionKeepLastCount,
                    RetentionKeepDaysEnabled = pair.RetentionKeepDaysEnabled,
                    RetentionKeepDaysCount = pair.RetentionKeepDaysCount,
                    RetentionMode = pair.RetentionMode,
                    RetentionExcludedTags = pair.RetentionExcludedTags != null
                        ? new List<string>(pair.RetentionExcludedTags)
                        : new List<string>(),
                    ExcludedPaths = new List<string>(pair.ExcludedPaths)
                });
            }

            return clone;
        }

        private void ModernSettings_MouseDown(object? sender, MouseEventArgs e)
        {
            if (activeHintOwner != null)
            {
                settingsToolTip.Hide(activeHintOwner);
                activeHintOwner = null;
            }
        }

        private void titleBar_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WmNclButtonDown, HtCaption, 0);
            }
        }

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern int SendMessage(nint hWnd, int msg, int wParam, int lParam);
    }
}