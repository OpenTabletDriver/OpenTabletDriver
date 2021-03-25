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

        protected void UnlinkAll(IList<IPositionedPipelineElement<T>> elements)
        {
            foreach (var element in elements)
            {
                var emitEvent = typeof(IPipelineElement<T>).GetEvent(nameof(IPipelineElement<T>.Emit));
                typeof(IPipelineElement<T>).GetProperty("Events");
            }
        }

        protected IList<IPositionedPipelineElement<T>> GroupElements(IList<IPositionedPipelineElement<T>> elements, PipelinePosition position)
        {
            return elements.Where(e => e.Position == position)?.ToArray();
        }
    }
}