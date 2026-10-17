// Design-Rule / UI consistency: Template dialogs use ModernTheme as the sole palette and sizing source.
// Semantic Anchor: Manage existing filter and pair-setting templates without altering backup configuration.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EasyVersionBackup
{
    internal static class TemplateDialogs
    {
        private static Form CreateDialog(string title, Size size)
        {
            Form dialog = new Form
            {
                Text = title,
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = size,
                BackColor = ModernTheme.WindowBackColor,
                ForeColor = ModernTheme.TextColor,
                Font = new Font(ModernTheme.FontFamilyName, ModernTheme.DefaultFontSize),
                ShowInTaskbar = false
            };
            ModernWindowFrame.Apply(dialog);
            Panel titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = ModernTheme.TitleBarHeight,
                BackColor = ModernTheme.TitleBarBackColor
            };
            Label caption = new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(ModernTheme.TitleBarTextLeft, 0, 0, 0),
                ForeColor = ModernTheme.TextColor,
                Cursor = Cursors.SizeAll
            };
            Button close = new Button
            {
                Text = "×",
                Dock = DockStyle.Right,
                Width = ModernTheme.TitleBarButtonSize.Width,
                BackColor = ModernTheme.TitleBarBackColor,
                ForeColor = ModernTheme.TextColor,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            close.FlatAppearance.BorderSize = 0;
            close.FlatAppearance.MouseOverBackColor = ModernTheme.CloseButtonHoverColor;
            titleBar.Controls.Add(caption);
            titleBar.Controls.Add(close);
            dialog.Controls.Add(titleBar);
            // Allow dragging the custom title bar without invoking a native Windows caption.
            MouseEventHandler drag = (_, e) =>
            {
                if (e.Button != MouseButtons.Left) return;
                NativeMethods.ReleaseCapture();
                NativeMethods.SendMessage(dialog.Handle, 0xA1, (IntPtr)2, IntPtr.Zero);
            };
            titleBar.MouseDown += drag;
            caption.MouseDown += drag;
            return dialog;
        }

        private static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            internal static extern bool ReleaseCapture();
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            internal static extern IntPtr SendMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);
        }

        internal static string? AskName(IWin32Window owner, System.Collections.Generic.IEnumerable<string> existing)
        {
            using Form dialog = CreateDialog("Save as Template", new Size(360, 166));
            Label description = new Label
            {
                Text = "New name or select a template to overwrite:",
                Location = new Point(12, ModernTheme.TitleBarHeight + 12),
                AutoSize = true,
                ForeColor = ModernTheme.TextColor
            };
            ComboBox name = new ComboBox
            {
                Location = new Point(12, ModernTheme.TitleBarHeight + 36),
                Width = dialog.ClientSize.Width - 24,
                DropDownStyle = ComboBoxStyle.DropDown,
                BackColor = ModernTheme.ControlBackColor,
                ForeColor = ModernTheme.TextColor,
                FlatStyle = FlatStyle.Flat
            };
            name.Items.AddRange(existing.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).Cast<object>().ToArray());
            Button ok = ModernTheme.CreateDialogPrimaryButton("buttonSaveTemplate", "Save");
            Button cancel = ModernTheme.CreateDialogSecondaryButton("buttonCancelTemplate", "Cancel", DialogResult.Cancel);
            ModernTheme.PositionDialogButtons(dialog, ok, cancel);
            ok.Click += (_, _) =>
            {
                string entered = name.Text.Trim();
                if (entered.Length == 0) return;
                if (name.Items.Cast<string>().Any(x => string.Equals(x, entered, StringComparison.OrdinalIgnoreCase)))
                {
                    if (ModernConfirmationDialog.Show(dialog, "Overwrite Template", $"Overwrite template '{entered}'?") != DialogResult.Yes) return;
                }
                dialog.DialogResult = DialogResult.OK;
            };
            dialog.Controls.AddRange(new Control[] { description, name, ok, cancel });
            dialog.AcceptButton = ok;
            dialog.CancelButton = cancel;
            return dialog.ShowDialog(owner) == DialogResult.OK ? name.Text.Trim() : null;
        }

        internal static void Manage(IWin32Window owner, AppSettings settings)
        {
            using Form dialog = CreateDialog("Template Manager", new Size(500, 320));
            Label typeLabel = new Label { Text = "Template type", Location = new Point(12, ModernTheme.TitleBarHeight + 14), AutoSize = true };
            ComboBox type = new ComboBox
            {
                Location = new Point(12, ModernTheme.TitleBarHeight + 34),
                Width = dialog.ClientSize.Width - 24,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = ModernTheme.ControlBackColor,
                ForeColor = ModernTheme.TextColor,
                FlatStyle = FlatStyle.Flat
            };
            type.Items.AddRange(new object[] { "Filter Templates", "Settings Templates" });
            ListBox items = new ListBox
            {
                Location = new Point(12, ModernTheme.TitleBarHeight + 68),
                Size = new Size(dialog.ClientSize.Width - 24, 145),
                BackColor = ModernTheme.ControlBackColor,
                ForeColor = ModernTheme.TextColor,
                BorderStyle = BorderStyle.FixedSingle
            };
            void Refresh()
            {
                items.Items.Clear();
                IEnumerable<string> keys = type.SelectedIndex == 0
                    ? settings.ExclusionTemplates.Keys.AsEnumerable()
                    : settings.PairSettingsTemplates.Keys.AsEnumerable();
                items.Items.AddRange(keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).Cast<object>().ToArray());
            }
            type.SelectedIndexChanged += (_, _) => Refresh();
            Button remove = ModernTheme.CreateDialogSecondaryButton("buttonRemoveTemplate", "Remove", DialogResult.None);
            Button done = ModernTheme.CreateDialogPrimaryButton("buttonDoneTemplates", "Done");
            ModernTheme.PositionDialogButtons(dialog, done, remove);
            done.Click += (_, _) => { dialog.DialogResult = DialogResult.OK; };
            remove.Click += (_, _) =>
            {
                if (items.SelectedItem is not string key) return;
                if (ModernConfirmationDialog.Show(dialog, "Remove Template", $"Remove template '{key}'?") != DialogResult.Yes) return;
                if (type.SelectedIndex == 0) settings.ExclusionTemplates.Remove(key);
                else settings.PairSettingsTemplates.Remove(key);
                Refresh();
            };
            dialog.Controls.AddRange(new Control[] { typeLabel, type, items, remove, done });
            type.SelectedIndex = 0;
            dialog.AcceptButton = done;
            dialog.ShowDialog(owner);
        }
    }
}
