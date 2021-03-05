using System;
using BenchmarkDotNet.Attributes;
using OpenTabletDriver.Native.Windows;
using OpenTabletDriver.Native.Windows.Input;

namespace OpenTabletDriver.Benchmarks.Output
{
    public class WindowsInteropBenchmark
    {
        private INPUT[] inputs;

        [GlobalSetup]
        public void Setup()
        {
            inputs = new INPUT[]
            {
                new INPUT
                {
                    type = INPUT_TYPE.MOUSE_INPUT,
                    U = new InputUnion
                    {
                        mi = new MOUSEINPUT
                        {
                            time = 0,
                            dx = 0,
                            dy = 0,
                            dwExtraInfo = UIntPtr.Zero
                        }
                    }
                }
            };
        }

        [Benchmark]
        public void SendInputAbsolute()
        {
            inputs[0].U.mi.dwFlags = MOUSEEVENTF.ABSOLUTE | MOUSEEVENTF.MOVE | MOUSEEVENTF.VIRTUALDESK;
            _ = Windows.SendInput(1, inputs, INPUT.Size);
        }

        [Benchmark]
        public void SendInputRelative()
        {
            inputs[0].U.mi.dwFlags = MOUSEEVENTF.MOVE;
            _ = Windows.SendInput(1, inputs, INPUT.Size);
        }
    }
}