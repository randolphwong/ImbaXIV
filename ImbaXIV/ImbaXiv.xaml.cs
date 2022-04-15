using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
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

        private IntPtr _windowHandle;
        private HwndSource _source;
        private const int HOTKEY_ID = 1337;
        private const int MOD_ALT = 1;
        private const int MOD_CONTROL = 2;
        private const int MOD_SHIFT = 4;
        private int minimapHotkey = 'C';

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
            minimapWindow = new MinimapWindow();
            minimapWindow.Core = core;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_ALT | MOD_SHIFT | MOD_CONTROL, minimapHotkey);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            int vkey = (((int)lParam >> 16) & 0xFFFF);
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
                    }
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
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            minimapWindow.Close();
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
    }
}
