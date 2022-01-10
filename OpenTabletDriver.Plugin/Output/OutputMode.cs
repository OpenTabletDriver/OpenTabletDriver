using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin.Output
{
    public abstract class OutputMode : PipelineManager<IDeviceReport>, IOutputMode
    {
        public OutputMode()
        {
            Passthrough = true;
        }

        private bool passthrough;
        private TabletReference tablet;
        private IList<IPositionedPipelineElement<IDeviceReport>> elements;
        private IPipelineElement<IDeviceReport> entryElement;

        public event Action<IDeviceReport> Emit;

        protected bool Passthrough
        {
            private set
            {
                Action<IDeviceReport> output = this.OnOutput;
                if (value && !passthrough)
                {
                    this.entryElement = this;
                    Link(this, output);
                    this.passthrough = true;
                }
                else if (!value && passthrough)
                {
                    this.entryElement = null;
                    Unlink(this, output);
                    this.passthrough = false;
                }
            }
            get => this.passthrough;
        }

        protected IList<IPositionedPipelineElement<IDeviceReport>> PreTransformElements { private set; get; } = Array.Empty<IPositionedPipelineElement<IDeviceReport>>();
        protected IList<IPositionedPipelineElement<IDeviceReport>> PostTransformElements { private set; get; } = Array.Empty<IPositionedPipelineElement<IDeviceReport>>();

        public Matrix3x2 TransformationMatrix { protected set; get; }

        public IList<IPositionedPipelineElement<IDeviceReport>> Elements
        {
            set
            {
                this.elements = value;

                Passthrough = false;
                DestroyInternalLinks();

                if (Elements != null && Elements.Count > 0)
                {
                    PreTransformElements = GroupElements(Elements, PipelinePosition.PreTransform);
                    PostTransformElements = GroupElements(Elements, PipelinePosition.PostTransform);

                    Action<IDeviceReport> output = this.OnOutput;

                    if (PreTransformElements.Any() && !PostTransformElements.Any())
                    {
                        entryElement = PreTransformElements.First();

                        // PreTransform --> Transform --> Output
                        LinkAll(PreTransformElements, this, output);
                    }
                    else if (PostTransformElements.Any() && !PreTransformElements.Any())
                    {
                        entryElement = this;

                        // Transform --> PostTransform --> Output
                        LinkAll(this, PostTransformElements, output);
                    }
                    else if (PreTransformElements.Any() && PostTransformElements.Any())
                    {
                        entryElement = PreTransformElements.First();

                        // PreTransform --> Transform --> PostTransform --> Output
                        LinkAll(PreTransformElements, this, PostTransformElements, output);
                    }
                }
                else
                {
                    Passthrough = true;
                    PreTransformElements = Array.Empty<IPositionedPipelineElement<IDeviceReport>>();
                    PostTransformElements = Array.Empty<IPositionedPipelineElement<IDeviceReport>>();
                }
            }
            get => this.elements;
        }

        public virtual TabletReference Tablet
        {
            set
            {
                this.tablet = value;
                this.TransformationMatrix = CreateTransformationMatrix();
            }
            get => this.tablet;
        }

        public virtual void Consume(IDeviceReport report)
        {
            if (report is IAbsolutePositionReport tabletReport)
                if (Transform(tabletReport) is IAbsolutePositionReport transformedReport)
                    report = transformedReport;

            Emit?.Invoke(report);
        }

        public virtual void Read(IDeviceReport deviceReport) => entryElement?.Consume(deviceReport);

        protected abstract Matrix3x2 CreateTransformationMatrix();
        protected abstract IAbsolutePositionReport Transform(IAbsolutePositionReport tabletReport);
        protected abstract void OnOutput(IDeviceReport report);

        private void DestroyInternalLinks()
        {
            Action<IDeviceReport> output = this.OnOutput;

            if (PreTransformElements.Any() && !PostTransformElements.Any())
            {
                UnlinkAll(PreTransformElements, this, output);
            }
            else if (PostTransformElements.Any() && !PreTransformElements.Any())
            {
                UnlinkAll(this, PostTransformElements, output);
            }
            else if (PreTransformElements.Any() && PostTransformElements.Any())
            {
                UnlinkAll(PreTransformElements, this, PostTransformElements, output);
            }
        }
    }
}
