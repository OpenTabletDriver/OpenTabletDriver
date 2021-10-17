using System.Linq;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin.Output
{
    /// <summary>
    /// An absolutely positioned output mode.
    /// </summary>
    [PluginIgnore]
    public abstract class AbsoluteOutputMode : OutputMode, IPointerProvider<IAbsolutePointer>
    {
        private Vector2 min, max;
        private Area outputArea, inputArea;

        /// <summary>
        /// The area in which the tablet's input is transformed to.
        /// </summary>
        public Area Input
        {
            set
            {
                this.inputArea = value;
                this.TransformationMatrix = CreateTransformationMatrix();
            }
            get => this.inputArea;
        }

        /// <summary>
        /// The area in which the final processed output is transformed to.
        /// </summary>
        public Area Output
        {
            set
            {
                this.outputArea = value;
                this.TransformationMatrix = CreateTransformationMatrix();
            }
            get => this.outputArea;
        }

        /// <summary>
        /// The class in which the final absolute positioned output is handled.
        /// </summary>
        public abstract IAbsolutePointer Pointer { set; get; }

        /// <summary>
        /// Whether to clip all tablet inputs to the assigned areas.
        /// </summary>
        /// <remarks>
        /// If false, input outside of the area can escape the assigned areas, but still will be transformed.
        /// If true, input outside of the area will be clipped to the edges of the assigned areas.
        /// </remarks>
        public bool AreaClipping { set; get; }

        /// <summary>
        /// Whether to stop accepting input outside of the assigned areas.
        /// </summary>
        /// <remarks>
        /// If true, <see cref="AreaClipping"/> is automatically implied true.
        /// </remarks>
        public bool AreaLimiting { set; get; }

        protected override Matrix3x2 CreateTransformationMatrix()
        {
            if (Input != null && Output != null && Tablet != null)
            {
                var transform = CalculateTransformation(Input, Output, Tablet.Properties.Specifications.Digitizer);

                var halfDisplayWidth = Output?.Width / 2 ?? 0;
                var halfDisplayHeight = Output?.Height / 2 ?? 0;

                var minX = Output?.Position.X - halfDisplayWidth ?? 0;
                var maxX = Output?.Position.X + Output?.Width - halfDisplayWidth ?? 0;
                var minY = Output?.Position.Y - halfDisplayHeight ?? 0;
                var maxY = Output?.Position.Y + Output?.Height - halfDisplayHeight ?? 0;

                this.min = new Vector2(minX, minY);
                this.max = new Vector2(maxX, maxY);

                return transform;
            }
            else
            {
                return Matrix3x2.Identity;
            }
        }

        protected static Matrix3x2 CalculateTransformation(Area input, Area output, DigitizerSpecifications digitizer)
        {
            // Convert raw tablet data to millimeters
            var res = Matrix3x2.CreateScale(
                digitizer.Width / digitizer.MaxX,
                digitizer.Height / digitizer.MaxY);

            // Translate to the center of input area
            res *= Matrix3x2.CreateTranslation(
                -input.Position.X, -input.Position.Y);

            // Apply rotation
            res *= Matrix3x2.CreateRotation(
                (float)(-input.Rotation * System.Math.PI / 180));

            // Scale millimeters to pixels
            res *= Matrix3x2.CreateScale(
                output.Width / input.Width, output.Height / input.Height);

            // Translate output to virtual screen coordinates
            res *= Matrix3x2.CreateTranslation(
                output.Position.X, output.Position.Y);

            return res;
        }

        /// <summary>
        /// Transposes, transforms, and performs all absolute positioning calculations to a <see cref="IAbsolutePositionReport"/>.
        /// </summary>
        /// <param name="report">The <see cref="IAbsolutePositionReport"/> in which to transform.</param>
        protected override IAbsolutePositionReport Transform(IAbsolutePositionReport report)
        {
            // Apply transformation
            var pos = Vector2.Transform(report.Position, this.TransformationMatrix);

            // Clipping to display bounds
            var clippedPoint = Vector2.Clamp(pos, this.min, this.max);
            if (AreaLimiting && clippedPoint != pos)
                return null;

            if (AreaClipping)
                pos = clippedPoint;

            report.Position = pos;

            return report;
        }

        protected override void OnOutput(IDeviceReport report)
        {
            if (report is IAbsolutePositionReport absReport)
                Pointer.SetPosition(absReport.Position);
        }
    }
}
