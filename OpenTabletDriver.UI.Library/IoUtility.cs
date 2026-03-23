using System.Diagnostics;

namespace OpenTabletDriver.UI;

public static class IoUtility
{
    public static void OpenLink(string url)
    {
        if (OperatingSystem.IsWindows())
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        else if (OperatingSystem.IsLinux())
        {
            Process.Start("xdg-open", url);
        }
        else if (OperatingSystem.IsMacOS())
        {
            Process.Start("open", url);
        }
        else
        {
            throw new PlatformNotSupportedException();
        }
    }
}
