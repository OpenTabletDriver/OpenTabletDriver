using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTabletDriver.Native;

namespace OpenTabletDriver.Plugin
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

        public static void Open(string path)
        {
            switch (CurrentPlatform)
            {
                case RuntimePlatform.Windows:
                    var startInfo = new ProcessStartInfo("cmd", $"/c start \"{path.Replace("&", "^&")}\"")
                    {
                        CreateNoWindow = true
                    };
                    Process.Start(startInfo);
                    break;
                case RuntimePlatform.Linux:
                    Process.Start("xdg-open", $"\"{path}\"");
                    break;
                case RuntimePlatform.MacOS:
                case RuntimePlatform.FreeBSD:
                    Process.Start("open", $"\"{path}\"");
                    break;
            }
        }
    }
}
