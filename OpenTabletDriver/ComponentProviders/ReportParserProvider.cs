using System;
using System.Collections.Generic;
using OpenTabletDriver.Plugin.Components;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.ComponentProviders
{
    public class ReportParserProvider : IReportParserProvider
    {
        private static readonly Dictionary<string, Func<IReportParser<IDeviceReport>>> _reportParsers = new()
        {
            { typeof(AuxReportParser).FullName, () => new AuxReportParser() },
            { typeof(DeviceReportParser).FullName, () => new DeviceReportParser() },
            { typeof(TabletReportParser).FullName, () => new TabletReportParser() },
            { typeof(TiltTabletReportParser).FullName, () => new TiltTabletReportParser() },
            { typeof(Vendors.Huion.GianoReportParser).FullName, () => new Vendors.Huion.GianoReportParser() },
            { typeof(Vendors.SkipByteTabletReportParser).FullName, () => new Vendors.SkipByteTabletReportParser() },
            { typeof(Vendors.UCLogic.UCLogicReportParser).FullName, () => new Vendors.UCLogic.UCLogicReportParser() },
            { typeof(Vendors.Veikk.VeikkReportParser).FullName, () => new Vendors.Veikk.VeikkReportParser() },
            { typeof(Vendors.Wacom.Bamboo.BambooReportParser).FullName, () => new Vendors.Wacom.Bamboo.BambooReportParser() },
            { typeof(Vendors.Wacom.Intuos.IntuosReportParser).FullName, () => new Vendors.Wacom.Intuos.IntuosReportParser() },
            { typeof(Vendors.Wacom.Intuos.WacomDriverIntuosReportParser).FullName, () => new Vendors.Wacom.Intuos.WacomDriverIntuosReportParser() },
            { typeof(Vendors.Wacom.Intuos3.Intuos3ReportParser).FullName, () => new Vendors.Wacom.Intuos3.Intuos3ReportParser() },
            { typeof(Vendors.Wacom.IntuosV1.IntuosV1ReportParser).FullName, () => new Vendors.Wacom.IntuosV1.IntuosV1ReportParser() },
            { typeof(Vendors.Wacom.IntuosV1.WacomDriverIntuosV1ReportParser).FullName, () => new Vendors.Wacom.IntuosV1.WacomDriverIntuosV1ReportParser() },
            { typeof(Vendors.Wacom.IntuosV2.IntuosV2ReportParser).FullName, () => new Vendors.Wacom.IntuosV2.IntuosV2ReportParser() },
            { typeof(Vendors.Wacom.IntuosV2.WacomDriverIntuosV2ReportParser).FullName, () => new Vendors.Wacom.IntuosV2.WacomDriverIntuosV2ReportParser() },
            { typeof(Vendors.Wacom.Wacom64bAuxReportParser).FullName, () => new Vendors.Wacom.Wacom64bAuxReportParser() },
            { typeof(Vendors.XP_Pen.XP_PenReportParser).FullName, () => new Vendors.XP_Pen.XP_PenReportParser() }
        };

        public IReportParser<IDeviceReport> GetReportParser(string reportParserName)
        {
            return _reportParsers[reportParserName].Invoke();
        }
    }
}