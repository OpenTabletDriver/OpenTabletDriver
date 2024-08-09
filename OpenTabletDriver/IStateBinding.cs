using JetBrains.Annotations;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver
{
    /// <summary>
    /// A binding with a boolean state.
    /// </summary>
    [PublicAPI]
    [PluginInterface]
    public interface IStateBinding : IBinding
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
