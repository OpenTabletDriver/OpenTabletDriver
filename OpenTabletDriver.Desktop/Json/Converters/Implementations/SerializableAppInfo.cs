using OpenTabletDriver.Desktop.Interop.AppInfo;

namespace OpenTabletDriver.Desktop.Json.Converters.Implementations
{
    internal sealed class SerializableAppInfo : Serializable, IAppInfo
    {
        public string ConfigurationDirectory { get; set; } = string.Empty;
        public string SettingsFile { get; set; } = string.Empty;
        public string BinaryDirectory { get; set; } = string.Empty;
        public string PluginDirectory { get; set; } = string.Empty;
        public string PresetDirectory { get; set; } = string.Empty;
        public string LogDirectory { get; set; } = string.Empty;
        public string TemporaryDirectory { get; set; } = string.Empty;
        public string CacheDirectory { get; set; } = string.Empty;
        public string BackupDirectory { get; set; } = string.Empty;
        public string TrashDirectory { get; set; } = string.Empty;
        public string AppDataDirectory { get; set; } = string.Empty;
    }
}
