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
    public abstract class RelativeOutputMode : OutputMode<IRelativePointer>
    {
        private HPETDeltaStopwatch stopwatch = new HPETDeltaStopwatch();
        private Vector2? lastTransformedPos;
        private Vector2 lastReadPos;
        private bool outOfRange;

        // for handling detection of low resetTimes
        private uint _resets;
        private bool _warnedBadResets;

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

        public override void Read(scoped ref DeviceReport deviceReport)
        {
            if (deviceReport.OutOfRange)
            {
                outOfRange = true;
            }
            // intercept positional reports
            else if (deviceReport.PenPosition.HasValue)
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
                    Log.WriteNotify("RelativeOutputMode",
                        $"Position reset spam detected - the configured reset time ({ResetTime.TotalMilliseconds} ms) is likely too low",
                        LogLevel.Warning);

                // skip duplicate reports sent by tablets right after going into
                // range from an out of range state.
                if (outOfRange && deviceReport.PenPosition.Value == lastReadPos)
                    return;

                outOfRange = false;
                lastReadPos = deviceReport.PenPosition.Value;
            }

            base.Read(ref deviceReport);
        }

        protected override bool Transform(scoped ref DeviceReport report)
        {
            if (!report.PenPosition.HasValue) return false;

            var pos = Vector2.Transform(report.PenPosition.Value, TransformationMatrix);
            var delta = pos - lastTransformedPos;

            lastTransformedPos = pos;
            report.PenPosition = delta.GetValueOrDefault();

            return true;
        }

        protected override void OnOutputPreSync(scoped ref DeviceReport report)
        {
            if (Pointer != null && report.PenPosition.HasValue)
                Pointer.SetPosition(report.PenPosition.Value);
        }
    }
}
