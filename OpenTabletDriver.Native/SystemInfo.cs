using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native
{
    public static class SystemInfo
    {
        public static RuntimePlatform CurrentPlatform
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return RuntimePlatform.Windows;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return RuntimePlatform.Linux;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return RuntimePlatform.MacOS;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
                    return RuntimePlatform.FreeBSD;
                else
                    return 0;
            }
        }

        public static void Open(string path, bool alternative = false)
        {
            switch (CurrentPlatform)
            {
                case RuntimePlatform.Windows:
                    var startInfo = new ProcessStartInfo("cmd", $"/c start {path.Replace("&", "^&")}")
                    {
                        CreateNoWindow = true
                    };
                    Process.Start(startInfo);
                    break;
                case RuntimePlatform.Linux:
                    if (alternative)
                        Process.Start("dbus-send", $"--session --dest=org.freedesktop.FileManager1 --type=method_call /org/freedesktop/FileManager1 org.freedesktop.FileManager1.ShowFolders array:string:\"file:{path}\" string:\"\"");
                    else
                        Process.Start("xdg-open", path);
                    break;
                case RuntimePlatform.MacOS:
                case RuntimePlatform.FreeBSD:
                    Process.Start("open", path);
                    break;
            }
        }
    }
}