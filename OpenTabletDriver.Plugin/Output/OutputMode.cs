using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin.Output
{
    public abstract class OutputMode<T> : PipelineManager, IOutputMode where T : IPointer
    {
        protected OutputMode()
        {
            Passthrough = true;
        }

        private bool passthrough;
        private TabletReference? tablet;
        private IList<IPositionedPipelineElement> elements = [];
        private IPipelineElement? entryElement;

        public event RefAction<DeviceReport>? Emit;

        public abstract T? Pointer { get; set; }

        protected bool Passthrough
        {
            private set
            {
                RefAction<DeviceReport> output = this.OnOutput;
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

        protected IList<IPositionedPipelineElement> InternalElements { private set; get; } =
            Array.Empty<IPositionedPipelineElement>();
        protected IList<IPositionedPipelineElement> PreTransformElements { private set; get; } =
            Array.Empty<IPositionedPipelineElement>();

        protected IList<IPositionedPipelineElement> PostTransformElements { private set; get; } =
            Array.Empty<IPositionedPipelineElement>();

        public bool DisablePressure { set; get; }

        public bool DisableTilt { set; get; }

        public bool DisableRotation { set; get; }

        public Matrix3x2 TransformationMatrix { protected set; get; }

        public IList<IPositionedPipelineElement> Elements
        {
            set
            {
                this.elements = value;

                Passthrough = false;
                DestroyInternalLinks();

                if (elements.Count > 0)
                {
                    InternalElements = GroupElements(Elements, PipelinePosition.Internal);
                    PreTransformElements = GroupElements(Elements, PipelinePosition.PreTransform);
                    PostTransformElements = GroupElements(Elements, PipelinePosition.PostTransform);

                    RefAction<DeviceReport> output = this.OnOutput;

                    var links = ElementsAsPipeline;

                    entryElement = links.First();

                    LinkAll(links, output);
                }
                else
                {
                    Passthrough = true;
                    InternalElements = Array.Empty<IPositionedPipelineElement>();
                    PreTransformElements = Array.Empty<IPositionedPipelineElement>();
                    PostTransformElements = Array.Empty<IPositionedPipelineElement>();
                }
            }
            get => this.elements;
        }

        private List<IPipelineElement> ElementsAsPipeline
        {
            get
            {
                var links = new List<IPipelineElement>();

                links.AddRange(InternalElements);
                links.AddRange(PreTransformElements);
                links.Add(this);
                links.AddRange(PostTransformElements);

                return links;
            }
        }

        public virtual TabletReference? Tablet
        {
            set
            {
                this.tablet = value;
                this.TransformationMatrix = CreateTransformationMatrix();
            }
            get => this.tablet;
        }

        public virtual void Consume(scoped ref DeviceReport report)
        {
            bool success = false;
            if (report.PenPosition.HasValue)
                success = Transform(ref report);

            if (success)
                Emit?.Invoke(ref report);
        }

        public virtual void Read(scoped ref DeviceReport deviceReport) => entryElement?.Consume(ref deviceReport);

        protected abstract Matrix3x2 CreateTransformationMatrix();
        protected abstract bool Transform(scoped ref DeviceReport report);

        /// <summary>
        /// Late output handler action, prior to synchronizing report. Used to set position in official OpenTabletDriver output modes.
        /// <para/>
        /// This function is called from the base implementation of <see cref="OnOutput"/> prior to synchronizing with <see cref="Pointer"/> if it's an <see cref="ISynchronousPointer"/>.
        /// </summary>
        /// <param name="report">The current <see cref="DeviceReport"/> reference</param>
        /// <remarks>
        /// If you're implementing your own output mode from scratch,
        /// and do not need the normal pointer interface handling of OpenTabletDriver (e.g. for blazingly fast position-only output modes)
        /// you probably want to override <see cref="OnOutput"/> instead</remarks>
        protected virtual void OnOutputPreSync(scoped ref DeviceReport report) { }

        /// <summary>
        /// Full output handler for the output mode.
        /// </summary>
        /// <param name="report">The current <see cref="DeviceReport"/> reference</param>
        /// <remarks>If you only want to emit the position, you should override <see cref="OnOutputPreSync"/> instead</remarks>
        protected virtual void OnOutput(scoped ref DeviceReport report)
        {
            var synchronousPointer = Pointer as ISynchronousPointer;
            // small behavior change from v0.6.8 and prior: don't handle remaining pipeline if out of range was reported
            if (report.OutOfRange)
                synchronousPointer?.Reset();
            else
            {
                // this should be ordered from least to most chance of having a
                // dependency to another pointer property.
                if (report.HoverDistance.HasValue && Pointer is IHoverDistanceHandler hoverDistanceHandler)
                    hoverDistanceHandler.SetHoverDistance(report.HoverDistance.Value);
                if (report.Eraser.HasValue && Pointer is IEraserHandler eraserHandler)
                    eraserHandler.SetEraser(report.Eraser.Value);
                if (report.Tilt.HasValue && Pointer is ITiltHandler tiltHandler && !DisableTilt)
                    tiltHandler.SetTilt(report.Tilt.Value);
                if (report.Rotation.HasValue && Pointer is IRotationHandler rotationHandler &&
                    !DisableRotation && Tablet?.Properties.Specifications.Pen.MaxRotation != null && Tablet?.Properties.Specifications.Pen.MinRotation != null)
                {
                    var rotationRange = (float)Tablet.Properties.Specifications.Pen.MaxRotation! - (float)Tablet.Properties.Specifications.Pen.MinRotation!;
                    rotationHandler.SetRotation((report.Rotation.Value - (float)Tablet.Properties.Specifications.Pen.MinRotation!) / rotationRange);
                }
                if (report.PenPressure.HasValue && Pointer is IPressureHandler pressureHandler &&
                    !DisablePressure && Tablet?.Properties.Specifications.Pen != null)
                    pressureHandler.SetPressure(report.PenPressure.Value / (float)Tablet.Properties.Specifications.Pen.MaxPressure);

                // make sure to set the position last
                OnOutputPreSync(ref report);
            }

            synchronousPointer?.Flush();
        }

        private void DestroyInternalLinks()
        {
            RefAction<DeviceReport> output = this.OnOutput;

            var links = ElementsAsPipeline;

            UnlinkAll(links, output);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                entryElement = null;

                foreach (var obj in Elements)
                    if (obj is IDisposable disposable)
                        disposable.Dispose();
            }

            _isDisposed = true;
        }
    }
}
