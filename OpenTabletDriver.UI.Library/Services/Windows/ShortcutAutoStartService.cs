using System.Diagnostics;
using WindowsShortcutFactory;

namespace OpenTabletDriver.UI.Services.Windows;

/// <summary>
/// Creates a shortcut in the startup folder to start the application on login.
/// </summary>
public class ShortcutAutoStartService : IAutoStartService
{
    public bool AutoStartSupported => true;
    public bool AutoStart => File.Exists(GetShortcutPath());

    public string? BackendName => "Windows";

    public bool TrySetAutoStart(bool autoStart)
    {
        if (!autoStart && AutoStart)
        {
            // Remove shortcut
            File.Delete(GetShortcutPath());
            return true;
        }
        else if (autoStart && !AutoStart)
        {
            // Create shortcut
            using var shortcut = new WindowsShortcut
            {
                // TODO: Find a better way to get the executable path
                Path = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "OpenTabletDriver.UI.exe"),
                Description = "Launch OpenTabletDriver",
                ShowCommand = ProcessWindowStyle.Minimized
            };

            shortcut.Save(GetShortcutPath());
            return true;
        }

        return false;
    }

    private static string GetShortcutPath()
    {
        var startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        return Path.Combine(startupPath, "OpenTabletDriver.lnk");
    }
}
