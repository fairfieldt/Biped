
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

using System.Windows.Input;


namespace biped
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string BIND_KEY_TEXT = "Hit a key to change the bind.";
        private static string BIND_BUTTON_TEXT = "Click a box to change a pedal.";

        private SettingsStorage settings = new SettingsStorage();
        private Biped biped;
        private Config config;

        private Pedal currentPedalToSet = Pedal.NONE;
        public MainWindow()
        {
            InitializeComponent();
            LoadPedalBinds();
            biped = new Biped(config);
        }

        private void RecordPedalBind(Pedal pedal)
        {
            currentPedalToSet = pedal;
            StatusText.Content = BIND_KEY_TEXT;
        }

        private void OnKeyUp(object sender, KeyEventArgs args)
        {
            switch (currentPedalToSet)
            {
                case Pedal.LEFT:
                    LeftText.Content = args.Key;
                    break;
                case Pedal.MIDDLE:
                    MiddleText.Content = args.Key;
                    break;
                case Pedal.RIGHT:
                    RightText.Content = args.Key;
                    break;
                case Pedal.NONE:
                    return;
            }

            int vKey = KeyInterop.VirtualKeyFromKey(args.Key);
            SavePedalBind(currentPedalToSet, vKey);

            StatusText.Content = BIND_BUTTON_TEXT;
            currentPedalToSet = Pedal.NONE;

        }

        private void SavePedalBind(Pedal pedal, int keyCode)
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

        private Key vKeyToKey(int vKey)
        {
            return KeyInterop.KeyFromVirtualKey(vKey);
        }

        private void LoadPedalBinds()
        {
            int left = settings.GetFromRegistry(Pedal.LEFT.ToString("g"));
            int middle = settings.GetFromRegistry(Pedal.MIDDLE.ToString("g"));
            int right = settings.GetFromRegistry(Pedal.RIGHT.ToString("g"));

            config = new Config(left, middle, right);
            LeftText.Content = vKeyToKey(left).ToString();
            MiddleText.Content = vKeyToKey(middle).ToString();
            RightText.Content = vKeyToKey(right).ToString();
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
