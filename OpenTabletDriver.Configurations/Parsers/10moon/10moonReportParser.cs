using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.TenMoon
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class TenMoonReportParser : IReportParser<IDeviceReport>
    {
        private bool auxiliaryPressed;

        public IDeviceReport Parse(byte[] report)
        {
            // Reports curtos (interface auxiliar de 8 bytes) não têm os bytes 11 e 12
            if (report.Length < 13 || report[0] != 6)
                return new DeviceReport(report);

            // Teclas da mesa são ativas em nível baixo.
            // Sem tecla: byte 11 = 0xFF e byte 12 = 0x33.
            // As teclas 0x100, 0x200, 0x400 e 0x800 só alteram o byte 12.
            bool pressed = (report[11] & 0xF0) != 0xF0 || (report[12] & 0x33) != 0x33;
            if (pressed || auxiliaryPressed)
            {
                // Emit an all-up auxiliary report on release so bindings can be used again.
                auxiliaryPressed = pressed;
                return new TenMoonAuxReport(report);
            }

            return new TenMoonTabletReport(report);
        }
    }
}
