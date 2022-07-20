using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// An area conversion utility, from manufacturer driver area to an <see cref="AngledArea"/>.
    /// </summary>\
    [PublicAPI]
    public interface IAreaConverter
    {
        /// <summary>
        /// The vendor in which this converter is designated.
        /// </summary>
        DeviceVendor Vendor { get; }

        /// <summary>
        /// The label for the top <see cref="Convert"/> argument.
        /// </summary>
        string Top { get; }

        /// <summary>
        /// The label for the left <see cref="Convert"/> argument.
        /// </summary>
        string Left { get; }

        /// <summary>
        /// The label for the bottom <see cref="Convert"/> argument.
        /// </summary>
        string Bottom { get; }

        /// <summary>
        /// The label for the right <see cref="Convert"/> argument.
        /// </summary>
        string Right { get; }

        /// <summary>
        /// Converts a manufacturer driver area to an OpenTabletDriver area.
        /// </summary>
        /// <param name="tablet">The tablet in which the area is designated to.</param>
        /// <param name="top">The dimension defined by the <see cref="Top"/> label.</param>
        /// <param name="left">The dimension defined by the <see cref="Left"/> label.</param>
        /// <param name="bottom">The dimension defined by the <see cref="Bottom"/> label.</param>
        /// <param name="right">The dimension defined by the <see cref="Right"/> label.</param>
        /// <returns>An <see cref="AngledArea"/> converted from manufacturer specifications.</returns>
        AngledArea Convert(InputDevice tablet, double top, double left, double bottom, double right);
    }
}
