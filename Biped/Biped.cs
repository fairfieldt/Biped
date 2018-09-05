using System;
using System.IO;
using System.Linq;
using System.Reflection;
using HidLibrary;

namespace Biped
{

    enum Pedal
    {
        LEFT,
        MIDDLE,
        RIGHT
    }

    enum PedalState
    {
        DOWN,
        UP
    }

    class Biped
    {
        private const int VendorID = 0x5f3;
        private const int ProductID = 0xFF;
        private static byte LEFT_MASK = 0x01;
        private static byte MIDDLE_MASK = 0x02;
        private static byte RIGHT_MASK = 0x04;
        private static HidDevice device;

        private static PedalState LeftState = PedalState.UP;
        private static PedalState MiddleState = PedalState.UP;
        private static PedalState RightState = PedalState.UP;

        private static Input input = new Input();
        private static Config config = new Config();

        static void Main(string[] args)
        {
            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string configPath = Path.Combine(dir, "config.yaml");
            config.Load(configPath);

            device = HidDevices.Enumerate(VendorID, ProductID).FirstOrDefault();
            device.OpenDevice();

            device.MonitorDeviceEvents = true;

            device.ReadReport(OnReport);

            Console.ReadLine();
        }

        private static void OnReport(HidReport report)
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
            } else
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
            } else
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
                
            } else
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

        private static void HandlePedalEvent(Pedal pedal, PedalState state)
        {
            Console.WriteLine("Pedal {0} status: {1}", pedal.ToString("g"), state.ToString("g"));

            //left send Q, middle send W, right send E
            switch(pedal)
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
