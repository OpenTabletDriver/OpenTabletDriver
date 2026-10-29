using System;
using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin.Output
{
    public class PipelineManager
    {
        protected static void Link<T>(IPipelineElement source, T destination)
        {
            if (destination != null)
            {
                switch (destination)
                {
                    case IPipelineElement nextElement:
                        source.Emit += nextElement.Consume;
                        break;
                    case IEnumerable<IPipelineElement> nextGroup:
                        source.Emit += nextGroup.First().Consume;
                        break;
                    case RefAction<DeviceReport> nextAction:
                        source.Emit += nextAction;
                        break;
                }
            }
        }

        protected static void Unlink<T>(IPipelineElement source, T destination)
        {
            if (destination != null)
            {
                switch (destination)
                {
                    case IPipelineElement nextElement:
                        source.Emit -= nextElement.Consume;
                        break;
                    case IEnumerable<IPipelineElement> nextGroup:
                        source.Emit -= nextGroup.First().Consume;
                        break;
                    case RefAction<DeviceReport> nextAction:
                        source.Emit -= nextAction;
                        break;
                }
            }
        }

        protected static void LinkElements(IEnumerable<IPipelineElement> elements)
        {
            IPipelineElement? prevElement = null;
            foreach (var element in elements)
            {
                if (prevElement != null)
                    Link(prevElement, element);

                prevElement = element;
            }
        }

        protected static void UnlinkElements(IEnumerable<IPipelineElement> elements)
        {
            IPipelineElement? prevElement = null;
            foreach (var element in elements)
            {
                if (prevElement != null)
                    Unlink(prevElement, element);

                prevElement = element;
            }
        }

        protected static void LinkAll(params object[] elements)
        {
            foreach ((var prev, var next) in elements.Zip(elements.Skip(1)))
            {
                if (prev is IPipelineElement prevElement)
                {
                    Link(prevElement, next);
                }
                else if (prev is IEnumerable<IPipelineElement> prevGroup)
                {
                    var pipelineElements = prevGroup as IPipelineElement[] ?? [.. prevGroup];
                    LinkElements(pipelineElements);
                    Link(pipelineElements.Last(), next);
                }
            }
        }

        protected static void UnlinkAll(params object[] elements)
        {
            foreach ((var prev, var next) in elements.Zip(elements.Skip(1)))
            {
                if (prev is IPipelineElement prevElement)
                {
                    Unlink(prevElement, next);
                }
                else if (prev is IEnumerable<IPipelineElement> prevGroup)
                {
                    var pipelineElements = prevGroup as IPipelineElement[] ?? [.. prevGroup];
                    UnlinkElements(pipelineElements);
                    Unlink(pipelineElements.Last(), next);
                }
            }
        }

        protected static IList<IPositionedPipelineElement> GroupElements(IList<IPositionedPipelineElement> elements,
            PipelinePosition position) =>
            [.. elements.Where(e => e.Position == position)];
    }
}
