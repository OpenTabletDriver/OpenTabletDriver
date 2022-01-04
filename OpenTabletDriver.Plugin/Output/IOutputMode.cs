using System.Collections.Generic;
using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin.Output
{
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
        IList<IPositionedPipelineElement<IDeviceReport>> Elements { set; get; }

        /// <summary>
        /// The transformation matrix in which the 
        /// </summary>
        /// <value></value>
        Matrix3x2 TransformationMatrix { get; }

        /// <summary>
        /// The current tablet assigned to this <see cref="IOutputMode"/>
        /// </summary>
        TabletReference Tablet { set; get; }
    }
}
