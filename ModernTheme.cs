using System.Drawing;

namespace EasyVersionBackup
{
    public static class ModernTheme
    {
        // window color settings start
        public static readonly Color WindowBackColor = Color.FromArgb(27, 40, 56);
        public static readonly Color TitleBarBackColor = Color.FromArgb(23, 26, 33);
        public static readonly Color ControlBackColor = Color.FromArgb(42, 71, 94);
        public static readonly Color ControlHoverBackColor = Color.FromArgb(55, 90, 120);
        public static readonly Color AccentColor = Color.FromArgb(102, 192, 244);
        public static readonly Color AccentHoverColor = Color.FromArgb(143, 212, 255);
        public static readonly Color TextColor = Color.FromArgb(199, 213, 224);
        public static readonly Color DarkTextColor = Color.FromArgb(23, 26, 33);
        public static readonly Color CloseButtonHoverColor = Color.FromArgb(196, 43, 28);
        // window color settings end

        // window border settings start
        public static readonly Color WindowBorderColor = Color.FromArgb(92, 104, 112);
        public const float WindowBorderWidth = 1F;
        // window border settings end

        // window font settings start
        public const string FontFamilyName = "Segoe UI";
        public const float DefaultFontSize = 9F;
        public const float TitleFontSize = 9F;
        public const float HeaderFontSize = 9F;
        // window font settings end

        // button text positioning settings start
        public static readonly Padding DefaultButtonTextPadding = Padding.Empty;
        public static readonly Padding DialogPrimaryButtonTextPadding = new Padding(0, 0, 0, 1);
        public static readonly Padding DialogSecondaryButtonTextPadding = new Padding(0, 0, 0, 2);
        public static readonly Padding ToolbarPlusButtonTextPadding = new Padding(0, 0, 0, 3);
        public static readonly Padding ToolbarMinusButtonTextPadding = new Padding(0, 0, 0, 4);
        // button text positioning settings end

        // button size settings start
        public static readonly Size DialogButtonSize = new Size(75, 27);
        public static readonly Size ToolbarButtonSize = new Size(28, 26);
        // button size settings end
    }
}