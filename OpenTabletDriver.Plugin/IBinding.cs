using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin
{
    public interface IBinding
    {
        /// <summary>
        /// The method to perform when the binding is being activated.
        /// </summary>
        /// <param name="report">The report that triggered the press.</param>
        void Press(IDeviceReport report);

        /// <summary>
        /// Invoked when the binding is being released.
        /// </summary>
        /// <param name="report">The report that triggered the release.</param>
        void Release(IDeviceReport report);
    }
}