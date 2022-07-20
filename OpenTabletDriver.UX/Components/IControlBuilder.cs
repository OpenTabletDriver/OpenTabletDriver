using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;

namespace OpenTabletDriver.UX.Components
{
    public interface IControlBuilder
    {
        /// <summary>
        /// Builds a control of <see cref="T"/>.
        /// </summary>
        /// <param name="additionalDependencies">Any additional dependencies to be provided to the constructor.</param>
        /// <typeparam name="T">The control type.</typeparam>
        /// <returns>A built control of <see cref="T"/></returns>
        T Build<T>(params object[] additionalDependencies) where T : Control;

        /// <summary>
        /// Generates controls for plugin settings based on the provided type.
        /// </summary>
        /// <param name="settings">The plugin settings to modify</param>
        /// <param name="type">The plugin type</param>
        IEnumerable<Control> Generate(PluginSettings? settings, Type type);
    }
}
