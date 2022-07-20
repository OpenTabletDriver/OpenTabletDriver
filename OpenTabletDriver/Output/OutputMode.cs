using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using JetBrains.Annotations;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Output
{
    /// <summary>
    /// The base implementation of an <see cref="IOutputMode"/>, complete with transformation calculation.
    /// </summary>
    [PublicAPI]
    public abstract class OutputMode : PipelineManager<IDeviceReport>, IOutputMode
    {
        protected OutputMode(InputDevice tablet)
        {
            Tablet = tablet;
            Passthrough = true;
        }

        private bool _passthrough;
        private InputDevice _tablet = null!;
        private IList<IDevicePipelineElement>? _elements;
        private IPipelineElement<IDeviceReport>? _entryElement;

        public event Action<IDeviceReport>? Emit;

        protected bool Passthrough
        {
            private set
            {
                Action<IDeviceReport> output = OnOutput;
                if (value && !_passthrough)
                {
                    _entryElement = this;
                    Link(this, output);
                    _passthrough = true;
                }
                else if (!value && _passthrough)
                {
                    _entryElement = null;
                    Unlink(this, output);
                    _passthrough = false;
                }
            }
            get => _passthrough;
        }

        private IList<IDevicePipelineElement> _preTransformElements = Array.Empty<IDevicePipelineElement>();
        private IList<IDevicePipelineElement> _postTransformElements = Array.Empty<IDevicePipelineElement>();

        public Matrix3x2 TransformationMatrix { protected set; get; }

        public IList<IDevicePipelineElement>? Elements
        {
            set
            {
                _elements = value;

                Passthrough = false;
                DestroyInternalLinks();

                if (Elements != null && Elements.Any())
                {
                    _preTransformElements = GroupElements(Elements, PipelinePosition.PreTransform);
                    _postTransformElements = GroupElements(Elements, PipelinePosition.PostTransform);

                    Action<IDeviceReport> output = OnOutput;

                    if (_preTransformElements.Any() && !_postTransformElements.Any())
                    {
                        _entryElement = _preTransformElements.First();

                        // PreTransform --> Transform --> Output
                        LinkAll(_preTransformElements, this, output);
                    }
                    else if (_postTransformElements.Any() && !_preTransformElements.Any())
                    {
                        _entryElement = this;

                        // Transform --> PostTransform --> Output
                        LinkAll(this, _postTransformElements, output);
                    }
                    else if (_preTransformElements.Any() && _postTransformElements.Any())
                    {
                        _entryElement = _preTransformElements.First();

                        // PreTransform --> Transform --> PostTransform --> Output
                        LinkAll(_preTransformElements, this, _postTransformElements, output);
                    }
                }
                else
                {
                    Passthrough = true;
                    _preTransformElements = Array.Empty<IDevicePipelineElement>();
                    _postTransformElements = Array.Empty<IDevicePipelineElement>();
                }
            }
            get => _elements;
        }

        public InputDevice Tablet
        {
            set
            {
                _tablet = value;
                TransformationMatrix = CreateTransformationMatrix();
            }
            get => _tablet;
        }

        public virtual void Consume(IDeviceReport report)
        {
            if (report is IAbsolutePositionReport tabletReport)
                if (Transform(tabletReport) is IAbsolutePositionReport transformedReport)
                    report = transformedReport;

            Emit?.Invoke(report);
        }

        public virtual void Read(IDeviceReport deviceReport) => _entryElement?.Consume(deviceReport);

        /// <summary>
        /// Invoked to calculate the new transformation matrix.
        /// </summary>
        /// <returns>A transformation matrix to be applied to the IDeviceReport.</returns>
        protected abstract Matrix3x2 CreateTransformationMatrix();

        /// <summary>
        /// Transform an absolutely positioned device report.
        /// </summary>
        /// <param name="tabletReport">
        /// The tablet report to be transformed.
        /// </param>
        /// <returns>
        /// A transformed <see cref="IAbsolutePositionReport"/>.
        /// </returns>
        protected abstract IAbsolutePositionReport? Transform(IAbsolutePositionReport tabletReport);

        /// <summary>
        /// Pushes the final report state to its native handlers.
        /// </summary>
        /// <param name="report">A transformed report to output.</param>
        protected abstract void OnOutput(IDeviceReport report);

        private void DestroyInternalLinks()
        {
            Action<IDeviceReport> output = OnOutput;

            if (_preTransformElements.Any() && !_postTransformElements.Any())
            {
                UnlinkAll(_preTransformElements, this, output);
            }
            else if (_postTransformElements.Any() && !_preTransformElements.Any())
            {
                UnlinkAll(this, _postTransformElements, output);
            }
            else if (_preTransformElements.Any() && _postTransformElements.Any())
            {
                UnlinkAll(_preTransformElements, this, _postTransformElements, output);
            }
        }
    }
}
