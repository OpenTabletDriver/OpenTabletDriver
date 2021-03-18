using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin.Output
{
    /// <summary>
    /// An absolutely positioned output mode.
    /// </summary>
    [PluginIgnore]
    public abstract class AbsoluteOutputMode : BaseOutputMode
    {
        private Vector2 min, max;
        private Matrix3x2 transformationMatrix;

        protected override void UpdateTransformMatrix()
        {
            var input = Config.Input;
            var output = Config.Output;

            if (input != null && output != null && Tablet?.Digitizer != null)
                this.transformationMatrix = CalculateTransformation(input, output, Tablet.Digitizer);

            var halfDisplayWidth = output?.Width / 2 ?? 0;
            var halfDisplayHeight = output?.Height / 2 ?? 0;

            var minX = output?.Position.X - halfDisplayWidth ?? 0;
            var maxX = output?.Position.X + output?.Width - halfDisplayWidth ?? 0;
            var minY = output?.Position.Y - halfDisplayHeight ?? 0;
            var maxY = output?.Position.Y + output?.Height - halfDisplayHeight ?? 0;

            this.min = new Vector2(minX, minY);
            this.max = new Vector2(maxX, maxY);
        }

        protected static Matrix3x2 CalculateTransformation(Area input, Area output, DigitizerIdentifier tablet)
        {
            // Convert raw tablet data to millimeters
            var res = Matrix3x2.CreateScale(
                tablet.Width / tablet.MaxX,
                tablet.Height / tablet.MaxY);

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

        protected override Vector2? Transpose(ITabletReport report)
        {
            var pos = new Vector2(report.Position.X, report.Position.Y);

            // Pre Filter
            foreach (IFilter filter in this.preFilters ??= Array.Empty<IFilter>())
                pos = filter.Filter(pos);

            // Apply transformation
            pos = Vector2.Transform(pos, this.transformationMatrix);

            // Clipping to display bounds
            var clippedPoint = Vector2.Clamp(pos, this.min, this.max);
            if (Config.AreaLimiting && clippedPoint != pos)
                return null;

            if (Config.AreaClipping)
                pos = clippedPoint;

            // Post Filter
            foreach (IFilter filter in this.postFilters ??= Array.Empty<IFilter>())
                pos = filter.Filter(pos);

            return pos;
        }
    }
}
