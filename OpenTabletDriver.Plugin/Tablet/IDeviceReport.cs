using System;

namespace OpenTabletDriver.Plugin.Tablet
{
    public interface IDeviceReport
    {
        byte[] Raw { get; }
        string GetStringRaw() => BitConverter.ToString(Raw).Replace('-', ' ');
        string GetStringFormat() => GetStringRaw();
    }
}