using System;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Plugin.Output.Async;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin.Output
{
    public abstract class BaseOutputMode : IOutputMode
    {
        public BaseOutputMode()
        {
            Config.PropertyChanged += (_, arg) =>
            {
                if (arg.PropertyName == "Input" || arg.PropertyName == "Output")
                    UpdateTransformMatrix();
            };
        }

        private bool isDisposed;
        protected IFilter[] filters, preFilters, postFilters;
        protected AsyncFilterHandler asyncHandler;
        protected TabletState tablet;

        public abstract IPointer Pointer { get; }

        public IFilter[] Filters
        {
            set
            {
                this.filters = value;
                this.preFilters = this.filters.Where(t => t.FilterStage == FilterStage.PreTranspose || t.FilterStage == FilterStage.PreAsync).ToArray();
                this.postFilters = this.filters.Where(t => t.FilterStage == FilterStage.PostTranspose).ToArray();
            }
            get => this.filters;
        }

        public TabletState Tablet
        {
            set
            {
                this.tablet = value;
                UpdateTransformMatrix();
            }
            get => this.tablet;
        }

        public OutputModeConfig Config { get; set; } = new OutputModeConfig();

        public AsyncFilterHandler AsyncHandler
        {
            set
            {
                asyncHandler = value;
                asyncHandler.Bind(t => Process(t));
            }
            get => asyncHandler;
        }

        public virtual void Read(IDeviceReport report)
        {
            if (report is ITabletReport tabletReport)
            {
                if (Tablet.Digitizer.ActiveReportID.IsInRange(tabletReport.ReportID))
                {
                    if (AsyncHandler != null)
                    {
                        AsyncHandler.Process(tabletReport);
                    }
                    else
                    {
                        Process(tabletReport);
                    }
                }
            }
        }

        private void Process(ITabletReport tabletReport)
        {
            if (Pointer is IPressureHandler pressureHandler)
                pressureHandler.SetPressure((float)tabletReport.Pressure / (float)Tablet.Digitizer.MaxPressure);

            if (Transpose(tabletReport) is Vector2 pos)
            {
                Pointer.HandlePoint(pos);
            }
        }

        protected abstract void UpdateTransformMatrix();
        protected abstract Vector2? Transpose(ITabletReport report);

        public void Dispose()
        {
            if (isDisposed)
                return;

            AsyncHandler?.Dispose();
            if (Filters != null)
            {
                foreach (var filter in Filters)
                {
                    try
                    {
                        if (filter is IDisposable disposableFilter)
                            disposableFilter.Dispose();
                    }
                    catch
                    {
                        Log.Write("Plugin", $"Unable to dispose object '{filter.GetType().Name}'", LogLevel.Error);
                    }
                }
            }

            isDisposed = true;
        }

        ~BaseOutputMode()
        {
            Dispose();
        }
    }
}