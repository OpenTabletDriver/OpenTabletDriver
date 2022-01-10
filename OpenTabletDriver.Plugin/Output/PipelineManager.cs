using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenTabletDriver.Plugin.Output
{
    public class PipelineManager<T>
    {
        protected void Link<T2>(IPipelineElement<T> source, T2 destination)
        {
            if (source != null && destination != null)
            {
                switch (destination)
                {
                    case IPipelineElement<T> nextElement:
                        source.Emit += nextElement.Consume;
                        break;
                    case IEnumerable<IPipelineElement<T>> nextGroup:
                        source.Emit += nextGroup.First().Consume;
                        break;
                    case Action<T> nextAction:
                        source.Emit += nextAction;
                        break;
                }
            }
        }

        protected void Unlink<T2>(IPipelineElement<T> source, T2 destination)
        {
            if (source != null && destination != null)
            {
                switch (destination)
                {
                    case IPipelineElement<T> nextElement:
                        source.Emit -= nextElement.Consume;
                        break;
                    case IEnumerable<IPipelineElement<T>> nextGroup:
                        source.Emit -= nextGroup.First().Consume;
                        break;
                    case Action<T> nextAction:
                        source.Emit -= nextAction;
                        break;
                }
            }
        }

        protected void LinkElements(IEnumerable<IPipelineElement<T>> elements)
        {
            if (elements != null && elements.Any())
            {
                IPipelineElement<T> prevElement = null;
                foreach (var element in elements)
                {
                    Link(prevElement, element);
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
                    Unlink(prevElement, element);
                    prevElement = element;
                }
            }
        }

        protected void LinkAll(params object[] elements)
        {
            foreach ((var prev, var next) in elements.Zip(elements.Skip(1)))
            {
                if (prev is IPipelineElement<T> prevElement)
                {
                    Link(prevElement, next);
                }
                else if (prev is IEnumerable<IPipelineElement<T>> prevGroup)
                {
                    LinkElements(prevGroup);
                    Link(prevGroup.Last(), next);
                }
            }
        }

        protected void UnlinkAll(params object[] elements)
        {
            foreach ((var prev, var next) in elements.Zip(elements.Skip(1)))
            {
                if (prev is IPipelineElement<T> prevElement)
                {
                    Unlink(prevElement, next);
                }
                else if (prev is IEnumerable<IPipelineElement<T>> prevGroup)
                {
                    UnlinkElements(prevGroup);
                    Unlink(prevGroup.Last(), next);
                }
            }
        }

        protected IList<IPositionedPipelineElement<T>> GroupElements(IList<IPositionedPipelineElement<T>> elements, PipelinePosition position)
        {
            return elements.Where(e => e.Position == position)?.ToArray() ?? Array.Empty<IPositionedPipelineElement<T>>();
        }
    }
}
