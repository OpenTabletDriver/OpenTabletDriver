using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Timing;

namespace OpenTabletDriver.Plugin.Output
{
    /// <summary>
    /// A relatively positioned output mode.
    /// </summary>
    [PluginIgnore]
    public abstract class RelativeOutputMode : OutputMode
    {
        private HPETDeltaStopwatch stopwatch = new HPETDeltaStopwatch(true);
        private Vector2? lastTransformedPos;
        private Vector2 lastReadPos;
        private bool outOfRange;

        // for handling detection of low resetTimes
        private uint _resets;
        private bool _warnedBadResets = false;

        /// <summary>
        /// The class in which the final relative positioned output is handled.
        /// </summary>
        public abstract IRelativePointer Pointer { set; get; }

        private Vector2 sensitivity;

        /// <summary>
        /// The sensitivity vector in which input will be transformed.
        /// <remarks>
        /// This sensitivity is in mm/px.
        /// </remarks>
        /// </summary>
        public Vector2 Sensitivity
        {
            set
            {
                this.sensitivity = value;
                this.TransformationMatrix = CreateTransformationMatrix();
            }
            get => this.sensitivity;
        }

        private float rotation;
        private TimeSpan _resetTime;

        /// <summary>
        /// The angle of rotation to be applied to the input.
        /// </summary>
        public float Rotation
        {
            set
            {
                this.rotation = value;
                this.TransformationMatrix = CreateTransformationMatrix();
            }
            get => this.rotation;
        }

        /// <summary>
        /// The delay in which to reset the last known position in relative positioning.
        /// </summary>
        public TimeSpan ResetTime
        {
            set
            {
                _resetTime = value;
                _resets = 0;
                _warnedBadResets = false;
            }
            get => _resetTime;
        }

        protected override Matrix3x2 CreateTransformationMatrix()
        {
            var transform = Matrix3x2.CreateRotation(
                (float)(-Rotation * System.Math.PI / 180));

            var digitizer = Tablet?.Properties.Specifications.Digitizer;
            return transform *= Matrix3x2.CreateScale(
                sensitivity.X * ((digitizer?.Width / digitizer?.MaxX) ?? 0.01f),
                sensitivity.Y * ((digitizer?.Height / digitizer?.MaxY) ?? 0.01f));
        }

        public override void Read(IDeviceReport deviceReport)
        {
            // intercept positional reports
            if (deviceReport is IAbsolutePositionReport report)
            {
                var deltaTime = stopwatch.Restart();

                // reset origin when exceeding the reset delay
                if (deltaTime > ResetTime)
                {
                    outOfRange = true;
                    lastTransformedPos = null;
                    _resets++;
                }
                else _resets = 0;

                if (!_warnedBadResets &&
                    (_warnedBadResets =
                        _resets > 10))
                    Log.Write("RelativeOutputMode",
                        $"Position reset spam detected - the configured reset time ({ResetTime.TotalMilliseconds} ms) is likely too low",
                        LogLevel.Warning);

                // skip duplicate reports sent by tablets right after going into
                // range from an out of range state.
                if (outOfRange && report.Position == lastReadPos)
                    return;

                outOfRange = false;
                lastReadPos = report.Position;
            }
            else if (deviceReport is OutOfRangeReport)
            {
                outOfRange = true;
            }

            base.Read(deviceReport);
        }

        protected override IAbsolutePositionReport Transform(IAbsolutePositionReport report)
        {
            var pos = Vector2.Transform(report.Position, TransformationMatrix);
            var delta = pos - lastTransformedPos;

            lastTransformedPos = pos;
            report.Position = delta.GetValueOrDefault();

            return report;
        }

        protected override void OnOutput(IDeviceReport report)
        {
            // this should be ordered from least to most chance of having a
            // dependency to another pointer property.
            if (report is IProximityReport proximityReport && Pointer is IHoverDistanceHandler hoverDistanceHandler)
                hoverDistanceHandler.SetHoverDistance(proximityReport.HoverDistance);
            if (report is IEraserReport eraserReport && Pointer is IEraserHandler eraserHandler)
                eraserHandler.SetEraser(eraserReport.Eraser);
            if (report is ITiltReport tiltReport && Pointer is ITiltHandler tiltHandler)
                tiltHandler.SetTilt(tiltReport.Tilt);
            if (report is ITabletReport tabletReport && Pointer is IPressureHandler pressureHandler
                && Tablet?.Properties.Specifications.Pen != null)
                pressureHandler.SetPressure(tabletReport.Pressure / (float)Tablet.Properties.Specifications.Pen.MaxPressure);

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
    }
}
