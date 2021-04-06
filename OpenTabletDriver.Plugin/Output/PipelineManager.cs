using System.Collections.Generic;
using System.Linq;

namespace OpenTabletDriver.Plugin.Output
{
    public class PipelineManager<T>
    {
        protected void LinkElement(IPipelineElement<T> source, IPipelineElement<T> destination)
        {
            if (source != null && destination != null)
                source.Emit += destination.Consume;
        }

        protected void UnlinkElement(IPipelineElement<T> source, IPipelineElement<T> destination)
        {
            if (source != null && destination != null)
                source.Emit -= destination.Consume;
        }

        protected void LinkElements(IEnumerable<IPipelineElement<T>> elements)
        {
            if (elements != null && elements.Any())
            {
                IPipelineElement<T> prevElement = null;
                foreach (var element in elements)
                {
                    LinkElement(prevElement, element);
                    prevElement = element;
                }
            }
        }

        protected void UnlinkElements(IEnumerable<IPipelineElement<T>> elements)
        {
            if (elements != null && elements.Any())
            {
                IPipelineElement<T> prevElement = null;
                foreach (var element in elements)
                {
                    UnlinkElement(prevElement, element);
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