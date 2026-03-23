using System.Collections.Generic;
using System.Numerics;
using JetBrains.Annotations;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Output
{
    /// <summary>
    /// An output mode in which all reports are ultimately transformed and subsequently handled.
    /// </summary>
    [PublicAPI]
    [PluginInterface]
    public interface IOutputMode : IPipelineElement<IDeviceReport>
    {
        /// <summary>
        /// Consume the <see cref="IDeviceReport"/> emitted by the device endpoints to be transformed by the pipeline, including <see cref="TransformationMatrix"/>.
        /// </summary>
        /// <param name="report">The <see cref="IDeviceReport"/> to be transformed</param>
        void Read(IDeviceReport report);

        /// <summary>
        /// The list of pipeline elements in which the report is modified.
        /// </summary>
        IList<IDevicePipelineElement>? Elements { set; get; }

        /// <summary>
        /// The transformation matrix in which the device report will be modified.
        /// </summary>
        Matrix3x2 TransformationMatrix { get; }

        /// <summary>
        /// The current tablet assigned to this <see cref="IOutputMode"/>
        /// </summary>
        InputDevice Tablet { get; }
    }
}
