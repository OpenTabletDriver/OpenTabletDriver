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

        protected IList<IPositionedPipelineElement<IDeviceReport>> PreTransformElements { private set; get; } =
            Array.Empty<IPositionedPipelineElement<IDeviceReport>>();

        protected IList<IPositionedPipelineElement<IDeviceReport>> PostTransformElements { private set; get; } =
            Array.Empty<IPositionedPipelineElement<IDeviceReport>>();

        public bool DisablePressure { set; get; }

        public bool DisableTilt { set; get; }

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

                    var links = ElementsAsPipeline;

                    entryElement = links.First();

                    LinkAll(links, output);
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

        private List<IPipelineElement<IDeviceReport>> ElementsAsPipeline
        {
            get
            {
                var links = new List<IPipelineElement<IDeviceReport>>();

                links.AddRange(PreTransformElements);
                links.Add(this);
                links.AddRange(PostTransformElements);

                return links;
            }
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
                report = Transform(tabletReport);
            if (report != null)
                Emit?.Invoke(report);
        }

        public virtual void Read(IDeviceReport deviceReport) => entryElement?.Consume(deviceReport);

        protected abstract Matrix3x2 CreateTransformationMatrix();
        protected abstract IAbsolutePositionReport Transform(IAbsolutePositionReport tabletReport);
        protected abstract void OnOutput(IDeviceReport report);

        private void DestroyInternalLinks()
        {
            Action<IDeviceReport> output = this.OnOutput;

            var links = ElementsAsPipeline;

            UnlinkAll(links, output);
        }

        public virtual void Dispose()
        {
            entryElement = null;

            foreach (var obj in Elements ?? [])
                if (obj is IDisposable disposable)
                    disposable.Dispose();
        }
    }
}
