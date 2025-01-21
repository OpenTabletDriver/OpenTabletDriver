using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenTabletDriver.Plugin.Output
{
    public class PipelineManager<T>
    {
        protected void Link<T2>(IPipelineElement<T> source, T2 destination)
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

        protected void Unlink<T2>(IPipelineElement<T> source, T2 destination)
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

        protected void LinkElements(IEnumerable<IPipelineElement<T>> elements)
        {
            var pipelineElements = elements as IPipelineElement<T>[] ?? elements.ToArray();
            if (!pipelineElements.Any()) return;
            IPipelineElement<T>? prevElement = null;
            foreach (var element in pipelineElements)
            {
                if (prevElement != null)
                    Link(prevElement, element);
                prevElement = element;
            }
        }

        protected void UnlinkElements(IEnumerable<IPipelineElement<T>> elements)
        {
            var pipelineElements = elements as IPipelineElement<T>[] ?? elements.ToArray();
            if (!pipelineElements.Any()) return;
            IPipelineElement<T>? prevElement = null;
            foreach (var element in pipelineElements)
            {
                if (prevElement != null)
                    Unlink(prevElement, element);
                prevElement = element;
            }
        }

        protected void LinkAll(params object[] elements)
        {
            foreach (var (prev, next) in elements.Zip(elements.Skip(1)))
            {
                if (prev is IPipelineElement<T> prevElement)
                {
                    Link(prevElement, next);
                }
                else if (prev is IEnumerable<IPipelineElement<T>> prevGroup)
                {
                    var pipelineElements = prevGroup as IPipelineElement<T>[] ?? prevGroup.ToArray();
                    LinkElements(pipelineElements);
                    Link(pipelineElements.Last(), next);
                }
            }
        }

        protected void UnlinkAll(params object[] elements)
        {
            foreach (var (prev, next) in elements.Zip(elements.Skip(1)))
            {
                if (prev is IPipelineElement<T> prevElement)
                {
                    Unlink(prevElement, next);
                }
                else if (prev is IEnumerable<IPipelineElement<T>> prevGroup)
                {
                    var pipelineElements = prevGroup as IPipelineElement<T>[] ?? prevGroup.ToArray();
                    UnlinkElements(pipelineElements);
                    Unlink(pipelineElements.Last(), next);
                }
            }
        }

        protected IList<IPositionedPipelineElement<T>> GroupElements(IList<IPositionedPipelineElement<T>>? elements, PipelinePosition position)
        {
            return elements?.Where(e => e.Position == position)?.ToArray() ?? [];
        }
    }
}
