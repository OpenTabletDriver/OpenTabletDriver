using System.Numerics;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop.Input.Absolute
{
    public class WindowsTouchPointer : IAbsolutePointer, IPressureHandler, ISynchronousPointer, ITiltHandler
    {
        private readonly TouchDevice _touchDevice;
        private bool _inContact;
        private bool _lastContact;
        private bool _dirty;

        public WindowsTouchPointer()
        {
            _touchDevice = new TouchDevice();
            _inContact = false;
            _lastContact = false;
        }

        public void Flush()
        {
            if (_dirty)
            {
                _touchDevice.Inject();
                _dirty = false;
            }
        }

        public void Reset()
        {
            _touchDevice.UnsetPointerFlags(POINTER_FLAGS.INRANGE);
        }

        public void SetPosition(Vector2 pos)
        {
            _dirty = true;
            _touchDevice.SetPointerFlags(POINTER_FLAGS.INRANGE);
            _touchDevice.SetPosition(new POINT((int)pos.X, (int)pos.Y));
            if (_inContact != _lastContact)
            {
                if (_inContact)
                {
                    _touchDevice.UnsetPointerFlags(POINTER_FLAGS.UP | POINTER_FLAGS.UPDATE);
                    _touchDevice.SetPointerFlags(POINTER_FLAGS.DOWN);
                    _lastContact = _inContact;
                }
                else
                {
                    _touchDevice.UnsetPointerFlags(POINTER_FLAGS.DOWN | POINTER_FLAGS.UPDATE);
                    _touchDevice.SetPointerFlags(POINTER_FLAGS.UP);
                    _lastContact = _inContact;
                }
            }
            else
            {
                _touchDevice.SetPointerFlags(POINTER_FLAGS.UPDATE);
            }
            _touchDevice.SetTarget();
        }

        public void SetPressure(float percentage)
        {
            var pressure = (uint)(percentage * 1024);
            if (pressure > 0)
            {
                _touchDevice.SetPressure(pressure);
                _touchDevice.SetPointerFlags(POINTER_FLAGS.INCONTACT | POINTER_FLAGS.FIRSTBUTTON);
                _inContact = true;
            }
            else
            {
                _touchDevice.SetPressure(1);
                _touchDevice.UnsetPointerFlags(POINTER_FLAGS.INCONTACT | POINTER_FLAGS.FIRSTBUTTON);
                _inContact = false;
            }
        }

        public void SetTilt(Vector2 tilt)
        {
            _touchDevice.SetTilt(tilt);
        }
    }
}
