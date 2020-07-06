using System;
using System.Diagnostics;

namespace NativeLib
{
    public static class Tools
    {
        public static void OpenUrl(string url)
        {
            if (PlatformInfo.IsWindows)
            {
                var startInfo = new ProcessStartInfo("cmd", $"/c start \"\" \"{url.Replace("&", "^&")}\"")
                {
                    CreateNoWindow = true
                };
                Process.Start(startInfo);
            }
            else if (PlatformInfo.IsLinux)
                Process.Start("xdg-open", url);
            else if (PlatformInfo.IsOSX)
                Process.Start("open", url);
            else
                throw new InvalidOperationException("Unable to open the URL, as platform wasn't detected.");
        }
    }
}
