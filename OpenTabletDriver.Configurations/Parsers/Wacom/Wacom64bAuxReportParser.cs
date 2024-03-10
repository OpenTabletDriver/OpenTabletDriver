
﻿using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Touch;


namespace OpenTabletDriver.Configurations.Parsers.Wacom
{
    public class Wacom64bAuxReportParser : IReportParser<IDeviceReport>
    {
        public virtual IDeviceReport Parse(byte[] data)
        {

            return new WacomTouchReport(data, ref _prevTouches, ref _auxButtons);
        }
        private bool[] _auxButtons;
        private TouchPoint[] _prevTouches;

    }
}
