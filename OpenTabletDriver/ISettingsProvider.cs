using JetBrains.Annotations;

namespace OpenTabletDriver
{
    /// <summary>
    /// A settings provider interface that injects settings marked by <see cref="OpenTabletDriver.Attributes.SettingAttribute"/>.
    /// </summary>
    [PublicAPI]
    public interface ISettingsProvider
    {
        /// <summary>
        /// Injects all settings properties marked by <see cref="OpenTabletDriver.Attributes.SettingAttribute"/>.
        /// </summary>
        /// <param name="obj">The object to inject settings into.</param>
        void Inject(object obj);
    }
}
