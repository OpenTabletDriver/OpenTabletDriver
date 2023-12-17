namespace OpenTabletDriver.Desktop.Interop.AppInfo
{
    public interface IAppInfo
    {
        string AppDataDirectory { set; get; }
        string ConfigurationDirectory { set; get; }
        string SettingsFile { set; get; }
        string BinaryDirectory { set; get; }
        string PluginDirectory { set; get; }
        string PresetDirectory { set; get; }
        string LogDirectory { set; get; }
        string TemporaryDirectory { set; get; }
        string CacheDirectory { set; get; }
        string BackupDirectory { set; get; }
        string TrashDirectory { set; get; }
    }
}
