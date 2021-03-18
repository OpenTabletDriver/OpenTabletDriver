using System;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output.Interpolator;

namespace OpenTabletDriver.Plugin.Output.Async
{
    [PluginName("No-op"), HideFromEditor]
    public class NoopAsyncFilter : IAsyncFilter
    {
        private SyntheticTabletReport report;

        public SyntheticTabletReport Filter(TimeSpan delta)
        {
            return report;
        }

        public void UpdateState(SyntheticTabletReport report, TimeSpan delta)
        {
            this.report = report;
        }
    }
}