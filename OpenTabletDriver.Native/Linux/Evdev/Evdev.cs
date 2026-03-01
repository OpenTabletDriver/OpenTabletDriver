using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using OpenTabletDriver.Native.Linux.Evdev.Structs;

#nullable enable

namespace OpenTabletDriver.Native.Linux.Evdev
{
    /// <summary>
    /// Linux Evdev/uinput device handling
    /// <para/>
    /// Usage:<br/>
    /// 1. <see cref="libevdev_new"/><br/>
    /// 2. <see cref="libevdev_set_name"/><br/>
    /// 3. Enable any needed from:<br/>
    /// - <see cref="libevdev_enable_event_code(EvdevHandle, EventType, EventCode)"/><br/>
    /// - <see cref="libevdev_enable_event_code_abs"/><br/>
    /// - <see cref="libevdev_enable_event_code_rep"/><br/>
    /// - <see cref="libevdev_enable_event_type"/> (optionally, enabling event codes auto-enables event types)<br/>
    /// - <see cref="libevdev_enable_property"/>.<para/>
    /// Then <see cref="libevdev_uinput_create_from_device"/>
    /// into as many
    /// <see cref="libevdev_uinput_write_event"/> as you need
    /// <para/>
    /// Upstream documentation:<br/>
    /// <a href="https://www.freedesktop.org/software/libevdev/doc/latest/group__init.html">Freedesktop.org libevdev docs: evdev init</a><br/>
    /// <a href="https://www.freedesktop.org/software/libevdev/doc/latest/group__kernel.html">Freedesktop.org libevdev docs: evdev device modification</a><br/>
    /// <a href="https://www.freedesktop.org/software/libevdev/doc/latest/group__uinput.html">Freedesktop.org libevdev docs: uinput</a><br/>
    /// <a href="https://www.freedesktop.org/software/libevdev/doc/latest/modules.html">See all Freedesktop.org libevdev doc modules</a><br/>
    /// </summary>
    internal static partial class Evdev
    {
        internal class EvdevHandle() : SafeHandleZeroOrMinusOneIsInvalid(true)
        {
            protected override bool ReleaseHandle()
            {
                libevdev_free(handle);
                return true;
            }
        }

        internal class EvdevUinputHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private readonly EvdevHandle _evdevHandle;

            /// <remarks>
            /// You must still manually dispose <paramref name="evdevHandle"/>
            /// </remarks>
            public EvdevUinputHandle(IntPtr uinputPtr, EvdevHandle evdevHandle) : base(true)
            {
                base.handle = uinputPtr;
                _evdevHandle = evdevHandle;
            }

            public override bool IsInvalid => _evdevHandle.IsInvalid || base.IsInvalid;

            protected override bool ReleaseHandle()
            {
                libevdev_uinput_destroy(handle);
                return true;
            }
        }

        private const string libevdev = "libevdev.so.2";

        /// <summary>
        /// Creates a new evdev handle
        /// </summary>
        /// <returns>A disposable handle</returns>
        [LibraryImport(libevdev)]
        internal static partial EvdevHandle libevdev_new();

        /// <summary>
        /// Sets a new name for the evdev device
        /// </summary>
        /// <param name="evdevHandle">A valid handle for the device from <see cref="libevdev_new"/></param>
        /// <param name="name">The name of the device</param>
        /// <remarks>The <paramref name="evdevHandle"/> is not validated</remarks>
        [LibraryImport(libevdev)]
        internal static partial void libevdev_set_name(EvdevHandle evdevHandle, [MarshalAs(UnmanagedType.LPStr)] string name);

        /// <summary>
        /// Enable property for evdev device
        /// </summary>
        /// <param name="evdevHandle">A valid handle for the device from <see cref="libevdev_new"/></param>
        /// <param name="prop">A single property of <see cref="InputProperty"/></param>
        /// <returns><c>0</c> on success, or <c>-1</c> on failure</returns>
        /// <remarks>The <paramref name="evdevHandle"/> is not validated</remarks>
        [LibraryImport(libevdev)]
        internal static partial int libevdev_enable_property(EvdevHandle evdevHandle, InputProperty prop);

