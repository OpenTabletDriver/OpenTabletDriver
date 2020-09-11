using System;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Contracts
{
    public interface IDeviceReader<T> where T : IDeviceReport
    {
        event EventHandler<T> Report;
        IReportParser<T> Parser { get; }
    }
}