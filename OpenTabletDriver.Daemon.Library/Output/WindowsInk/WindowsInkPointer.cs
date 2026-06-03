using System;
using System.Numerics;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Daemon.Library.Output.WindowsInk
{
    internal unsafe class WindowsInkPointer : IAbsolutePointer, IPressureHandler, ITiltHandler, IEraserHandler, ISynchronousPointer
    {
        private readonly Vector2 _conversionFactor;
        private readonly IVirtualScreen _screen;
        private readonly VMultiInstance<DigitizerInputReport> _instance;
        private readonly DigitizerInputReport* _rawPointer;
        private ThinOsPointer? _osPointer;
        private Vector2 _internalPosition;
        private Vector2 _previousPosition;
        private bool _dirty;
        private bool _isEraser;

        public WindowsInkPointer(IVirtualScreen screen)
        {
            _screen = screen;
            _conversionFactor = new Vector2(32767, 32767) / new Vector2(screen.Width, screen.Height);
            _instance = new VMultiInstance<DigitizerInputReport>("Windows Ink", extended => extended ? DigitizerInputReport.Extended() : DigitizerInputReport.Normal());
            _rawPointer = _instance.Pointer;

            if (_instance.Extended)
                Log.Write("Windows Ink", "Using extended VMulti digitizer");
        }

        public bool Sync
        {
            set => _osPointer = value ? new ThinOsPointer(_screen) : null;
        }

        public bool ForcedSync { get; set; }

        public void SetPosition(Vector2 position)
        {
            if (position == _previousPosition)
                return;

            _internalPosition = position;
            _instance.EnableButtonBit((int)WindowsInkButtonFlags.InRange);
            var convertedPosition = Convert(position);
            _rawPointer->X = (ushort)Math.Clamp(convertedPosition.X, 0, 32767);
            _rawPointer->Y = (ushort)Math.Clamp(convertedPosition.Y, 0, 32767);
            _dirty = true;
            _previousPosition = position;
        }

        public void SetPressure(float percentage)
        {
            var maximumPressure = _instance.Extended ? 16383 : 8191;
            _rawPointer->Pressure = (ushort)Math.Clamp(percentage * maximumPressure, 0, maximumPressure);
            _dirty = true;
        }

        public void SetTilt(Vector2 tilt)
        {
            _rawPointer->XTilt = unchecked((byte)Math.Clamp((int)tilt.X, -127, 127));
            _rawPointer->YTilt = unchecked((byte)Math.Clamp((int)tilt.Y, -127, 127));
            _dirty = true;
        }

        public void SetEraser(bool isEraser)
        {
            if (_isEraser == isEraser)
                return;

            TransitionEraserState(isEraser);
            _isEraser = isEraser;
            _dirty = true;
        }

        public void MouseDown(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    _instance.EnableButtonBit((int)(_isEraser ? WindowsInkButtonFlags.Eraser : WindowsInkButtonFlags.Press));
                    break;
                case MouseButton.Right:
                case MouseButton.Middle:
                case MouseButton.Backward:
                case MouseButton.Forward:
                    _instance.EnableButtonBit((int)WindowsInkButtonFlags.Barrel);
                    break;
            }
            _instance.Write();
        }

        public void MouseUp(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    _instance.DisableButtonBit((int)(WindowsInkButtonFlags.Press | WindowsInkButtonFlags.Eraser));
                    _rawPointer->Pressure = 0;
                    break;
                case MouseButton.Right:
                case MouseButton.Middle:
                case MouseButton.Backward:
                case MouseButton.Forward:
                    _instance.DisableButtonBit((int)WindowsInkButtonFlags.Barrel);
                    break;
            }
            _instance.Write();
        }

        public void Reset()
        {
            if (_osPointer is not null && !ForcedSync)
                SyncOsCursor();

            _instance.DisableButtonBit((int)(WindowsInkButtonFlags.Press | WindowsInkButtonFlags.Eraser | WindowsInkButtonFlags.Barrel | WindowsInkButtonFlags.InRange | WindowsInkButtonFlags.Invert));
            _rawPointer->Pressure = 0;
            _instance.Write();
        }

        public void Flush()
        {
            if (!_dirty)
                return;

            _dirty = false;
            if (ForcedSync)
                SyncOsCursor();
            _instance.Write();
        }

        private Vector2 Convert(Vector2 position)
        {
            return position * _conversionFactor;
        }

        private void SyncOsCursor()
        {
            _osPointer?.SetPosition(_internalPosition);
        }

        private void TransitionEraserState(bool isEraser)
        {
            var buttons = _rawPointer->Header.Buttons;
            var pressure = _rawPointer->Pressure;

            _instance.DisableButtonBit((int)(WindowsInkButtonFlags.Press | WindowsInkButtonFlags.Eraser));
            _rawPointer->Pressure = 0;
            _instance.Write();

            _rawPointer->Header.Buttons = 0;
            _instance.Write();

            _instance.EnableButtonBit((int)WindowsInkButtonFlags.InRange);
            if (isEraser)
                _instance.EnableButtonBit((int)WindowsInkButtonFlags.Invert);
            else
                _instance.DisableButtonBit((int)WindowsInkButtonFlags.Invert);
            _instance.Write();

            if (VMultiInstance.HasBit(buttons, (int)(WindowsInkButtonFlags.Press | WindowsInkButtonFlags.Eraser)))
                _instance.EnableButtonBit((int)(isEraser ? WindowsInkButtonFlags.Eraser : WindowsInkButtonFlags.Press));
            _rawPointer->Pressure = pressure;
        }
    }
}
