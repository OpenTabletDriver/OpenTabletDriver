using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin
{
    public interface IStateBinding : IBinding
    {
        /// <summary>
        /// The method to perform when the binding is being activated.
        /// </summary>
        /// <param name="tablet">The tablet that this report is from.</param>
        /// <param name="report">The report that triggered the press.</param>
        void Press(TabletReference tablet, IDeviceReport report);

        /// <summary>
        /// Invoked when the binding is being released.
        /// </summary>
        /// <param name="tablet">The tablet that this report is from.</param>
        /// <param name="report">The report that triggered the release.</param>
        void Release(TabletReference tablet, IDeviceReport report);
    }
}