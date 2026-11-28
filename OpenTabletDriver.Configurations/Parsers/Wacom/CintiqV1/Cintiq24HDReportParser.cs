using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Wacom.CintiqV1
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class Cintiq24HDReportParser : CintiqV1ReportParser
    {
        public override IDeviceReport Parse(byte[] report)
        {
            return report[0] switch
            {
                0x0C => new CintiqV1AuxReport(report, ParseTopButtons(report), 1, 2),
                _ => base.Parse(report)
            };
        }

        private static bool[] ParseTopButtons(byte[] report)
        {
            var infoButton = report[3];
            var topButtons = report[4];

            // These 3 buttons are on a touch strip, ranging from 0x1000 (left) to 0x0001 (right)
            return
            [
                infoButton.IsBitSet(3) || infoButton.IsBitSet(4), // i-button
                topButtons.IsBitSet(0) || topButtons.IsBitSet(1), // settings button
                topButtons.IsBitSet(6) || topButtons.IsBitSet(7), // keyboard button
            ];
        }
    }
}
