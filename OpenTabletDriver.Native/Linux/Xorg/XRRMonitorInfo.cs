using System;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Linux.Xorg
{
    public record struct XRRMonitorInfo(
        int Primary,
        int Automatic,
        int NOutput,
        int X,
        int Y,
        int Width,
        int Height,
        int MWidth,
        int MHeight
    );

    [StructLayout(LayoutKind.Sequential)]
    internal struct XRRMonitorInfoNative
    {
        /// <summary>
        /// The IntPtr behind the Name is an X11 Atom and would require resolving via XGetAtomName
        /// </summary>
        public IntPtr Name;
        public int Primary;
        public int Automatic;
        public int NOutput;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public int MWidth;
        public int MHeight;
        /// <summary>
        /// An X11 <c>RROutput</c> that is a <c>typedef</c> of <c>XID</c>
        /// </summary>
        public IntPtr Outputs;

        public static implicit operator XRRMonitorInfo(XRRMonitorInfoNative monitorInfoNative)
        {
            return new XRRMonitorInfo
            {
                // Name unimplemented
                Primary = monitorInfoNative.Primary,
                Automatic = monitorInfoNative.Automatic,
                NOutput = monitorInfoNative.NOutput,
                X = monitorInfoNative.X,
                Y = monitorInfoNative.Y,
                Width = monitorInfoNative.Width,
                Height = monitorInfoNative.Height,
                MWidth = monitorInfoNative.MWidth,
                MHeight = monitorInfoNative.MHeight,
                // Outputs unimplemented
            };
        }
    }
}
