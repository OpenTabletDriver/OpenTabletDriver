using System;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Output
{
    [PluginName("Artist Mode"), SupportedPlatform(PluginPlatform.Linux)]
    public class LinuxArtistMode : AbsoluteOutputMode, IPointerProvider<IAbsolutePointer>
    {
        [Resolved]
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
        }
    }
}
