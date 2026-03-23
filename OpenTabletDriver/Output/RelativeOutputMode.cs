using System;
using System.ComponentModel;
using System.Numerics;
using JetBrains.Annotations;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Platform.Pointer;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Output
{
    /// <summary>
    /// A relative positioned output mode.
    /// </summary>
    [PublicAPI]
    [PluginInterface]
    public abstract class RelativeOutputMode : OutputMode
    {
        private readonly HPETDeltaStopwatch _stopwatch = new HPETDeltaStopwatch();

        protected RelativeOutputMode(InputDevice tablet, IRelativePointer relativePointer)
            : base(tablet)
        {
            Pointer = relativePointer;
        }

        private Vector2? _lastTransformedPos;
        private Vector2 _lastReadPos;
        private bool _outOfRange;

        /// <summary>
        /// The class in which the final relative positioned output is handled.
        /// </summary>
        public IRelativePointer Pointer { get; }

        private Vector2 _sensitivity;

        /// <summary>
        /// The sensitivity vector in which input will be transformed.
        /// <remarks>
        /// This sensitivity is in mm/px.
        /// </remarks>
        /// </summary>
        [Setting("Sensitivity"), Unit("px/mm"), MemberSourcedDefaults(nameof(GetDefaultSensitivity))]
        public Vector2 Sensitivity
        {
            set
            {
                _sensitivity = value;
                TransformationMatrix = CreateTransformationMatrix();
            }
            get => _sensitivity;
        }

        private float _rotation;

        /// <summary>
        /// The angle of rotation to be applied to the input.
        /// </summary>
        [Setting("Rotation"), Unit("°"), DefaultValue(0f)]
        public float Rotation
        {
            set
            {
                _rotation = value;
                TransformationMatrix = CreateTransformationMatrix();
            }
            get => _rotation;
        }

        /// <summary>
        /// The delay in which to reset the last known position in relative positioning.
        /// </summary>
        [Setting("Reset Delay"), MemberSourcedDefaults(nameof(GetDefaultResetDelay))]
        public TimeSpan ResetDelay { set; get; }

        protected override Matrix3x2 CreateTransformationMatrix()
        {
            var transform = Matrix3x2.CreateRotation(
                (float)(-Rotation * Math.PI / 180));

            var digitizer = Tablet?.Configuration.Specifications.Digitizer;
            return transform *= Matrix3x2.CreateScale(
                _sensitivity.X * ((digitizer?.Width / digitizer?.MaxX) ?? 0.01f),
                _sensitivity.Y * ((digitizer?.Height / digitizer?.MaxY) ?? 0.01f));
        }

        public override void Read(IDeviceReport deviceReport)
        {
            // intercept positional reports
            if (deviceReport is IAbsolutePositionReport report)
            {
                var deltaTime = _stopwatch.Restart();

                // reset origin when exceeding the reset delay
                if (deltaTime > ResetDelay)
                {
                    _outOfRange = true;
                    _lastTransformedPos = null;
                }

                // skip duplicate reports sent by tablets right after going into
                // range from an out of range state.
                if (_outOfRange && report.Position == _lastReadPos)
                    return;

                _outOfRange = false;
                _lastReadPos = report.Position;
            }
            else if (deviceReport is OutOfRangeReport)
            {
                _outOfRange = true;
            }

            base.Read(deviceReport);
        }

        protected override IAbsolutePositionReport Transform(IAbsolutePositionReport report)
        {
            var pos = Vector2.Transform(report.Position, TransformationMatrix);
            var delta = pos - _lastTransformedPos;

            _lastTransformedPos = pos;
            report.Position = delta.GetValueOrDefault();

            return report;
        }

        protected override void OnOutput(IDeviceReport report)
        {
            // this should be ordered from least to most chance of having a
            // dependency to another pointer property. for example, proximity
            // should be set before position, because in LinuxArtistMode
            // the SetPosition method is dependent on the proximity state.
            if (report is IHoverReport proximityReport && Pointer is IHoverDistanceHandler hoverDistanceHandler)
                hoverDistanceHandler.SetHoverDistance(proximityReport.HoverDistance);
            if (report is IEraserReport eraserReport && Pointer is IEraserHandler eraserHandler)
                eraserHandler.SetEraser(eraserReport.Eraser);
            if (report is ITiltReport tiltReport && Pointer is ITiltHandler tiltHandler)
                tiltHandler.SetTilt(tiltReport.Tilt);
            if (report is ITabletReport tabletReport && Pointer is IPressureHandler pressureHandler)
                pressureHandler.SetPressure(tabletReport.Pressure / (float)Tablet.Configuration.Specifications.Pen!.MaxPressure);

            // make sure to set the position last
            if (report is IAbsolutePositionReport absReport)
                Pointer.SetPosition(absReport.Position);
            if (Pointer is ISynchronousPointer synchronousPointer)
            {
                if (report is OutOfRangeReport)
                    synchronousPointer.Reset();
                synchronousPointer.Flush();
            }
        }

        public static Vector2 GetDefaultSensitivity()
        {
            return new Vector2(10, 10);
        }

        public static TimeSpan GetDefaultResetDelay()
        {
            return TimeSpan.FromMilliseconds(500);
        }
    }
}
