using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Supernote // Use the same namespace
{
    // This is the main parser class OpenTabletDriver interacts with
    public class SupernoteNomadReportParser : IReportParser<IDeviceReport>
    {
        // This method is called by OTD for each raw report received
        public IDeviceReport Parse(byte[] data)
        {
            // Check if the report matches the expected format for the Supernote Nomad pen report
            // We expect a 9-byte report starting with 0x00 (Report ID 0)
            if (data.Length == 9 && data[0] == 0x00)
            {
                // If it matches, create and return a SupernoteNomadReport
                return new SupernoteNomadReport(data);
            }

            // If the report doesn't match the expected format,
            // return a generic DeviceReport or potentially log a warning.
            // Returning DeviceReport(data) allows OTD's debugger to still show the raw data
            // for reports that aren't handled by this parser.
            return new DeviceReport(data);
        }
    }
}
