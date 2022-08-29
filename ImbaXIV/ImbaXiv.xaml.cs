using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Threading;
using System.Text;

namespace ImbaXIV
{
    public partial class MainWindow : Window
    {
        private ImbaXIVCore core;
        private MinimapWindow minimapWindow;
        private bool minified = false;
        private bool debugMode = true;
        private Config _config;

        private IntPtr _windowHandle;
        private HwndSource _source;
        private const int HOTKEY_ID = 1337;
        private const int MOD_ALT = 1;
        private const int MOD_CONTROL = 2;
        private const int MOD_SHIFT = 4;
        private KeyBinding _minimapKeyBinding;
        private KeyBinding _pendingMinimapKeyBinding;

        private readonly string _removeKeyBindMsg = "Right click to remove keybind";

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        public MainWindow()
        {
            InitializeComponent();
            core = new ImbaXIVCore();
            ToggleDebug();
            ToggleMinify();
            Task.Run(CoreWorker);

            _config = ConfigManager.Load();
            if (_config.Version != ConfigManager.Version)
                _config = new Config();

            minimapWindow = new MinimapWindow(core, _config);
            MinimapSlider.Value = _config.MinimapSize;
            _minimapKeyBinding = new KeyBinding();
            _pendingMinimapKeyBinding = new KeyBinding();
            _minimapKeyBinding.Key = _config.MinimapKeyBindingKey;
            _minimapKeyBinding.Modifiers = _config.MinimapKeyBindingModifiers;
            if (_minimapKeyBinding.Key == Key.None)
            {
                _minimapKeyBinding.Key = Key.C;
                _minimapKeyBinding.Modifiers = ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt;
            }
            string keybindStr = KeyBindingToString(_minimapKeyBinding);
            MinimapHotkeyTextBox.Text = keybindStr;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            if (_minimapKeyBinding.Key == Key.None)
                return;

            int vkCode = KeyInterop.VirtualKeyFromKey(_minimapKeyBinding.Key);
            int modifiers = (int)_minimapKeyBinding.Modifiers;
            RegisterHotKey(_windowHandle, HOTKEY_ID, modifiers, vkCode);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            int minimapHotkey = KeyInterop.VirtualKeyFromKey(_minimapKeyBinding.Key);
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            int vkey = ((int)lParam >> 16) & 0xFFFF;
                            if (vkey == minimapHotkey)
                            {
                                if (IsMinimapWindowOpened())
                                {
                                    minimapWindow.Topmost = !minimapWindow.Topmost;
                                    if (minimapWindow.Topmost)
                                        minimapWindow.UnhideMinimap(HideReason.UserSetting);
                                    else
                                        minimapWindow.HideMinimap(HideReason.UserSetting);
                                }
                            }
                            handled = true;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }

        private bool IsMinimapWindowOpened()
        {
            foreach (var wnd in Application.Current.Windows)
            {
                if (wnd is MinimapWindow)
                    return true;
            }
            return false;
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            if (_minimapKeyBinding.Key != Key.None)
                UnregisterHotKey(_windowHandle, HOTKEY_ID);
            minimapWindow.Close();

            minimapWindow.UpdateConfig();
            _config = minimapWindow.Config;
            _config.MinimapKeyBindingModifiers = _minimapKeyBinding.Modifiers;
            _config.MinimapKeyBindingKey = _minimapKeyBinding.Key;
            ConfigManager.Save(_config);
            base.OnClosed(e);
        }

        private void eventloop()
        {
            if (!core.IsAttached)
            {
                if (minimapWindow.IsVisible)
                    minimapWindow.Visibility = Visibility.Hidden;

                if (!core.AttachProcess())
                    return;
                minimapWindow.Visibility = Visibility.Visible;
            }
            if (!core.Update())
            {
                return;
            }
            if (core.MainCharEntity.StructPtr == 0 || !core.MainCharEntity.IsVisible)
            {
                if (minimapWindow.IsVisible)
                    minimapWindow.HideMinimap(HideReason.PcNotVisible);
            }
            else
            {
                if (!minimapWindow.IsVisible)
                    minimapWindow.UnhideMinimap(HideReason.PcNotVisible);
            }
            minimapWindow.Update();

            PosInfo mainCharPos = core.MainCharEntity.Pos;
            MainCharPosTextBox.Text = $"{mainCharPos.X,4:N1} {mainCharPos.Y,4:N1} {mainCharPos.Z,4:N1}";
            StructCTextBox.Text = core.TargetInfo;

            String questEntityText = "";
            foreach (var entity in core.QuestEntities)
            {
                questEntityText += $"{entity.Name}: {entity.Pos.X,4:N1} {entity.Pos.Y,4:N1} {entity.Pos.Z,4:N1}\n";
            }
            QuestEntitiesTextBox.Text = questEntityText;
        }

        private void CoreWorker()
        {
            while (true)
            {
                Dispatcher.Invoke(eventloop);
                Thread.Sleep(25);
            }
        }

        private void TargetsBtn_Click(object sender, RoutedEventArgs e)
        {
            String[] targets;

            if (TargetsTextBox.Text == "")
            {
                targets = new string[0];
            }
            else
            {
                targets = TargetsTextBox.Text.Split(',');
            }
            core.Targets = targets;
        }


