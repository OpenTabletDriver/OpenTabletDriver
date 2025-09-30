using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.Windows.PenPointer
{
    using HANDLE = IntPtr;
    using HWND = IntPtr;
    using DWORD = UInt32;

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    /// <summary>
    /// Identifies the pointer input types.
    /// </summary>
    public enum POINTER_INPUT_TYPE : uint
    {
        /// <summary>
        /// Generic pointer type. This type never appears in pointer messages or pointer data. Some data query functions allow the caller to restrict the query to specific pointer type. The PT_POINTER type can be used in these functions to specify that the query is to include pointers of all types
        /// </summary>
        PT_POINTER = 1,

        /// <summary>
        /// Touch pointer type.
        /// </summary>
        PT_TOUCH = 2,

        /// <summary>
        /// Pen pointer type.
        /// </summary>
        PT_PEN = 3,

        /// <summary>
        /// Mouse pointer type.
        /// </summary>
        PT_MOUSE = 4,

        /// <summary>
        /// Touchpad pointer type (Windows 8.1 and later).
        /// </summary>
        PT_TOUCHPAD = 5
    }

    /// <summary>
    /// Identifies the pointer device types.
    /// </summary>
    public enum POINTER_DEVICE_TYPE : uint
    {
        /// <summary>
        /// Direct pen digitizer (integrated into display).
        /// </summary>
        INTEGRATED_PEN = 0x00000001,

        /// <summary>
        /// Indirect pen digitizer (not integrated into display).
        /// </summary>
        EXTERNAL_PEN = 0x00000002,

        /// <summary>
        /// Touch digitizer.
        /// </summary>
        TOUCH = 0x00000003,

        /// <summary>
        /// Touchpad digitizer (Windows 8.1 and later).
        /// </summary>
        TOUCH_PAD = 0x00000004,

        /// <summary>
        /// Forces this enumeration to compile to 32 bits in size. Without this value, some compilers would allow this enumeration to compile to a size other than 32 bits.
        /// </summary>
        /// <remarks>
        /// You should not use this value.
        /// </remarks>
        MAX = 0xFFFFFFFF
    }

    /// <summary>
    /// Identifies the visual feedback behaviors available to <see cref="SyntheticPointerInterop.CreateSyntheticPointerDevice"/>.
    /// </summary>
    public enum POINTER_FEEDBACK_MODE
    {
        /// <summary>
        /// Visual feedback might be suppressed by the user's pen (Settings -> Devices -> Pen & Windows Ink) and touch (Settings -> Ease of Access -> Cursor & pointer size) settings.
        /// </summary>
        DEFAULT = 1,

        /// <summary>
        /// Visual feedback overrides the user's pen and touch settings.
        /// </summary>
        INDIRECT = 2,

        /// <summary>
        /// Visual feedback is disabled.
        /// </summary>
        NONE = 3
    }

    /// <summary>
    /// Identifies a change in the state of a button associated with a pointer.
    /// </summary>
    public enum POINTER_BUTTON_CHANGE_TYPE
    {
        /// <summary>
        /// No change in button state.
        /// </summary>
        NONE,

        /// <summary>
        /// The first button (see <see cref="POINTER_FLAGS.FIRSTBUTTON"/>) transitioned to a pressed state.
        /// </summary>
        FIRSTBUTTON_DOWN,

        /// <summary>
        /// The first button (see <see cref="POINTER_FLAGS.FIRSTBUTTON"/>) transitioned to a released state.
        /// </summary>
        FIRSTBUTTON_UP,

        /// <summary>
        /// The second button (see <see cref="POINTER_FLAGS.SECONDBUTTON"/>) transitioned to a pressed state.
        /// </summary>
        SECONDBUTTON_DOWN,

        /// <summary>
        /// The second button (see <see cref="POINTER_FLAGS.SECONDBUTTON"/>) transitioned to a released state.
        /// </summary>
        SECONDBUTTON_UP,

        /// <summary>
        /// The third button (see <see cref="POINTER_FLAGS.THIRDBUTTON"/>) transitioned to a pressed state.
        /// </summary>
        THIRDBUTTON_DOWN,

        /// <summary>
        /// The third button (see <see cref="POINTER_FLAGS.THIRDBUTTON"/>) transitioned to a released state.
        /// </summary>
        THIRDBUTTON_UP,

        /// <summary>
        /// The fourth button (see <see cref="POINTER_FLAGS.FOURTHBUTTON"/>) transitioned to a pressed state.
        /// </summary>
        FOURTHBUTTON_DOWN,

        /// <summary>
        /// The fourth button (see <see cref="POINTER_FLAGS.FOURTHBUTTON"/>) transitioned to a released state.
        /// </summary>
        FOURTHBUTTON_UP,

        /// <summary>
        /// The fifth button (see <see cref="POINTER_FLAGS.FIFTHBUTTON"/>) transitioned to a pressed state.
        /// </summary>
        FIFTHBUTTON_DOWN,

        /// <summary>
        /// The fifth button (see <see cref="POINTER_FLAGS.FIFTHBUTTON"/>) transitioned to a released state.
        /// </summary>
        FIFTHBUTTON_UP
    }

    /// <summary>
    /// Values that can appear in the <see cref="POINTER_INFO.pointerFlags"/> field of the <see cref="POINTER_INFO"/> structure.
    /// </summary>
    /// <remarks>
    /// XBUTTON1 and XBUTTON2 are additional buttons used on many mouse devices. They return the same data as standard mouse buttons.
    /// </remarks>
    [Flags]
    public enum POINTER_FLAGS
    {
        /// <summary>
        /// Default
        /// </summary>
        NONE = 0x00000000,

        /// <summary>
        /// Indicates the arrival of a new pointer.
        /// </summary>
        NEW = 0x00000001,

        /// <summary>
        /// Indicates that this pointer continues to exist. When this flag is not set, it indicates the pointer has left detection range.
        /// </summary>
        /// <remarks>
        /// This flag is typically not set only when a hovering pointer leaves detection range (<see cref="UPDATE"/> is set) or when a pointer in contact with a window surface leaves detection range (<see cref="UP"/> is set).
        /// </remarks>
        INRANGE = 0x00000002,

        /// <summary>
        /// Indicates that this pointer is in contact with the digitizer surface. When this flag is not set, it indicates a hovering pointer.
        /// </summary>
        INCONTACT = 0x00000004,

        /// <summary>
        /// Indicates a primary action, analogous to a left mouse button down.
        /// </summary>
        /// <remarks>
        /// A touch pointer has this flag set when it is in contact with the digitizer surface.
        /// A pen pointer has this flag set when it is in contact with the digitizer surface with no buttons pressed.
        /// A mouse pointer has this flag set when the left mouse button is down.
        /// </remarks>
        FIRSTBUTTON = 0x00000010,

        /// <summary>
        /// Indicates a secondary action, analogous to a right mouse button down.
        /// </summary>
        /// <remarks>
        /// A touch pointer does not use this flag.
        /// A pen pointer has this flag set when it is in contact with the digitizer surface with the pen barrel button pressed.
        /// A mouse pointer has this flag set when the right mouse button is down.
        /// </remarks>
        SECONDBUTTON = 0x00000020,

        /// <summary>
        /// Analogous to a mouse wheel button down.
        /// </summary>
        /// <remarks>
        /// A touch pointer does not use this flag.
        /// A pen pointer does not use this flag.
        /// A mouse pointer has this flag set when the mouse wheel button is down.
        /// </remarks>
        THIRDBUTTON = 0x00000040,

        /// <summary>
        /// Analogous to a first extended mouse (XButton1) button down.
        /// </summary>
        /// <remarks>
        /// A touch pointer does not use this flag.
        /// A pen pointer does not use this flag.
        /// A mouse pointer has this flag set when the first extended mouse (XBUTTON1) button is down.
        /// </remarks>
        FOURTHBUTTON = 0x00000080,

        /// <summary>
        /// Analogous to a second extended mouse (XButton2) button down.
        /// </summary>
        /// <remarks>
        /// A touch pointer does not use this flag.
        /// A pen pointer does not use this flag.
        /// A mouse pointer has this flag set when the first extended mouse (XBUTTON1) button is down.
        /// </remarks>
        FIFTHBUTTON = 0x00000100,

        /// <summary>
        /// Indicates that this pointer has been designated as the primary pointer. A primary pointer is a single pointer that can perform actions beyond those available to non-primary pointers. For example, when a primary pointer makes contact with a window s surface, it may provide the window an opportunity to activate by sending it a WM_POINTERACTIVATE message.
        /// </summary>
        /// <remarks>
        /// The primary pointer is identified from all current user interactions on the system (mouse, touch, pen, and so on). As such, the primary pointer might not be associated with your app. The first contact in a multi-touch interaction is set as the primary pointer. Once a primary pointer is identified, all contacts must be lifted before a new contact can be identified as a primary pointer. For apps that don't process pointer input, only the primary pointer's events are promoted to mouse events.
        /// </remarks>
        PRIMARY = 0x00002000,

        /// <summary>
        /// Confidence is a suggestion from the source device about whether the pointer represents an intended or accidental interaction, which is especially relevant for PT_TOUCH pointers where an accidental interaction (such as with the palm of the hand) can trigger input. The presence of this flag indicates that the source device has high confidence that this input is part of an intended interaction.
        /// </summary>
        CONFIDENCE = 0x000004000,

        /// <summary>
        /// Indicates that the pointer is departing in an abnormal manner, such as when the system receives invalid input for the pointer or when a device with active pointers departs abruptly. If the application receiving the input is in a position to do so, it should treat the interaction as not completed and reverse any effects of the concerned pointer.
        /// </summary>
        CANCELED = 0x000008000,

        /// <summary>
        /// Indicates that this pointer transitioned to a down state; that is, it made contact with the digitizer surface.
        /// </summary>
        DOWN = 0x00010000,

        /// <summary>
        /// Indicates that this is a simple update that does not include pointer state changes.
        /// </summary>
        UPDATE = 0x00020000,

        /// <summary>
        /// Indicates that this pointer transitioned to an up state; that is, contact with the digitizer surface ended.
        /// </summary>
        UP = 0x00040000,

        /// <summary>
        /// Indicates input associated with a pointer wheel. For mouse pointers, this is equivalent to the action of the mouse scroll wheel (WM_MOUSEHWHEEL).
        /// </summary>
        WHEEL = 0x00080000,

        /// <summary>
        /// Indicates input associated with a pointer h-wheel. For mouse pointers, this is equivalent to the action of the mouse horizontal scroll wheel (WM_MOUSEHWHEEL).
        /// </summary>
        HWHEEL = 0x00100000,

        /// <summary>
        /// Indicates that this pointer was captured by (associated with) another element and the original element has lost capture (see WM_POINTERCAPTURECHANGED).
        /// </summary>
        CAPTURECHANGED = 0x00200000,

        /// <summary>
        /// Indicates that this pointer has an associated transform.
        /// </summary>
        HASTRANSFORM = 0x00400000,
    }

    /// <summary>
    /// Values that can appear in the <see cref="POINTER_PEN_INFO.penFlags"/> field of the <see cref="POINTER_PEN_INFO"/> structure.
    /// </summary>
    public enum PEN_FLAGS
    {
        /// <summary>
        /// There is no pen flag. This is the default.
        /// </summary>
        NONE = 0x00000000,

        /// <summary>
        /// The barrel button is pressed.
        /// </summary>
        BARREL = 0x00000001,

        /// <summary>
        /// The pen is inverted.
        /// </summary>
        INVERTED = 0x00000002,

        /// <summary>
        /// The eraser button is pressed.
        /// </summary>
        ERASER = 0x00000004
    }

    /// <summary>
    /// Values that can appear in the <see cref="POINTER_PEN_INFO.penMask"/> field of the <see cref="POINTER_PEN_INFO"/> structure.
    /// </summary>
    [Flags]
    public enum PEN_MASK
    {
        /// <summary>
        /// Default. None of the optional fields are valid.
        /// </summary>
        NONE = 0x00000000,

        /// <summary>
        /// <see cref="POINTER_PEN_INFO.pressure"/> of the <see cref="POINTER_PEN_INFO"/> structure is valid.
        /// </summary>
        PRESSURE = 0x00000001,

        /// <summary>
        /// <see cref="POINTER_PEN_INFO.rotation"/> of the <see cref="POINTER_PEN_INFO"/> structure is valid.
        /// </summary>
        ROTATION = 0x00000002,

        /// <summary>
        /// <see cref="POINTER_PEN_INFO.tiltX"/> of the <see cref="POINTER_PEN_INFO"/> structure is valid.
        /// </summary>
        TILT_X = 0x00000004,

        /// <summary>
        /// <see cref="POINTER_PEN_INFO.tiltY"/> of the <see cref="POINTER_PEN_INFO"/> structure is valid.
        /// </summary>
        TILT_Y = 0x00000008
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINTER_INFO
    {
        public POINTER_INPUT_TYPE pointerType;
        public uint pointerId;
        public uint frameId;
        public POINTER_FLAGS pointerFlags;
        public HANDLE sourceDevice;
        public HWND hwndTarget;
        public POINT ptPixelLocation;
        public POINT ptHimetricLocation;
        public POINT ptPixelLocationRaw;
        public POINT ptHimetricLocationRaw;
        public DWORD dwTime;
        public uint historyCount;
        public int InputData;
        public DWORD dwKeyStates;
        public ulong PerformanceCount;
        public POINTER_BUTTON_CHANGE_TYPE ButtonChangeType;
    }

    public struct POINTER_PEN_INFO
    {
        public POINTER_INFO pointerInfo;
        public PEN_FLAGS pointerFlags;
        public PEN_MASK penMask;
        public uint pressure;
        public uint rotation;
        public int tiltX;
        public int tiltY;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINTER_TYPE_INFO
    {
        public POINTER_INPUT_TYPE type;
        public POINTER_PEN_INFO penInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct POINTER_DEVICE_INFO
    {
        public DWORD displayOrientation;
        public void* device;
        public POINTER_DEVICE_TYPE pointerDeviceType;
        public void* monitor;
        public uint startingCursorId;
        public ushort maxActiveContacts;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 520)]
        public string productString;
    }

    public static class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr CreateSyntheticPointerDevice(POINTER_INPUT_TYPE pointerType,
                                                                           ulong maxCount, POINTER_FEEDBACK_MODE mode);

        [DllImport("user32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool InjectSyntheticPointerInput(IntPtr device, [In, MarshalAs(UnmanagedType.LPArray)] POINTER_TYPE_INFO[] pointerInfo, uint count);

        [DllImport("user32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static unsafe extern bool GetPointerDevices(out uint deviceCount, [In, Out, MarshalAs(UnmanagedType.LPArray), AllowNull] POINTER_DEVICE_INFO[] pointerDevices);

        [DllImport("user32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetForegroundWindow();
    }
}