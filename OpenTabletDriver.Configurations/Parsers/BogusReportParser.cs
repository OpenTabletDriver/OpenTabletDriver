using System;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers
{
    public class BogusReportParser : IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] data)
        {
            //TODO: change the API to support not returning anything.
            if (data.Length == 0)
            {
                Console.WriteLine("empty packet sadge");
                return null;
            }

            Console.WriteLine("pogu packet");
            return new DeviceReport(data);
        }
    }
}
