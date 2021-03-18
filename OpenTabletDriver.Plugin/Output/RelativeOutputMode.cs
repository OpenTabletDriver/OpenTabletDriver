using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output.Async;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Timing;

namespace OpenTabletDriver.Plugin.Output
{
    /// <summary>
    /// A relatively positioned output mode.
    /// </summary>
    [PluginIgnore]
    public abstract class RelativeOutputMode : AbsoluteOutputMode
    {
        public RelativeOutputMode()
        {
            Config.PropertyChanged += (_, args) =>
            {
                if (!skipPropertyCheck)
                {
                    skipPropertyCheck = true;
                    switch (args.PropertyName)
                    {
                        case "AreaLimiting":
                            Config.AreaLimiting = false;
                            break;
                        case "AreaClipping":
                            Config.AreaClipping = false;
                            break;
                    }
                    skipPropertyCheck = false;
                }
            };
        }

        private Vector2? lastPos;
        private HPETDeltaStopwatch stopwatch = new HPETDeltaStopwatch(true);
        private double reportMsAvg = 10;
        private bool skipReport, skipPropertyCheck;

        protected override void UpdateTransformMatrix()
        {
            this.skipReport = true; // Prevents cursor from jumping on sensitivity change
            base.UpdateTransformMatrix();
        }

        protected override Vector2? Transpose(ITabletReport report)
        {
            var pos = base.Transpose(report);
            var deltaTime = stopwatch.Restart().TotalMilliseconds;
            reportMsAvg += (deltaTime - reportMsAvg) * 0.1;
            Vector2? delta;

            if (lastPos.HasValue & pos.HasValue & !skipReport & AsyncFilterHandler.WithinDelayTolerance(deltaTime, reportMsAvg))
            {
                delta = pos.Value - lastPos.Value;
            }
            else
            {
                skipReport = false;
                delta = null;
            }

            lastPos = pos;
            return delta;
        }
    }
}
