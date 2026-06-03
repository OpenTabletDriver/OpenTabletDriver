using System;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenTabletDriver.Platform.Display;

namespace OpenTabletDriver.Daemon.Library.Output.WindowsInk
{
    internal class ThinOsPointer
    {
        private readonly Vector2 _conversion;
        private readonly Input[] _inputs =
        {
            new()
            {
                Type = InputType.MouseInput,
                Mouse = new MouseInput
                {
                    Time = 0,
                    ExtraInfo = UIntPtr.Zero
                }
            }
        };

        public ThinOsPointer(IVirtualScreen screen)
        {
            _conversion = new Vector2(screen.Width, screen.Height) / 65535;
        }

        public void SetPosition(Vector2 position)
        {
            var converted = position / _conversion;

            _inputs[0].Mouse.Flags = MouseEventFlags.Absolute | MouseEventFlags.Move | MouseEventFlags.VirtualDesk;
            _inputs[0].Mouse.Dx = (int)converted.X;
            _inputs[0].Mouse.Dy = (int)converted.Y;
            _ = SendInput(1, _inputs, Input.Size);
        }

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint inputCount, [MarshalAs(UnmanagedType.LPArray), In] Input[] inputs, int inputSize);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Input
    {
        public InputType Type;
        public MouseInput Mouse;

        public static int Size => Marshal.SizeOf<Input>();
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MouseInput
    {
        public int Dx;
        public int Dy;
        public uint MouseData;
        public MouseEventFlags Flags;
        public uint Time;
        public UIntPtr ExtraInfo;
    }

    internal enum InputType
    {
        MouseInput,
        KeyboardInput,
        HardwareInput
    }

    [Flags]
    internal enum MouseEventFlags : uint
    {
        Move = 0x0001,
        Absolute = 0x8000,
        VirtualDesk = 0x4000,
        MoveNoCoalesce = 0x2000
    }
}