        /// <summary>
        /// Enable type for device
        /// </summary>
        /// <param name="evdevHandle">A valid handle for the device from <see cref="libevdev_new"/></param>
        /// <param name="type">The <see cref="EventType"/> to enable</param>
        /// <returns><c>0</c> on success, or <c>-1</c> on failure</returns>
        /// <remarks>
        /// Upstream documentation indirectly suggests to instead enable types via
        /// <see cref="libevdev_enable_event_code_abs"/> or <see cref="libevdev_enable_event_code(EvdevHandle,EventType,EventCode)"/>
        /// The <paramref name="evdevHandle"/> is not validated
        /// </remarks>
        [LibraryImport(libevdev)]
        internal static partial int libevdev_enable_event_type(EvdevHandle evdevHandle, EventType type);

        /// <summary>
        /// Private function for the external call to enable the event code for the device.
        /// Please use the appropriate <c>internal</c> function instead (as seen in the Remarks section).
        /// </summary>
        /// <param name="evdevHandle">A valid handle for the device from <see cref="libevdev_new"/></param>
        /// <param name="type">The <see cref="EventType"/> that the <paramref name="code"/> refers to</param>
        /// <param name="code">The <see cref="EventCode"/> to enable</param>
        /// <param name="data">A pointer to the relevant data</param>
        /// <returns><c>0</c> on success, or <c>-1</c> on failure</returns>
        /// <remarks>
        /// Upstream library automatically enables the <see cref="EventType"/> if necessary<br/>
        /// The <paramref name="evdevHandle"/> is not validated<para/>
        /// Notes on <paramref name="type"/> and <paramref name="data"/> relations: <br/>
        /// - <see cref="EventType.EV_ABS"/> must use <see cref="input_absinfo"/> (<see cref="libevdev_enable_event_code_abs"/>) <br/>
        /// - <see cref="EventType.EV_REP"/> must use <see cref="int"/> (<see cref="libevdev_enable_event_code_rep"/>) <br/>
        /// - Otherwise <paramref name="data"/> must be <c>null</c> (<see cref="libevdev_enable_event_code(EvdevHandle,EventType,EventCode)"/>)
        /// </remarks>
        [LibraryImport(libevdev)]
        private static partial int libevdev_enable_event_code(EvdevHandle evdevHandle, EventType type, EventCode code, IntPtr data);

        /// <summary>
        /// Enable an <see cref="EventCode"/> of type <see cref="EventType.EV_ABS"/>
        /// </summary>
        /// <param name="evdevHandle">A valid handle for the device from <see cref="libevdev_new"/></param>
        /// <param name="code">The <see cref="EventCode"/> to enable</param>
        /// <param name="absinfo">An <see cref="input_absinfo"/> struct containing the ranges for the <paramref name="code"/> you're enabling</param>
        /// <returns><c>0</c> on success, or <c>-1</c> on failure</returns>
        /// <exception cref="ArgumentException">On null or invalid <paramref name="evdevHandle"/></exception>
        /// <remarks>
        /// Automatically enables event type <see cref="EventType.EV_ABS"/> if it wasn't previously enabled
        /// </remarks>
        internal static unsafe int libevdev_enable_event_code_abs(EvdevHandle evdevHandle, EventCode code, input_absinfo absinfo)
        {
            ValidateHandle(evdevHandle);

            return libevdev_enable_event_code(evdevHandle, EventType.EV_ABS, code, (IntPtr)(&absinfo));
        }

        /// <summary>
        /// Enable an <see cref="EventCode"/> of type <see cref="EventType.EV_REP"/>
        /// </summary>
        /// <param name="evdevHandle">A valid handle for the device from <see cref="libevdev_new"/></param>
        /// <param name="code">The <see cref="EventCode"/> to enable</param>
        /// <param name="data">"The data for the axis" (?)</param>
        /// <returns><c>0</c> on success, or <c>-1</c> on failure</returns>
        /// <exception cref="ArgumentException">On null or invalid <paramref name="evdevHandle"/></exception>
        /// <remarks>
        /// Untested in the OpenTabletDriver codebase.<br/>
        /// Automatically enables event type <see cref="EventType.EV_REP"/> if it wasn't previously enabled
        /// </remarks>
        internal static int libevdev_enable_event_code_rep(EvdevHandle evdevHandle, EventCode code, int data)
        {
            ValidateHandle(evdevHandle);

            return libevdev_enable_event_code(evdevHandle, EventType.EV_REP, code, data);
        }

