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
    public abstract class RelativeOutputMode : OutputMode, IPointerProvider<IRelativePointer>
    {
        private Vector2? lastPos;
        private HPETDeltaStopwatch stopwatch = new HPETDeltaStopwatch(true);
        private bool skipReport;

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
        public TimeSpan ResetTime { set; get; }

        protected override Matrix3x2 CreateTransformationMatrix()
        {
            this.skipReport = true; // Prevents cursor from jumping on sensitivity change

            var transform = Matrix3x2.CreateRotation(
                (float)(-Rotation * System.Math.PI / 180));

            var digitizer = Tablet?.Properties.Specifications.Digitizer;
            return transform *= Matrix3x2.CreateScale(
                sensitivity.X * ((digitizer?.Width / digitizer?.MaxX) ?? 0.01f),
                sensitivity.Y * ((digitizer?.Height / digitizer?.MaxY) ?? 0.01f));
        }

        protected override IAbsolutePositionReport Transform(IAbsolutePositionReport report)
        {
            var deltaTime = stopwatch.Restart();

            var pos = Vector2.Transform(report.Position, this.TransformationMatrix);
            var delta = pos - this.lastPos;

            this.lastPos = pos;
            report.Position = deltaTime < ResetTime ? delta.GetValueOrDefault() : Vector2.Zero;

            if (skipReport)
            {
                skipReport = false;
                return null;
            }

            return report;
        }

        protected override void OnOutput(IDeviceReport report)
        {
            if (report is IAbsolutePositionReport absReport)
            {
                Pointer.Translate(absReport.Position);
            }
        }
    }
}
