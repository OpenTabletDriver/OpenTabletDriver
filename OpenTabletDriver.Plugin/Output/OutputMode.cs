using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin.Output
{
    public abstract class OutputMode : PipelineManager<IDeviceReport>, IPipelineElement<IDeviceReport>, IOutputMode
    {
        public OutputMode()
        {
            SetPassthrough();
        }

        private bool isPassthrough;
        private TabletState tablet;
        private IList<IPositionedPipelineElement<IDeviceReport>> elements;
        private IPipelineElement<IDeviceReport> entryElement;

        public event Action<IDeviceReport> Emit;

        protected IList<IPositionedPipelineElement<IDeviceReport>> PreTransformElements { private set; get; }
        protected IList<IPositionedPipelineElement<IDeviceReport>> PostTransformElements { private set; get; }

        public Matrix3x2 TransformationMatrix { protected set; get; }

        public virtual void Consume(IDeviceReport report)
        {
            if (report is ITabletReport tabletReport)
            {
                if (Transform(tabletReport) is ITabletReport transformedTabletReport)
                    report = transformedTabletReport;
            }

            Emit?.Invoke(report);
        }

        public virtual void Read(IDeviceReport deviceReport) => entryElement?.Consume(deviceReport);

        protected abstract ITabletReport Transform(ITabletReport tabletReport);
        protected abstract void OnOutput(IDeviceReport report);
        
        public IList<IPositionedPipelineElement<IDeviceReport>> Elements
        {
            set
            {
                this.elements = value;

                if (Elements != null && Elements.Count > 0)
                {
                    UnsetPassthrough();
                    PreTransformElements = GroupElements(Elements, PipelinePosition.PreTransform);
                    PostTransformElements = GroupElements(Elements, PipelinePosition.PostTransform);
                    LinkElements(PreTransformElements);
                    LinkElements(PostTransformElements);

                    if (PreTransformElements.Any() && !PostTransformElements.Any())
                    {
                        entryElement = PreTransformElements.First();

                        // Link PreTransformElements to TransformElement
                        LinkElement(PreTransformElements.Last(), this);

                        // Link TransformElement to output
                        this.Emit += this.OnOutput;
                    }
                    else if (PostTransformElements.Any() && !PreTransformElements.Any())
                    {
                        entryElement = this;

                        // Link TransformElement to PostTransformElements
                        this.Emit += PostTransformElements.Last().Consume;

                        // Hook PostTransformElements to output
                        PostTransformElements.Last().Emit += this.OnOutput;
                    }
                    else if (PreTransformElements.Any() && PostTransformElements.Any())
                    {
                        entryElement = PreTransformElements.First();

                        // Link PreTransformElements to TransformElement
                        PreTransformElements.Last().Emit += this.Consume;

                        // Link TransformElement to PostTransformElements
                        this.Emit += PostTransformElements.First().Consume;

                        // Link PostTransformElements to output
                        PostTransformElements.Last().Emit += this.OnOutput;
                    }
                }
                else
                {
                    SetPassthrough();
                }
            }
            get => this.elements;
        }

        public virtual TabletState Tablet
        {
            set
            {
                this.tablet = value;
                this.TransformationMatrix = CreateTransformationMatrix();
            }
            get => this.tablet;
        }

        protected abstract Matrix3x2 CreateTransformationMatrix();

        private void SetPassthrough()
        {
            if (!isPassthrough)
            {
                this.entryElement = this;
                this.Emit += this.OnOutput;
                isPassthrough = true;
            }
        }

        private void UnsetPassthrough()
        {
            if (isPassthrough)
            {
                this.entryElement = null;
                this.Emit -= this.OnOutput;
                isPassthrough = false;
            }
        }
    }
}