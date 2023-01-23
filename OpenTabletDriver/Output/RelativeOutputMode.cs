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

        private Vector2? _lastPos;
        private bool _skipReport;

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
            _skipReport = true; // Prevents cursor from jumping on sensitivity change

            var transform = Matrix3x2.CreateRotation(
                (float)(-Rotation * Math.PI / 180));

            var digitizer = Tablet?.Configuration.Specifications.Digitizer;
            return transform *= Matrix3x2.CreateScale(
                _sensitivity.X * ((digitizer?.Width / digitizer?.MaxX) ?? 0.01f),
                _sensitivity.Y * ((digitizer?.Height / digitizer?.MaxY) ?? 0.01f));
        }

        protected override IAbsolutePositionReport? Transform(IAbsolutePositionReport report)
        {
            var deltaTime = _stopwatch.Restart();

            var pos = Vector2.Transform(report.Position, TransformationMatrix);
            var delta = pos - _lastPos;

            _lastPos = pos;
            report.Position = deltaTime < ResetDelay ? delta.GetValueOrDefault() : Vector2.Zero;

            if (_skipReport)
            {
                _skipReport = false;
                return null;
            }

            return report;
        }

        protected override void OnOutput(IDeviceReport report)
        {
            if (report is IEraserReport eraserReport && Pointer is IEraserHandler eraserHandler)
                eraserHandler.SetEraser(eraserReport.Eraser);
            if (report is IAbsolutePositionReport absReport)
                Pointer.SetPosition(absReport.Position);
            if (report is ITabletReport tabletReport && Pointer is IPressureHandler pressureHandler)
                pressureHandler.SetPressure(tabletReport.Pressure / (float)Tablet.Configuration.Specifications.Pen!.MaxPressure);
            if (report is ITiltReport tiltReport && Pointer is ITiltHandler tiltHandler)
                tiltHandler.SetTilt(tiltReport.Tilt);
            if (report is IHoverReport proximityReport && Pointer is IHoverDistanceHandler hoverDistanceHandler)
                hoverDistanceHandler.SetHoverDistance(proximityReport.HoverDistance);
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
