using System;
using TabletDriverPlugin.Tablet;

namespace TabletDriverLib.Contracts
{
    public interface IDeviceReader<T> where T : IDeviceReport
    {
        event EventHandler<T> Report;
        IReportParser<T> Parser { set; get; }
    }
}