        private void AlwaysOnTopMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
            if (this.Topmost)
            {
                AlwaysOnTopMenuItem.Header = "Disable always on top";
            }
            else
            {
                AlwaysOnTopMenuItem.Header = "Enable always on top";
            }
        }

        private void ToggleMinify()
        {
            minified = !minified;
            if (minified)
            {
                this.Width = 238;
            }
            else
            {
                this.Width = 628;
            }
        }

        private void MinifyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ToggleMinify();
            if (minified)
            {
                MinifyMenuItem.Header = "Show full window";
            }
            else
            {
                MinifyMenuItem.Header = "Show minimap only";
            }
        }

        private void ToggleDebug()
        {
            debugMode = !debugMode;
            if (debugMode)
            {
                DebugGrid.Visibility = Visibility.Visible;
                QuestEntitiesTextBox.Height = 60;
            }
            else
            {
                DebugGrid.Visibility = Visibility.Hidden;
                QuestEntitiesTextBox.Height = 188;
            }
        }

        private void DebugMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ToggleDebug();
            if (debugMode)
            {
                DebugMenuItem.Header = "Disable debug mode";
            }
            else
            {
                DebugMenuItem.Header = "Enable debug mode";
            }
        }

        private void CopyToClipboardMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(StructCTextBox.Text);
        }

        private void MinimapSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (minimapWindow is null)
                return;
            minimapWindow.Resize(e.NewValue);
            MinimapSizeTextBlock.Text = $"{e.NewValue * 100}%";
        }

        private void TextBox_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            MessageTextBlock.Text = _removeKeyBindMsg;
        }

        private void TextBox_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
                MessageTextBlock.Text = "";
        }

        private void TextBox_PreviewMouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_minimapKeyBinding.Key != Key.None)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);
                MinimapHotkeyTextBox.Text = "<None>";
                _minimapKeyBinding.Key = Key.None;
                _minimapKeyBinding.Modifiers = ModifierKeys.None;
                _pendingMinimapKeyBinding.Key = Key.None;
                _pendingMinimapKeyBinding.Modifiers = ModifierKeys.None;
            }
            e.Handled = true;
        }

        private void MinimapHotkeyTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            bool isAlnum = (e.Key >= Key.A && e.Key <= Key.Z) ||
                           (e.Key >= Key.D0 && e.Key <= Key.D9) ||
                           (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9);

            if (!isAlnum)
                return;

            _pendingMinimapKeyBinding.Key = e.Key;
            _pendingMinimapKeyBinding.Modifiers = Keyboard.Modifiers;
            string keybindStr = KeyBindingToString(_pendingMinimapKeyBinding);
            MinimapHotkeyTextBox.Text = keybindStr;
            MinimapHotkeyTextBox.Foreground = Brushes.Red;
        }

        private void MinimapHotkeyResetBtn_Click(object sender, RoutedEventArgs e)
        {
            string keybindStr = "<None>";
            if (_minimapKeyBinding.Key != Key.None)
                keybindStr = KeyBindingToString(_minimapKeyBinding);
            MinimapHotkeyTextBox.Text = keybindStr;
            MinimapHotkeyTextBox.Foreground = Brushes.Black;
        }

        private string KeyBindingToString(KeyBinding keybinding)
        {
            string modifiers = keybinding.Modifiers == ModifierKeys.None ? "" : keybinding.Modifiers.ToString();
            string keyStr = keybinding.Key.ToString();
            if (keybinding.Key >= Key.D0 && keybinding.Key <= Key.D9)
            {
                int val = (int)keybinding.Key - (int)Key.D0;
                keyStr = $"{val}";
            }
            return string.Join(" ", new string[] { modifiers, keyStr});
        }

        private void MinimapHotkeyUpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_pendingMinimapKeyBinding.Key == Key.None)
                return;
            if (_minimapKeyBinding.Key != Key.None)
                UnregisterHotKey(_windowHandle, HOTKEY_ID);

            if (!RegisterKeyBinding(_pendingMinimapKeyBinding))
            {
                MinimapHotkeyTextBox.Text = "<None>";
                MessageTextBlock.Text = "Failed to register hotkey";
            }
            else
            {
                _minimapKeyBinding.Key = _pendingMinimapKeyBinding.Key;
                _minimapKeyBinding.Modifiers = _pendingMinimapKeyBinding.Modifiers;
            }

            MinimapHotkeyTextBox.Foreground = Brushes.Black;
            _pendingMinimapKeyBinding.Key = Key.None;
            _pendingMinimapKeyBinding.Modifiers = ModifierKeys.None;
        }

        private bool RegisterKeyBinding(KeyBinding keyBinding)
        {
            int vkCode = KeyInterop.VirtualKeyFromKey(keyBinding.Key);
            int modifiers = (int)keyBinding.Modifiers;
            return RegisterHotKey(_windowHandle, HOTKEY_ID, modifiers, vkCode);
        }

        private void SetManualTarget(string targetName)
        {
            core.ManualTargetName = targetName;
            ManualTargetTextBlock.Text = targetName == "" ? "None" : targetName;
        }

        private void ManualTargetUpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            SetManualTarget(ManualTargetTextBox.Text);
        }

        private void ManualTargetClearBtn_Click(object sender, RoutedEventArgs e)
        {
            ManualTargetTextBox.Text = "";
            SetManualTarget("");
        }

        private void ManualTargetTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SetManualTarget(ManualTargetTextBox.Text);
        }
    }
}
