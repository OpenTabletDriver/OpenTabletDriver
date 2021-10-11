using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Interop
{
    public class SystemInterop
    {
        protected SystemInterop()
        {
        }

        public static PluginPlatform CurrentPlatform
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return PluginPlatform.Windows;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return PluginPlatform.Linux;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return PluginPlatform.MacOS;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
                    return PluginPlatform.FreeBSD;
                else
                    return 0;
            }
        }

        public static void Open(string path)
        {
            switch (CurrentPlatform)
            {
                case PluginPlatform.Windows:
                    var startInfo = new ProcessStartInfo("cmd", $"/c start {path.Replace("&", "^&")}")
                    {
                        CreateNoWindow = true
                    };
                    Process.Start(startInfo);
                    break;
                case PluginPlatform.Linux:
                    Process.Start("xdg-open", $"\"{path}\"");
                    break;
                case PluginPlatform.MacOS:
                case PluginPlatform.FreeBSD:
                    Process.Start("open", $"\"{path}\"");
                    break;
            }
        }

        public static void OpenFolder(string path)
        {
            switch (CurrentPlatform)
            {
                case PluginPlatform.Windows:
                    Process.Start("explorer", $"\"{path.Replace("&", "^&")}\"");
                    break;
                default:
                    Open(path);
                    break;
            }
        }
    }
}