using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.UCLogic
{
    // Wraps UCLogicReportParser and compacts the Gaomon 860T's sparse aux
    // button bits (0,1,2,5,6,7 of report[4]) into a dense 6-button array.
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class UCLogic860TReportParser : IReportParser<IDeviceReport>
    {
        private readonly UCLogicReportParser _parser = new();

        public IDeviceReport Parse(byte[] data)
        {
            var report = _parser.Parse(data);

            if (report is UCLogicAuxReport aux)
                return new UCLogic860TAuxReport(aux.Raw);

            return report;
        }
    }
}
