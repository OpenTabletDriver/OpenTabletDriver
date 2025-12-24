using System.ComponentModel;
using JetBrains.Annotations;
namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// Device specifications for gesture touchpads.
    /// </summary>
    [PublicAPI]
    public class GestureTouchpadSpecifications
    {
        /// <summary>
        /// The amount of gestures.
        /// </summary>
        public uint GestureCount { set; get; }
    }
}
