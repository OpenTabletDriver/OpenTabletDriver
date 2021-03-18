using System;
using OpenTabletDriver.Plugin.Output.Interpolator;

namespace OpenTabletDriver.Plugin.Output.Async
{
    public interface IAsyncFilter
    {
        SyntheticTabletReport Filter(TimeSpan delta);
        void UpdateState(SyntheticTabletReport report, TimeSpan delta);
    }
}