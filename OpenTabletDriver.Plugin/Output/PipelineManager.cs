using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace OpenTabletDriver.Plugin.Output
{
    public class PipelineManager<T>
    {
        protected void LinkElements(IList<IPositionedPipelineElement<T>> elements)
        {
            if (elements != null && elements.Count > 0)
            {
                IPipelineElement<T> prevElement = null;
                foreach (var element in elements)
                {
                    if (prevElement != null)
                        prevElement.Emit += element.Consume;
                    prevElement = element;
                }
            }
        }

        protected void UnlinkElements(IList<IPositionedPipelineElement<T>> elements)
        {
            IPipelineElement<T> prevElement = null;
            if (elements != null && elements.Count > 0)
            {
                foreach (var element in elements)
                {
                    if (prevElement != null)
                        prevElement.Emit -= element.Consume;
                    prevElement = element;
                }
            }
        }

        protected IList<IPositionedPipelineElement<T>> GroupElements(IList<IPositionedPipelineElement<T>> elements, PipelinePosition position)
        {
            return elements.Where(e => e.Position == position)?.ToArray();
        }
    }
}