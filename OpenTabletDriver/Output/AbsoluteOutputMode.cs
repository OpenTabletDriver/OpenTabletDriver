using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using JetBrains.Annotations;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Platform.Pointer;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Output
{
    /// <summary>
    /// An absolute positioned output mode.
    /// </summary>
    [PublicAPI]
    [PluginInterface]
    public abstract class AbsoluteOutputMode : OutputMode
    {
        protected AbsoluteOutputMode(InputDevice tablet, IAbsolutePointer absolutePointer)
            : base(tablet)
        {
            Pointer = absolutePointer;
        }

        private Vector2 _min, _max;
        private AngledArea? _inputArea;
        private Area? _outputArea;

        /// <summary>
        /// The area in which the tablet's input is transformed to.
        /// </summary>
        [Setting("Input Area")]
        [MemberSourcedDefaults(nameof(GetDefaultInputArea))]
        public AngledArea? Input
        {
            set
            {
                _inputArea = value;
                TransformationMatrix = CreateTransformationMatrix();
            }
            get => _inputArea;
        }

        /// <summary>
        /// The area in which the final processed output is transformed to.
        /// </summary>
        [Setting("Output Area")]
        [MemberSourcedDefaults(nameof(GetDefaultOutputArea))]
        public Area? Output
        {
            set
            {
                _outputArea = value;
                TransformationMatrix = CreateTransformationMatrix();
            }
            get => _outputArea;
        }

        /// <summary>
        /// Whether to lock aspect ratio when applying area settings.
        /// </summary>
        [Setting("Lock Aspect Ratio", "Locks area aspect ratios from changing."), DefaultValue(false)]
        public bool LockAspectRatio { set; get; }

        /// <summary>
        /// Whether to clip all tablet inputs to the assigned areas.
        /// </summary>
        /// <remarks>
        /// If false, input outside of the area can escape the assigned areas, but still will be transformed.
        /// If true, input outside of the area will be clipped to the edges of the assigned areas.
        /// </remarks>
        [Setting("Area Clipping", "Locks input within the area."), DefaultValue(true)]
        public bool AreaClipping { set; get; }

        /// <summary>
        /// Whether to stop accepting input outside of the assigned areas.
        /// </summary>
        /// <remarks>
        /// If true, <see cref="AreaClipping"/> is automatically implied true.
        /// </remarks>
        [Setting("Area Limiting", "Locks input within the area and discards input outside of it."), DefaultValue(true)]
        public bool AreaLimiting { set; get; }

        /// <summary>
        /// Whether to lock the area position and size inside of the maximum bounds.
        /// Typically used in the user interface.
        /// </summary>
        [Setting("Keep inside maximum bounds", "Locks input within the maximum area bounds."), DefaultValue(true)]
        public bool LockToBounds { set; get; }

        /// <summary>
        /// The class in which the final absolute positioned output is handled.
        /// </summary>
        public IAbsolutePointer Pointer { get; }

        protected override Matrix3x2 CreateTransformationMatrix()
        {
            if (Input != null && Output != null)
            {
                var transform = CalculateTransformation(Input, Output, Tablet.Configuration.Specifications.Digitizer!);

                var halfDisplayWidth = Output?.Width / 2 ?? 0;
                var halfDisplayHeight = Output?.Height / 2 ?? 0;

                var minX = Output?.XPosition - halfDisplayWidth ?? 0;
                var maxX = Output?.XPosition + Output?.Width - halfDisplayWidth ?? 0;
                var minY = Output?.YPosition - halfDisplayHeight ?? 0;
                var maxY = Output?.YPosition + Output?.Height - halfDisplayHeight ?? 0;

                _min = new Vector2(minX, minY);
                _max = new Vector2(maxX, maxY);

                return transform;
            }

            return Matrix3x2.Identity;
        }

        private static Matrix3x2 CalculateTransformation(AngledArea input, Area output, DigitizerSpecifications digitizer)
        {
            // Convert raw tablet data to millimeters
            var res = Matrix3x2.CreateScale(
                digitizer.Width / digitizer.MaxX,
                digitizer.Height / digitizer.MaxY);

            // Translate to the center of input area
            res *= Matrix3x2.CreateTranslation(
                -input.XPosition, -input.YPosition);

            // Apply rotation
            res *= Matrix3x2.CreateRotation(
                (float)(-input.Rotation * System.Math.PI / 180));

            // Scale millimeters to pixels
            res *= Matrix3x2.CreateScale(
                output.Width / input.Width, output.Height / input.Height);

            // Translate output to virtual screen coordinates
            res *= Matrix3x2.CreateTranslation(
                output.XPosition, output.YPosition);

            return res;
        }

        /// <summary>
        /// Transposes, transforms, and performs all absolute positioning calculations to a <see cref="IAbsolutePositionReport"/>.
        /// </summary>
        /// <param name="report">The <see cref="IAbsolutePositionReport"/> in which to transform.</param>
        protected override IAbsolutePositionReport? Transform(IAbsolutePositionReport report)
        {
            // Apply transformation
            var pos = Vector2.Transform(report.Position, TransformationMatrix);

            // Clipping to display bounds
            var clippedPoint = Vector2.Clamp(pos, _min, _max);
            if (AreaLimiting && clippedPoint != pos)
                return null;

            if (AreaClipping)
                pos = clippedPoint;

            report.Position = pos;

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

        public static AngledArea GetDefaultInputArea(DigitizerSpecifications digitizer)
        {
            return new AngledArea
            {
                Width = digitizer.Width,
                Height = digitizer.Height,
                XPosition = digitizer.Width / 2,
                YPosition = digitizer.Height / 2,
                Rotation = 0
            };
        }

        public static Area GetDefaultOutputArea(IVirtualScreen virtualScreen)
        {
            return new Area
            {
                Width = virtualScreen.Width,
                Height = virtualScreen.Height,
                XPosition = virtualScreen.Width / 2,
                YPosition = virtualScreen.Height / 2
            };
        }
    }
}
