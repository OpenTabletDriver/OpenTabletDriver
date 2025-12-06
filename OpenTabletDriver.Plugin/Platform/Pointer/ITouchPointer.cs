using System;
using OpenTabletDriver.Plugin.Tablet.Touch;

namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface ITouchPointer
    {
        /// <summary>
        /// Sets the positions of all finger(s) provided by the touch points from the parser.
        ///
        /// Positions must be provided in raw tablet coordinates, as the touch handler will remap itself (for now)
        /// </summary>
        /// <param name="positions">For each finger, an absolute position</param>
        /// <param name="maxX">The maximum positional value able to be reported on the X axis</param>
        /// <param name="maxY">The maximum positional value able to be reported on the Y axis</param>
        // TODO: maxX and maxY here should instead come from a tablet reference attached to the output mode (currently not possible as of 0.6.6.2)
        void SetPositions(ReadOnlySpan<TouchPoint> positions, int maxX, int maxY);
    }
}
