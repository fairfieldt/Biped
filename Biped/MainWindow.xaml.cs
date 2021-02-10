
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

using System.Windows.Input;
using System.Windows.Interop;

namespace biped
{

    public enum MapType : uint
    {
        MAPVK_VK_TO_VSC = 0x0,
        MAPVK_VSC_TO_VK = 0x1,
        MAPVK_VK_TO_CHAR = 0x2,
        MAPVK_VSC_TO_VK_EX = 0x3,
    }

    public class ShowWindowCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Application.Current.MainWindow.Show();
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string NO_DEVICE_DETECTED_TEXT = "No pedal found. Make sure it's connected.";
        private const string BIND_KEY_TEXT = "Hit a key to change the bind.";
        private const string BIND_BUTTON_TEXT = "Click a box to change a pedal.";

        private readonly SettingsStorage settings = new SettingsStorage();
        private readonly Biped biped;
        private Config config;

        private DateTime lastBindTime;

        private Pedal currentPedalToSet = Pedal.NONE;

        static Mutex mutex = new Mutex(true, "Biped-SingleInstance");

        public const int HWND_BROADCAST = 0xffff;
        public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");
        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);
        [DllImport("user32")]
        public static extern IntPtr FindWindow(string classname, string windowname);
        public MainWindow()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                InitializeComponent();
                LoadPedalBinds();
                biped = new Biped(config);
                if (biped.DeviceConnected != true)
                {
                    StatusText.Content = NO_DEVICE_DETECTED_TEXT;
                }
            }
            else
            {
                IntPtr hwnd = FindWindow(null, "Biped");
                if (hwnd != IntPtr.Zero)
                {
                    PostMessage(hwnd, WM_SHOWME, IntPtr.Zero, IntPtr.Zero);
                }
                Application.Current.Shutdown();

            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);

        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_SHOWME)
            {
                Show();
            }
            return IntPtr.Zero;
        }


        public void ApplyCommandLineBindings(uint left, uint middle, uint right)
        {
            System.Diagnostics.Debug.WriteLine($"Applying Keybind Left: {VKeyToKey(VKeyFromScanCode(left)).ToString()} ({left}), " +
                            $"Middle: {VKeyToKey(VKeyFromScanCode(middle)).ToString()} ({middle}), " +
                            $"Right: {VKeyToKey(VKeyFromScanCode(right)).ToString()} ({right})");

            SavePedalBind(Pedal.LEFT, left);
            SavePedalBind(Pedal.MIDDLE, middle);
            SavePedalBind(Pedal.RIGHT, right);

            LoadPedalBinds();
        }

        private void RecordPedalBind(Pedal pedal)
        {
            if (biped.DeviceConnected)
            {
                if (WasJustBound())
                   return;

                currentPedalToSet = pedal;
                StatusText.Content = BIND_KEY_TEXT;
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs args)
        {
            if (currentPedalToSet == Pedal.NONE) 
                return;

            int vKey = KeyInterop.VirtualKeyFromKey(args.Key);
            uint scanCode = ScanCodeFromVKey(vKey);
            SavePedalBind(currentPedalToSet, scanCode);

            StatusText.Content = BIND_BUTTON_TEXT;
            currentPedalToSet = Pedal.NONE;

            LoadPedalBinds(true);
        }

        private void OnMouseDown(object sender, MouseEventArgs args)
        {
            if (currentPedalToSet == Pedal.NONE)
                return;

            if (args.LeftButton == MouseButtonState.Pressed)
            {
                SavePedalBind(currentPedalToSet, (uint)CustomButtons.MOUSE_BUTTON_CODES.MouseLeft);
            }
            else if (args.MiddleButton == MouseButtonState.Pressed)
            {
                SavePedalBind(currentPedalToSet, (uint)CustomButtons.MOUSE_BUTTON_CODES.MouseMiddle);
            }
            else if (args.RightButton == MouseButtonState.Pressed)
            {
                SavePedalBind(currentPedalToSet, (uint)CustomButtons.MOUSE_BUTTON_CODES.MouseRight);
            }

            StatusText.Content = BIND_BUTTON_TEXT;
            currentPedalToSet = Pedal.NONE;
            LoadPedalBinds(true);
        }

        private void SavePedalBind(Pedal pedal, uint keyCode)
        {
            settings.SaveToRegistry(pedal.ToString("g"), keyCode);
            switch (pedal)
            {
                case Pedal.LEFT:
                    config.Left = keyCode;
                    break;
                case Pedal.MIDDLE:
                    config.Middle = keyCode;
                    break;
                case Pedal.RIGHT:
                    config.Right = keyCode;
                    break;
            }
            lastBindTime = DateTime.Now;
        }

        private Key VKeyToKey(int vKey)
        {
            return KeyInterop.KeyFromVirtualKey(vKey);
        }

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, MapType uMapType);


        private uint ScanCodeFromVKey(int vKey)
        {
            return MapVirtualKey((uint)vKey, MapType.MAPVK_VK_TO_VSC);
        }

        private int VKeyFromScanCode(uint scanCode)
        {
            return (int)MapVirtualKey(scanCode, MapType.MAPVK_VSC_TO_VK);
        }

        private string GetLabelContent(uint btnCode)
        {
            switch (btnCode)
            {
                case (uint)CustomButtons.MOUSE_BUTTON_CODES.MouseLeft:
                    return $"Mouse Btn Left ({btnCode})";
                case (uint)CustomButtons.MOUSE_BUTTON_CODES.MouseMiddle:
                    return $"Mouse Btn Middle ({btnCode})";
                case (uint)CustomButtons.MOUSE_BUTTON_CODES.MouseRight:
                    return $"Mouse Btn Right ({btnCode})";
                default:
                    return $"{VKeyToKey(VKeyFromScanCode(btnCode)).ToString()} ({btnCode})";
            }
        }

        private void LoadPedalBinds(bool update = false)
        {
            uint left = settings.GetFromRegistry(Pedal.LEFT.ToString("g"));
            uint middle = settings.GetFromRegistry(Pedal.MIDDLE.ToString("g"));
            uint right = settings.GetFromRegistry(Pedal.RIGHT.ToString("g"));

            config = new Config(left, middle, right);

            LeftText.Content = GetLabelContent(left);
            MiddleText.Content = GetLabelContent(middle);
            RightText.Content = GetLabelContent(right);

            if (update)
                biped.UpdateConfig(config);
        }

        private bool WasJustBound()
        {
            return lastBindTime.AddSeconds(0.5) > DateTime.Now;
        }

        private void LeftText_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            RecordPedalBind(Pedal.LEFT);
        }

        private void MiddleText_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            RecordPedalBind(Pedal.MIDDLE);
        }

        private void RightText_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            RecordPedalBind(Pedal.RIGHT);
        }

        private void MenuQuit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuToFront_Click(object sender, RoutedEventArgs e)
        {
            Show();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }
    }
}
