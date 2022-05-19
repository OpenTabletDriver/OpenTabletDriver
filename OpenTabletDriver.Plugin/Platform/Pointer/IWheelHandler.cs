using System.Numerics;

namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface IWheelHandler
    {
        /// <summary>
        /// Reports the absolute wheel position in degrees to the OS stack
        /// </summary>
        /// <param name="degrees">The amount of degrees the wheel is positioned to (0-359)</param>
        void SetWheel(uint degrees);

        /// <summary>
        /// Reports that the wheel is no longer being used
        /// </summary>
        void UnsetWheel();
    }
}
