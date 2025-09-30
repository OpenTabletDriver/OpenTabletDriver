using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Desktop.Interop.Input.Absolute
{
    public class PointerDevice
    {
        private readonly IntPtr _penHandle;
        private readonly POINTER_TYPE_INFO[] pointer;
        private readonly IntPtr _sourceDevice;

        public unsafe PointerDevice()
        {
            var _pointerInfo = new POINTER_INFO
            {
                pointerType = POINTER_INPUT_TYPE.PT_PEN,
                pointerId = 1,
                frameId = 0,
                pointerFlags = POINTER_FLAGS.NONE,
                sourceDevice = _sourceDevice,
                ptPixelLocation = new POINT(),
                ptPixelLocationRaw = new POINT(),
                dwTime = 0,
                historyCount = 0,
                dwKeyStates = 0,
                PerformanceCount = 0,
                ButtonChangeType = POINTER_BUTTON_CHANGE_TYPE.NONE
            };

            var _penInfo = new POINTER_PEN_INFO
            {
                pointerInfo = _pointerInfo,
                pointerFlags = PEN_FLAGS.NONE,
                penMask = PEN_MASK.PRESSURE | PEN_MASK.TILT_X | PEN_MASK.TILT_Y,
                pressure = 0, // 0 to 1024
                rotation = 0, // 0 to 359 (degrees clockwise)
                tiltX = 0, // -90 to +90
                tiltY = 0 // -90 to +90
            };

            pointer = new POINTER_TYPE_INFO[]
            {
                new POINTER_TYPE_INFO
                {
                    type = POINTER_INPUT_TYPE.PT_PEN,
                    penInfo = _penInfo
                }
            };

            // Retrieve handle to custom pen
            _penHandle = NativeMethods.CreateSyntheticPointerDevice(POINTER_INPUT_TYPE.PT_PEN, 1, POINTER_FEEDBACK_MODE.INDIRECT);
            var err = Marshal.GetLastWin32Error();
            if (err < 0 || _penHandle == IntPtr.Zero)
                throw new Exception("Failed to create handle.");

            // Notify WindowsInk
            ClearPointerFlags(POINTER_FLAGS.NEW);
            Inject();

            // Back to normal state
            ClearPointerFlags(POINTER_FLAGS.PRIMARY);
        }

        public void Inject()
        {
            if (!NativeMethods.InjectSyntheticPointerInput(_penHandle, pointer!, 1))
            {
                throw new Exception($"Input injection failed. Reason: {Marshal.GetLastWin32Error()}");
            }
        }

        public void SetTarget()
        {
            pointer![0].penInfo.pointerInfo.hwndTarget = NativeMethods.GetForegroundWindow();
        }

        public void SetPosition(POINT point)
        {
            pointer![0].penInfo.pointerInfo.ptPixelLocation = point;
            pointer[0].penInfo.pointerInfo.ptPixelLocationRaw = point;
        }

        public void SetPressure(uint pressure)
        {
            pointer![0].penInfo.pressure = pressure;
        }

        public void SetTilt(Vector2 tilt)
        {
            pointer![0].penInfo.tiltX = (int)tilt.X;
            pointer![0].penInfo.tiltY = (int)tilt.Y;
        }

        public void SetPointerFlags(POINTER_FLAGS flags)
        {
            pointer![0].penInfo.pointerInfo.pointerFlags |= flags;
        }

        public void UnsetPointerFlags(POINTER_FLAGS flags)
        {
            pointer![0].penInfo.pointerInfo.pointerFlags &= ~flags;
        }

        public void SetPenFlags(PEN_FLAGS flags)
        {
            pointer![0].penInfo.pointerFlags |= flags;
        }

        public void UnsetPenFlags(PEN_FLAGS flags)
        {
            pointer![0].penInfo.pointerFlags &= ~flags;
        }

        public void ClearPointerFlags()
        {
            pointer![0].penInfo.pointerInfo.pointerFlags = 0;
        }

        public void ClearPointerFlags(POINTER_FLAGS flags)
        {
            pointer![0].penInfo.pointerInfo.pointerFlags = flags;
        }
    }
}
