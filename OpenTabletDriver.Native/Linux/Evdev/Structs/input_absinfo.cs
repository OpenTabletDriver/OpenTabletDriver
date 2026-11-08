using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Linux.Evdev.Structs
{
    // https://github.com/torvalds/linux/blob/05441d697f7f9a927264f4058cbbc6a03dfcac9d/include/uapi/linux/input.h#L66-L100
    [StructLayout(LayoutKind.Sequential)]
    public struct input_absinfo
    {
        public int value;
        public int minimum;
        public int maximum;

        /// <summary>
        /// The fuzz values that is used to filter noise from the event stream. Can likely be used as a libinput-side chatter reduction.
        /// </summary>
        public int fuzz;

        /// <summary>
        /// Values within this range will be discarded by joydev interface and report as 0 instead <para/>
        /// Since we're not a joydev interface, this probably does nothing for us to set
        /// </summary>
        public int flat;

        /// <summary>
        /// Main vector/size axes (ABS_X/ABS_Y/ABS_Z/ABS_MT_*): units/mm <para/>
        /// Rotational axes (ABS_RX/ABS_RY/ABS_RZ): units/radian <para/>
        /// </summary>
        public int resolution;
    }
}
