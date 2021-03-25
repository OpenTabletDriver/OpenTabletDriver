using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin.Output
{
    public abstract class OutputMode : PipelineManager<IDeviceReport>, IPipelineElement<IDeviceReport>, IOutputMode
    {
        private TabletState tablet;
        private IList<IPositionedPipelineElement<IDeviceReport>> elements;

        private event Action<IDeviceReport> DeviceOutput;
        private event Action<IDeviceReport> PreTransform;
        private event Action<IDeviceReport> PostTransform;

        public event Action<IDeviceReport> Emit;

        public virtual void Consume(IDeviceReport report)
        {
            if (report is ITabletReport tabletReport)
            {
                if (Transform(tabletReport) is ITabletReport transformedTabletReport)
                    report = transformedTabletReport;
            }

            Emit?.Invoke(report);
        }

        public virtual void Read(IDeviceReport deviceReport) => DeviceOutput?.Invoke(deviceReport);

        protected abstract ITabletReport Transform(ITabletReport tabletReport);
        protected abstract void OnFinalReport(IDeviceReport report);

        protected virtual void OnPreTransform(IDeviceReport report) => PreTransform?.Invoke(report);
        protected virtual void OnPostTransform(IDeviceReport report) => PostTransform?.Invoke(report);

        public IList<IPositionedPipelineElement<IDeviceReport>> Elements
        {
            set
            {
                this.elements = value;

                if (Elements != null && Elements.Count > 0)
                {
                    PreTransformElements = GroupElements(Elements, PipelinePosition.PreTransform);
                    
                    this.DeviceOutput += PreTransformElements.First().Consume;
                    LinkElements(PreTransformElements);
                    PreTransformElements.Last().Emit += this.OnPreTransform;

                    this.PreTransform += this.Consume;
                    this.Emit += this.OnPostTransform;

                    PostTransformElements = GroupElements(Elements, PipelinePosition.PostTransform);
                    
                    this.PostTransform += PostTransformElements.First().Consume;
                    LinkElements(PostTransformElements);
                    PostTransformElements.Last().Emit += this.OnFinalReport;
                }
                else
                {
                    this.DeviceOutput += this.Consume;
                    this.Emit += this.OnFinalReport;
                }
            }
            get => this.elements;
        }

        public Matrix3x2 TransformationMatrix { protected set; get; }

        public virtual TabletState Tablet
        {
            set
            {
                this.tablet = value;
                this.TransformationMatrix = CreateTransformationMatrix();
            }
            get => this.tablet;
        }

        protected IList<IPositionedPipelineElement<IDeviceReport>> PreTransformElements { private set; get; }
        protected IList<IPositionedPipelineElement<IDeviceReport>> PostTransformElements { private set; get; }

        protected abstract Matrix3x2 CreateTransformationMatrix();
    }
}