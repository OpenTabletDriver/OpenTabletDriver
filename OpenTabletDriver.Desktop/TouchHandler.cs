using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Touch;

namespace OpenTabletDriver.Desktop
{
    [PluginIgnore]
    public class TouchHandler : IPositionedPipelineElement<IDeviceReport>
    {
        private readonly TabletReference _tablet;
        private readonly ITouchPointer _pointer;

        public TouchHandler(TabletReference tablet, ITouchPointer pointer)
        {
            _tablet = tablet;
            _pointer = pointer;
        }

        public void Consume(IDeviceReport report)
        {
            if (report is ITouchReport touchReport)
                HandleTouch(touchReport);
            Emit?.Invoke(report);
        }

        private void HandleTouch(ITouchReport report)
        {
            bool shouldReset = false;
            if (report.Touches.Any(x => x != null))
            {
                _pointer.SetPositions(report.Touches, (int)_tablet.Properties.Specifications.Touch!.MaxX, (int)_tablet.Properties.Specifications.Touch!.MaxY);
            }
            else
                shouldReset = true;

            if (_pointer is ISynchronousPointer syncPointer)
            {
                syncPointer.Flush();
                if (shouldReset)
                    syncPointer.Reset();
            }
        }

        public event Action<IDeviceReport> Emit;
        public PipelinePosition Position => PipelinePosition.PostTransform;
    }
}
