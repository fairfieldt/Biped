using System;
using System.Runtime.InteropServices;
using System.Windows.Controls;

namespace biped
{
    class Input
    {
        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;

        private static readonly uint KEYEVENTF_SCANCODE = 0x08;
        private static readonly uint KEYEVENTF_KEYUP = 0x02;
        [DllImport("user32.dll")]
        internal static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        public void SendKey(uint keyCode, PedalState state)
        {
            uint flags = KEYEVENTF_SCANCODE;
            if (state == PedalState.UP)
            {
                flags |= KEYEVENTF_KEYUP;
            }

            INPUT input;

            //if (keyCode == 1000 || keyCode == 2000 || keyCode == 3000)
            if (Enum.IsDefined(typeof(CustomButtons.MOUSE_BUTTON_CODES), keyCode))
            {
                uint mFlags = 0;
                switch (keyCode)
                {
                    case (uint)CustomButtons.MOUSE_BUTTON_CODES.MouseLeft:
                        mFlags = state == PedalState.UP ? MOUSEEVENTF_LEFTUP : MOUSEEVENTF_LEFTDOWN;
                        break;
                    case (uint)CustomButtons.MOUSE_BUTTON_CODES.MouseMiddle:
                        mFlags = state == PedalState.UP ? MOUSEEVENTF_MIDDLEUP : MOUSEEVENTF_MIDDLEDOWN;
                        break;
                    case (uint)CustomButtons.MOUSE_BUTTON_CODES.MouseRight:
                        mFlags = state == PedalState.UP ? MOUSEEVENTF_RIGHTUP : MOUSEEVENTF_RIGHTDOWN;
                        break;
                    default:
                        return;
                }

                input = new INPUT
                {
                    Type = 0
                };
                input.Data.Mouse = new MOUSEINPUT
                {
                    X = 0,
                    Y = 0,
                    MouseData = 0,
                    Flags = mFlags,
                    Time = 0,
                    ExtraInfo = IntPtr.Zero
                };
            }
            else
            {
                input = new INPUT
                {
                    Type = 1
                };
                input.Data.Keyboard = new KEYBDINPUT
                {
                    Vk = 0,
                    Scan = (ushort)keyCode,
                    Flags = flags,
                    Time = 0,
                    ExtraInfo = IntPtr.Zero
                };
            }
            INPUT[] inputs = new INPUT[] { input };
            if (SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT))) == 0)
            {
                Console.WriteLine("Error in sendKey");
            }
        }

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646270(v=vs.85).aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct INPUT
        {
            public uint Type;
            public MOUSEKEYBDHARDWAREINPUT Data;
        }

        /// <summary>
        /// http://social.msdn.microsoft.com/Forums/en/csharplanguage/thread/f0e82d6e-4999-4d22-b3d3-32b25f61fb2a
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        internal struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)]
            public HARDWAREINPUT Hardware;
            [FieldOffset(0)]
            public KEYBDINPUT Keyboard;
            [FieldOffset(0)]
            public MOUSEINPUT Mouse;
        }

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646310(v=vs.85).aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct HARDWAREINPUT
        {
            public uint Msg;
            public ushort ParamL;
            public ushort ParamH;
        }

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646310(v=vs.85).aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct KEYBDINPUT
        {
            public ushort Vk;
            public ushort Scan;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        /// <summary>
        /// http://social.msdn.microsoft.com/forums/en-US/netfxbcl/thread/2abc6be8-c593-4686-93d2-89785232dacd
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT
        {
            public int X;
            public int Y;
            public uint MouseData;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }
    }
}
