using System;
using System.Linq;
using System.Windows.Input;
using HidLibrary;

namespace biped
{

    enum Pedal
    {
        NONE,
        LEFT,
        MIDDLE,
        RIGHT
    }

    enum PedalState
    {
        DOWN,
        UP
    }

    public class Biped
    {
        private const int VendorID = 0x5f3;
        private const int ProductID = 0xFF;
        private const byte LEFT_MASK = 0x01;
        private const byte MIDDLE_MASK = 0x02;
        private const byte RIGHT_MASK = 0x04;
        private readonly HidDevice device;

        private PedalState LeftState = PedalState.UP;
        private PedalState MiddleState = PedalState.UP;
        private PedalState RightState = PedalState.UP;

        private readonly Input input = new Input();
        private readonly Config config;
        public bool DeviceConnected { get; set; }

        public Biped(Config config)
        {
            this.config = config;
            var devices = HidDevices.Enumerate(VendorID, ProductID);
            if (devices.Count() == 0)
            {
                DeviceConnected = false;
            } else
            {
                device = devices.First();
                DeviceConnected = true;
                device.OpenDevice();

                device.MonitorDeviceEvents = true;

                device.ReadReport(OnReport);
            }
        }

        private void OnReport(HidReport report)
        {
            var status = report.Data[0];
            Console.WriteLine(status);
            var initialLeftState = LeftState;
            var initialMiddleState = MiddleState;
            var initialRightState = RightState;
            if ((status & LEFT_MASK) != 0)
            {
                if (initialLeftState == PedalState.UP)
                {
                    LeftState = PedalState.DOWN;
                    HandlePedalEvent(Pedal.LEFT, LeftState);
                }
            }
            else
            {
                var sendPedalUp = initialLeftState == PedalState.DOWN;
                LeftState = PedalState.UP;
                if (sendPedalUp)
                {
                    HandlePedalEvent(Pedal.LEFT, LeftState);
                }
            }

            if ((status & MIDDLE_MASK) != 0)
            {
                if (MiddleState == PedalState.UP)
                {
                    MiddleState = PedalState.DOWN;
                    HandlePedalEvent(Pedal.MIDDLE, MiddleState);
                }
            }
            else
            {
                var sendPedalUp = initialMiddleState == PedalState.DOWN;
                MiddleState = PedalState.UP;
                if (sendPedalUp)
                {
                    HandlePedalEvent(Pedal.MIDDLE, MiddleState);
                }
            }

            if ((status & RIGHT_MASK) != 0)
            {
                if (RightState == PedalState.UP)
                {
                    RightState = PedalState.DOWN;
                    HandlePedalEvent(Pedal.RIGHT, RightState);
                }

            }
            else
            {
                var sendPedalUp = initialRightState == PedalState.DOWN;
                RightState = PedalState.UP;
                if (sendPedalUp)
                {
                    HandlePedalEvent(Pedal.RIGHT, RightState);
                }
            }

            device.ReadReport(OnReport);

        }

        private void HandlePedalEvent(Pedal pedal, PedalState state)
        {
            System.Diagnostics.Debug.WriteLine("Pedal {0} status: {1}", pedal.ToString("g"), state.ToString("g"));
            System.Diagnostics.Debug.WriteLine("Sending key {0} {1}", config.Left, (Key)config.Left);
            switch (pedal)
            {
                case Pedal.LEFT:
                    input.SendKey(config.Left, state);
                    break;
                case Pedal.MIDDLE:
                    input.SendKey(config.Middle, state);
                    break;
                case Pedal.RIGHT:
                    input.SendKey(config.Right, state);
                    break;

            }
        }
    }
}
