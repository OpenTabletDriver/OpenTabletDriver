using System.Numerics;
using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// A report containing information for a absolute positioned mouse.
    /// </summary>
    [PublicAPI]
    public interface IMouseReport : IAbsolutePositionReport
    {
        /// <summary>
        /// The current mouse buttons states.
        /// </summary>
        bool[] MouseButtons { set; get; }

        /// <summary>
        /// The current scroll delta.
        /// </summary>
        Vector2 Scroll { set; get; }
    }
}
