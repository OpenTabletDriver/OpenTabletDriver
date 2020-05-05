using System;
using System.IO;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using NativeLib;

namespace OpenTabletDriverUX
{
    public static class AppInfo
    {
        public static AboutDialog AboutDialog => _aboutDialog.Value;
        public static Bitmap Logo => _logo.Value;
        public static DirectoryInfo ConfigurationDirectory => new DirectoryInfo(Path.Join(Environment.CurrentDirectory, "Configurations"));
        public static DirectoryInfo AppDataDirectory => _appDataDirectory.Value;
        public static FileInfo SettingsFile => new FileInfo(Path.Join(AppDataDirectory.FullName, "settings.json"));

        private static readonly Lazy<AboutDialog> _aboutDialog = new Lazy<AboutDialog>(() => new AboutDialog
        {
            Title = "OpenTabletDriver",
            ProgramName = "OpenTabletDriver",
            ProgramDescription = "Open source, cross-platform tablet configurator",
            WebsiteLabel = "OpenTabletDriver GitHub Repository",
            Website = new Uri(@"https://github.com/InfinityGhost/OpenTabletDriver"),
            Version = $"v{Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}",
            Developers = new string[] { "InfinityGhost" },
            Designers = new string[] { "InfinityGhost" },
            Documenters = new string[] { "InfinityGhost" },
            License = File.ReadAllText("/usr/share/licenses/common/GPL3/license.txt"),
            Copyright = string.Empty,
            Logo = Logo.WithSize(256, 256)
        });

        private static readonly Lazy<Bitmap> _logo = new Lazy<Bitmap>(() => 
        {
            var dataStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("OpenTabletDriverUX.Assets.otd.png");
            return new Bitmap(dataStream);
        });

        private static readonly Lazy<DirectoryInfo> _appDataDirectory = new Lazy<DirectoryInfo>(() => 
        {
            if (PlatformInfo.IsWindows)
            {
                var appdata = Environment.GetEnvironmentVariable("LOCALAPPDATA");
                return new DirectoryInfo(Path.Join(appdata, "OpenTabletDriver"));
            }
            else if (PlatformInfo.IsLinux)
            {
                var home = Environment.GetEnvironmentVariable("HOME");
                return new DirectoryInfo(Path.Join(home, ".config", "OpenTabletDriver"));
            }
            else if (PlatformInfo.IsOSX)
            {
                var macHome = Environment.GetEnvironmentVariable("HOME");
                return new DirectoryInfo(Path.Join(macHome, "Library", "Application Support", "OpenTabletDriver"));
            }
            else
            {
                return null;
            }
        });
    }
}