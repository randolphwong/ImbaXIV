using System.Windows;
using System.Windows.Input;

namespace ImbaXIV
{
    public class Config
    {
        public double MinimapSize { get; set; }
        public Point MinimapPos { get; set; }
        public Key MinimapKeyBindingKey { get; set; }
        public ModifierKeys MinimapKeyBindingModifiers { get; set; }
        public string Version { get; set; }

        public Config()
        {
            MinimapSize = 1;
            MinimapPos = new Point();
            MinimapKeyBindingKey = Key.None;
            MinimapKeyBindingModifiers = ModifierKeys.None;
        }
    }
}
