using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace EasyVersionBackup
{
    public sealed class DebugModeSettings
    {
        public bool Enabled { get; set; }
        public bool HideActiveDataLossWarning { get; set; }
        public bool SkipInitialDisclaimer { get; set; }
        public bool SkipRecurringDataLossWarning { get; set; }
        public bool SkipExperimentalActivationConfirmations { get; set; }
        public bool SkipDeletionConfirmationDialogs { get; set; }
    }

    public static class DebugMode
    {
        // This password is intentionally stored as plain text.
        // It is only a simple access barrier for accidental activation.
        // It is NOT a security boundary and must not be treated as one.
        private const string Password =
            "Explosion!!";

        private static readonly string SettingsDirectoryPath =
            Path.Combine(
                AppContext.BaseDirectory,
                "Settings");

        // Debug settings are stored separately so they are not included
        // in the normal settings export/import process.
        private static readonly string SettingsFilePath =
            Path.Combine(
                SettingsDirectoryPath,
                "EasyVersionBackup.debug.json");

        public static DebugModeSettings Current { get; private set; } =
            Load();

        public static bool ShowAccessAndSettingsDialog(
            Form owner)
        {
            if (!ShowPasswordDialog(
                    owner))
            {
                SystemLogger.WriteDebugModeAccess(
                    false);
                return false;
            }

            SystemLogger.WriteDebugModeAccess(
                true);

            DebugModeSettings editedSettings =
                Clone(
                    Current);

            if (!ShowSettingsDialog(
                    owner,
                    editedSettings))
            {
                return false;
            }

            DebugModeSettings previousSettings =
                Clone(
                    Current);

            Current =
                editedSettings;

            Save(
                Current);

            SystemLogger.WriteDebugModeChanges(
                previousSettings,
                Current);

            return true;
        }

        private static DebugModeSettings Load()
        {
            try
            {
                if (!File.Exists(
                        SettingsFilePath))
                {
                    return new DebugModeSettings();
                }

                string json =
                    File.ReadAllText(
                        SettingsFilePath);

                return JsonSerializer.Deserialize<DebugModeSettings>(
                           json) ??
                       new DebugModeSettings();
            }
            catch
            {
                return new DebugModeSettings();
            }
        }

        private static void Save(
            DebugModeSettings settings)
        {
            Directory.CreateDirectory(
                SettingsDirectoryPath);

            string json =
                JsonSerializer.Serialize(
                    settings,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

            File.WriteAllText(
                SettingsFilePath,
                json);
        }

        private static DebugModeSettings Clone(
            DebugModeSettings settings)
        {
            return new DebugModeSettings
            {
                Enabled =
                    settings.Enabled,
                HideActiveDataLossWarning =
                    settings.HideActiveDataLossWarning,
                SkipInitialDisclaimer =
                    settings.SkipInitialDisclaimer,
                SkipRecurringDataLossWarning =
                    settings.SkipRecurringDataLossWarning,
                SkipExperimentalActivationConfirmations =
                    settings.SkipExperimentalActivationConfirmations,
                SkipDeletionConfirmationDialogs =
                    settings.SkipDeletionConfirmationDialogs
            };
        }

        private static bool ShowPasswordDialog(
            Form owner)
        {
            using Form form =
                CreateDialog(
                    owner,
                    "Debug Mode",
                    new Size(
                        470,
                        190));

            Label labelWarning =
                new Label
                {
                    Text =
                        "Debug Mode can disable safety warnings and confirmations.",
                    Location =
                        new Point(
                            18,
                            52),
                    Size =
                        new Size(
                            434,
                            24),
                    ForeColor =
                        Color.Red,
                    BackColor =
                        Color.Transparent
                };

            Label labelPassword =
                new Label
                {
                    Text =
                        "Password:",
                    Location =
                        new Point(
                            18,
                            86),
                    Size =
                        new Size(
                            90,
                            24),
                    ForeColor =
                        ModernTheme.TextColor,
                    BackColor =
                        Color.Transparent
                };

            TextBox textBoxPassword =
                new TextBox
                {
                    Location =
                        new Point(
                            112,
                            84),
                    Size =
                        new Size(
                            340,
                            27),
                    BackColor =
                        ModernTheme.ControlBackColor,
                    ForeColor =
                        ModernTheme.TextColor,
                    BorderStyle =
                        BorderStyle.FixedSingle,
                    UseSystemPasswordChar =
                        true
                };

            Button buttonOk =
                ModernTheme.CreateDialogPrimaryButton(
                    "buttonOk",
                    "OK");

            Button buttonCancel =
                ModernTheme.CreateDialogSecondaryButton(
                    "buttonCancel",
                    "Cancel",
                    DialogResult.Cancel);

            ModernTheme.PositionDialogButtons(
                form,
                buttonOk,
                buttonCancel);

            buttonOk.Click +=
                (sender, e) =>
                {
                    if (!string.Equals(
                            textBoxPassword.Text,
                            Password,
                            StringComparison.Ordinal))
                    {
                        ModernMessageDialog.Show(
                            form,
                            "Debug Mode",
                            "Incorrect password.");
                        textBoxPassword.SelectAll();
                        textBoxPassword.Focus();
                        return;
                    }

                    form.DialogResult =
                        DialogResult.OK;
                    form.Close();
                };

            form.Controls.Add(
                labelWarning);
            form.Controls.Add(
                labelPassword);
            form.Controls.Add(
                textBoxPassword);
            form.Controls.Add(
                buttonOk);
            form.Controls.Add(
                buttonCancel);

            form.AcceptButton =
                buttonOk;
            form.CancelButton =
                buttonCancel;

            return form.ShowDialog(
                       owner) ==
                   DialogResult.OK;
        }

        private static bool ShowSettingsDialog(
            Form owner,
            DebugModeSettings settings)
        {
            using Form form =
                CreateDialog(
                    owner,
                    "Debug Mode Settings",
                    new Size(
                        650,
                        430));

            Label labelWarning =
                new Label
                {
                    Text =
                        "WARNING: Debug Mode can bypass user-facing safety warnings and confirmations." +
                        Environment.NewLine +
                        "Technical deletion protections, path validation, reparse-point protection, and backup-success checks remain active.",
                    Location =
                        new Point(
                            18,
                            50),
                    Size =
                        new Size(
                            614,
                            54),
                    ForeColor =
                        Color.Red,
                    BackColor =
                        Color.Transparent
                };

            CheckBox checkBoxDisableAll =
                CreateCheckBox(
                    "Disable all user-facing safety features",
                    18,
                    116,
                    true);

            CheckBox checkBoxHideActiveDataLossWarning =
                CreateCheckBox(
                    "Hide the Retention/Cleanup active warning banner",
                    40,
                    154,
                    false);

            CheckBox checkBoxSkipInitialDisclaimer =
                CreateCheckBox(
                    "Skip the initial program disclaimer",
                    40,
                    186,
                    false);

            CheckBox checkBoxSkipRecurringDataLossWarning =
                CreateCheckBox(
                    "Skip recurring data-loss warnings",
                    40,
                    218,
                    false);

            CheckBox checkBoxSkipExperimentalActivationConfirmations =
                CreateCheckBox(
                    "Skip Retention and Source Cleanup activation confirmations",
                    40,
                    250,
                    false);

            CheckBox checkBoxSkipDeletionConfirmationDialogs =
                CreateCheckBox(
                    "Skip Source Cleanup and Retention deletion-preview confirmations",
                    40,
                    282,
                    false);

            checkBoxDisableAll.Checked =
                settings.Enabled;
            checkBoxHideActiveDataLossWarning.Checked =
                settings.HideActiveDataLossWarning;
            checkBoxSkipInitialDisclaimer.Checked =
                settings.SkipInitialDisclaimer;
            checkBoxSkipRecurringDataLossWarning.Checked =
                settings.SkipRecurringDataLossWarning;
            checkBoxSkipExperimentalActivationConfirmations.Checked =
                settings.SkipExperimentalActivationConfirmations;
            checkBoxSkipDeletionConfirmationDialogs.Checked =
                settings.SkipDeletionConfirmationDialogs;

            CheckBox[] subordinateCheckBoxes =
            {
                checkBoxHideActiveDataLossWarning,
                checkBoxSkipInitialDisclaimer,
                checkBoxSkipRecurringDataLossWarning,
                checkBoxSkipExperimentalActivationConfirmations,
                checkBoxSkipDeletionConfirmationDialogs
            };

            bool isUpdatingAll =
                false;

            checkBoxDisableAll.CheckedChanged +=
                (sender, e) =>
                {
                    if (isUpdatingAll)
                    {
                        return;
                    }

                    isUpdatingAll =
                        true;

                    try
                    {
                        foreach (CheckBox checkBox in
                                 subordinateCheckBoxes)
                        {
                            checkBox.Checked =
                                checkBoxDisableAll.Checked;
                        }
                    }
                    finally
                    {
                        isUpdatingAll =
                            false;
                    }
                };

            Button buttonOk =
                ModernTheme.CreateDialogPrimaryButton(
                    "buttonOk",
                    "OK");

            Button buttonCancel =
                ModernTheme.CreateDialogSecondaryButton(
                    "buttonCancel",
                    "Cancel",
                    DialogResult.Cancel);

            ModernTheme.PositionDialogButtons(
                form,
                buttonOk,
                buttonCancel);

            buttonOk.Click +=
                (sender, e) =>
                {
                    bool anyBypassEnabled =
                        subordinateCheckBoxes[0].Checked ||
                        subordinateCheckBoxes[1].Checked ||
                        subordinateCheckBoxes[2].Checked ||
                        subordinateCheckBoxes[3].Checked ||
                        subordinateCheckBoxes[4].Checked;

                    if (anyBypassEnabled)
                    {
                        DialogResult confirmationResult =
                            ModernConfirmationDialog.Show(
                                form,
                                "Confirm Debug Mode",
                                "Debug Mode will disable one or more user-facing safety warnings or confirmations." +
                                Environment.NewLine +
                                Environment.NewLine +
                                "Technical deletion protections remain active." +
                                Environment.NewLine +
                                Environment.NewLine +
                                "Do you want to save these Debug Mode settings?",
                                new Size(
                                    560,
                                    280));

                        if (confirmationResult !=
                            DialogResult.Yes)
                        {
                            return;
                        }
                    }

                    settings.Enabled =
                        anyBypassEnabled;
                    settings.HideActiveDataLossWarning =
                        checkBoxHideActiveDataLossWarning.Checked;
                    settings.SkipInitialDisclaimer =
                        checkBoxSkipInitialDisclaimer.Checked;
                    settings.SkipRecurringDataLossWarning =
                        checkBoxSkipRecurringDataLossWarning.Checked;
                    settings.SkipExperimentalActivationConfirmations =
                        checkBoxSkipExperimentalActivationConfirmations.Checked;
                    settings.SkipDeletionConfirmationDialogs =
                        checkBoxSkipDeletionConfirmationDialogs.Checked;

                    form.DialogResult =
                        DialogResult.OK;
                    form.Close();
                };

            form.Controls.Add(
                labelWarning);
            form.Controls.Add(
                checkBoxDisableAll);
            form.Controls.Add(
                checkBoxHideActiveDataLossWarning);
            form.Controls.Add(
                checkBoxSkipInitialDisclaimer);
            form.Controls.Add(
                checkBoxSkipRecurringDataLossWarning);
            form.Controls.Add(
                checkBoxSkipExperimentalActivationConfirmations);
            form.Controls.Add(
                checkBoxSkipDeletionConfirmationDialogs);
            form.Controls.Add(
                buttonOk);
            form.Controls.Add(
                buttonCancel);

            form.AcceptButton =
                buttonOk;
            form.CancelButton =
                buttonCancel;

            return form.ShowDialog(
                       owner) ==
                   DialogResult.OK;
        }

        private static CheckBox CreateCheckBox(
            string text,
            int left,
            int top,
            bool bold)
        {
            return new CheckBox
            {
                Text =
                    text,
                Location =
                    new Point(
                        left,
                        top),
                Size =
                    new Size(
                        590,
                        26),
                ForeColor =
                    bold
                        ? Color.Red
                        : ModernTheme.TextColor,
                BackColor =
                    Color.Transparent,
                Font =
                    new Font(
                        ModernTheme.FontFamilyName,
                        ModernTheme.DefaultFontSize,
                        bold
                            ? FontStyle.Bold
                            : FontStyle.Regular)
            };
        }

        private static Form CreateDialog(
            Form owner,
            string title,
            Size clientSize)
        {
            Form form =
                new Form
                {
                    Text =
                        title,
                    StartPosition =
                        FormStartPosition.CenterParent,
                    FormBorderStyle =
                        FormBorderStyle.None,
                    ClientSize =
                        clientSize,
                    BackColor =
                        ModernTheme.WindowBackColor,
                    Font =
                        new Font(
                            ModernTheme.FontFamilyName,
                            ModernTheme.DefaultFontSize),
                    ShowInTaskbar =
                        false,
                    Icon =
                        owner.Icon
                };

            ModernWindowFrame.Apply(
                form);

            Panel panelTitleBar =
                new Panel
                {
                    Dock =
                        DockStyle.Top,
                    Height =
                        ModernTheme.TitleBarHeight,
                    BackColor =
                        ModernTheme.TitleBarBackColor
                };

            Label labelTitle =
                new Label
                {
                    Text =
                        title,
                    AutoSize =
                        false,
                    Location =
                        new Point(
                            ModernTheme.TitleBarTextLeft,
                            0),
                    Size =
                        new Size(
                            form.ClientSize.Width -
                            ModernTheme.TitleBarTextLeft -
                            10,
                            ModernTheme.TitleBarHeight),
                    ForeColor =
                        ModernTheme.TextColor,
                    BackColor =
                        Color.Transparent,
                    TextAlign =
                        ContentAlignment.MiddleLeft,
                    Font =
                        new Font(
                            ModernTheme.FontFamilyName,
                            ModernTheme.TitleFontSize,
                            FontStyle.Bold)
                };

            panelTitleBar.Controls.Add(
                labelTitle);
            form.Controls.Add(
                panelTitleBar);

            return form;
        }
    }
}
