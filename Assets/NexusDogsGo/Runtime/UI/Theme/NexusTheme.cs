using UnityEngine;

namespace NexusDogsGo.UI.Theme
{
    public static class NexusTheme
    {
        public static readonly Color Background = Hex("061729");
        public static readonly Color Surface = Hex("0C2941");
        public static readonly Color SurfaceElevated = Hex("143B58");
        public static readonly Color Cyan = Hex("24C9FF");
        public static readonly Color CyanSoft = Hex("7FE8FF");
        public static readonly Color Green = Hex("32BB47");
        public static readonly Color Red = Hex("E94545");
        public static readonly Color Yellow = Hex("E4B429");
        public static readonly Color TextPrimary = Color.white;
        public static readonly Color TextSecondary = Hex("9AB6C9");

        public static Color Hex(string value)
        {
            Color color;
            return ColorUtility.TryParseHtmlString("#" + value, out color) ? color : Color.magenta;
        }
    }
}
