namespace OpenTabletDriver.Plugin.DependencyInjection
{
    /// <summary>
    /// When used on a OpenTabletDriver plugin class, the <see cref="PropertiesInitialized"/> function is run
    /// after any associated values for <see cref="Attributes.PropertyAttribute"/> have been set.
    /// </summary>
    public interface IPropertiesInitialized
    {
        /// <summary>
        /// This function is invoked after all <see cref="Attributes.PropertyAttribute"/>s have been set
        /// </summary>
        void PropertiesInitialized();
    }
}
