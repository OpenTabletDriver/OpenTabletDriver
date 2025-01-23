namespace OpenTabletDriver.UI.Models;

public class UIEnvironment
{
    /// <summary>
    /// Gets an array of arguments passed to the application.
    /// </summary>
    public string[] Args { get; }

    /// <summary>
    /// Gets the path to the application's data directory.
    /// </summary>
    public string AppDataPath { get; }

    public UIEnvironment(string[] args, string appDataPath)
    {
        Args = args;
        AppDataPath = appDataPath;

        if (!Directory.Exists(appDataPath))
            Directory.CreateDirectory(appDataPath);
    }

    public static UIEnvironment Create(string[] args)
    {
        var defaultAppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "OpenTabletDriver.UI"
        );

        var currentExe = Path.GetDirectoryName(AppContext.BaseDirectory)!;
        var alternativeAppDataPath = Path.Combine(currentExe, "userdata");

        var appData = Directory.Exists(alternativeAppDataPath)
            ? alternativeAppDataPath
            : defaultAppDataPath;

        return new UIEnvironment(args, appData);
    }
}
