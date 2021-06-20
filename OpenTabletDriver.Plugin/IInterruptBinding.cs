using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin
{
    public interface IInterruptBinding : IBinding
    {
        /// <summary>
        /// Invokes the binding on every report, giving the most updated information.
        /// </summary>
        /// <param name="tablet">The tablet that this report is from.</param>
        /// <param name="report">The report that triggered the invocation.</param>
        void Invoke(TabletReference tablet, IDeviceReport report);
    }
}