        /// <summary>
        /// Enable an <see cref="EventCode"/> not of type <see cref="EventType.EV_ABS"/> or <see cref="EventType.EV_REP"/>
        /// </summary>
        /// <param name="evdevHandle">A valid handle for the device from <see cref="libevdev_new"/></param>
        /// <param name="type">The <see cref="EventType"/> that the <paramref name="code"/> refers to</param>
        /// <param name="code">The <see cref="EventCode"/> to enable</param>
        /// <returns><c>0</c> on success, or <c>-1</c> on failure</returns>
        /// <exception cref="ArgumentException">On null or invalid <paramref name="evdevHandle"/></exception>
        /// <remarks>
        /// Automatically enables the <paramref name="type"/> if it wasn't previously enabled.<br/>
        /// For <see cref="EventType.EV_ABS"/> or <see cref="EventType.EV_REP"/> events, use the respective function instead:
        /// <see cref="libevdev_enable_event_code_abs"/> or <see cref="libevdev_enable_event_code_rep"/>
        /// </remarks>
        internal static int libevdev_enable_event_code(EvdevHandle evdevHandle, EventType type, EventCode code)
        {
            Debug.Assert(type != EventType.EV_ABS,
                $"{nameof(EventType.EV_ABS)} usage requires {nameof(input_absinfo)}. Use the {nameof(libevdev_enable_event_code_abs)} function instead");
            Debug.Assert(type != EventType.EV_REP,
                $"{nameof(EventType.EV_REP)} usage requires 'int'. Use the {nameof(libevdev_enable_event_code_rep)} function instead");

            ValidateHandle(evdevHandle);

            return libevdev_enable_event_code(evdevHandle, type, code, IntPtr.Zero);
        }

        /// <summary>
        /// Backed by a wrapper function as <see cref="EvdevUinputHandle"/> cannot easily be marshaled
        /// </summary>
        [LibraryImport(libevdev, EntryPoint = "libevdev_uinput_create_from_device")]
        private static partial int sys_libevdev_uinput_create_from_device(EvdevHandle evdevHandle, int uinput_fd, out IntPtr uinput_dev);

        /// <summary>
        /// Create an uinput device from an evdev device to send events from via <see cref="libevdev_uinput_write_event"/>
        /// </summary>
        /// <param name="evdevHandle">A valid handle for the device from <see cref="libevdev_new"/></param>
        /// <param name="uinput_dev">On success, a valid <see cref="EvdevUinputHandle"/></param>
        /// <returns><see cref="ERRNO.NONE"/> on success. On failure, the value of <paramref name="uinput_dev"/> is unmodified.</returns>
        /// <exception cref="ArgumentException">On null or invalid <paramref name="evdevHandle"/></exception>
        internal static ERRNO libevdev_uinput_create_from_device(EvdevHandle evdevHandle, out EvdevUinputHandle uinput_dev)
        {
            ValidateHandle(evdevHandle);

            int rv = sys_libevdev_uinput_create_from_device(evdevHandle, _LIBEVDEV_UINPUT_OPEN_MANAGED, out var uinputDevPtr);
            uinput_dev = new EvdevUinputHandle(uinputDevPtr, evdevHandle);
            return (ERRNO)(-rv);
        }

        [LibraryImport(libevdev, EntryPoint = "libevdev_uinput_write_event")]
        internal static partial int sys_libevdev_uinput_write_event(EvdevUinputHandle uinputHandle, EventType type, EventCode code, int value);

        /// <summary>
        /// Send an event via the created uinput device
        /// </summary>
        /// <param name="uinputHandle">A valid handle for the device from <see cref="libevdev_uinput_create_from_device"/></param>
        /// <param name="type">The previously enabled <see cref="EventType"/></param>
        /// <param name="code">The previously enabled <see cref="EventCode"/></param>
        /// <param name="value">The event value</param>
        /// <returns><see cref="ERRNO.NONE"/> on success</returns>
        internal static ERRNO libevdev_uinput_write_event(EvdevUinputHandle uinputHandle, EventType type, EventCode code, int value)
        {
            ValidateHandle(uinputHandle);

            return (ERRNO)(-sys_libevdev_uinput_write_event(uinputHandle, type, code, value));
        }

        private const int _LIBEVDEV_UINPUT_OPEN_MANAGED = -2;

        [LibraryImport(libevdev)]
        private static partial void libevdev_free(IntPtr dev);

        [LibraryImport(libevdev)]
        private static partial void libevdev_uinput_destroy(IntPtr uinputDev);

        private static void ValidateHandle(SafeHandle handle, [CallerArgumentExpression(nameof(handle))] string argName = "")
        {
            if (handle == null || handle.IsInvalid) throw new ArgumentException("Invalid handle", argName);
        }
    }
}
