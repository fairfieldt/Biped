
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;

using System.Windows.Input;



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

        private Pedal currentPedalToSet = Pedal.NONE;
        public MainWindow()
        {
            InitializeComponent();
            LoadPedalBinds();
            biped = new Biped(config);
            if (biped.DeviceConnected != true)
            {
                StatusText.Content = NO_DEVICE_DETECTED_TEXT;
            }
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
            //if (biped.DeviceConnected)
            //{
                currentPedalToSet = pedal;
                StatusText.Content = BIND_KEY_TEXT;
            //}
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

            LoadPedalBinds();
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

        private void LoadPedalBinds()
        {
            uint left = settings.GetFromRegistry(Pedal.LEFT.ToString("g"));
            uint middle = settings.GetFromRegistry(Pedal.MIDDLE.ToString("g"));
            uint right = settings.GetFromRegistry(Pedal.RIGHT.ToString("g"));

            config = new Config(left, middle, right);
            LeftText.Content = $"{VKeyToKey(VKeyFromScanCode(left)).ToString()} ({left})";
            MiddleText.Content = $"{VKeyToKey(VKeyFromScanCode(middle)).ToString()} ({middle})";
            RightText.Content = $"{VKeyToKey(VKeyFromScanCode(right)).ToString()} ({right})";
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
