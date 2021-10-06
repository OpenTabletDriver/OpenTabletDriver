using System;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Output
{
    [PluginName("Artist Mode"), SupportedPlatform(PluginPlatform.Linux)]
    public class LinuxArtistMode : AbsoluteOutputMode, IPointerProvider<IAbsolutePointer>
    {
        public LinuxArtistMode(IVirtualTablet virtualTablet)
        {
            VirtualTablet = virtualTablet;
        }

        public IVirtualTablet VirtualTablet { set; get; }

        public override IAbsolutePointer Pointer
        {
            set => throw new NotSupportedException();
            get => this.VirtualTablet;
        }

        protected override void OnOutput(IDeviceReport report)
        {
            base.OnOutput(report);

            if (report is ITiltReport tiltReport)
                VirtualTablet.SetTilt(tiltReport.Tilt);

            if (report is IEraserReport eraserReport)
                VirtualTablet.SetEraser(eraserReport.Eraser);

            if (report is IProximityReport proximityReport)
                VirtualTablet.SetProximity(proximityReport.NearProximity, proximityReport.HoverDistance);
        }
    }
}
