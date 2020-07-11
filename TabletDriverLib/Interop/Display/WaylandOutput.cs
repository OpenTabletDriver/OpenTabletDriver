using TabletDriverPlugin;
using TabletDriverPlugin.Platform.Display;
using WaylandNET.Client.Protocol;

namespace TabletDriverLib.Interop.Display
{
    public class WaylandOutput : IDisplay, WlOutput.IListener, ZxdgOutputV1.IListener
    {
        public WlOutput WlOutput { private set; get; }
        public ZxdgOutputV1 XdgOutput { set; get; }

        public int Index { private set; get; }
        public float Width { private set; get; }
        public float Height { private set; get; }
        public Point Position { private set; get; }

        public string Name { private set; get; }
        public string Description { private set; get; }

        public WaylandOutput(WlOutput wlOutput, int index)
        {
            WlOutput = wlOutput;
            Index = index;
        }

        public override string ToString()
        {
            return $"{Name} {Description} ({Width}x{Height}@{Position})";
        }

        void WlOutput.IListener.Geometry(WlOutput wlOutput, int x, int y, int physicalWidth, int physicalHeight,
            WlOutput.Subpixel subpixel, string make, string model, WlOutput.Transform transform)
        {
            // xdg_output version 1 does not have name or description events
            if (XdgOutput == null || XdgOutput.Version < 2)
            {
                Position = new Point(x, y);
                Name = make;
                Description = model;
            }
        }

        void WlOutput.IListener.Mode(WlOutput wlOutput, WlOutput.Mode flags, int width, int height, int refresh)
        {
            if (XdgOutput == null && flags.HasFlag(WlOutput.Mode.Current))
            {
                Width = width;
                Height = height;
            }
        }

        void WlOutput.IListener.Done(WlOutput wlOutput)
        {
        }

        void WlOutput.IListener.Scale(WlOutput wlOutput, int factor)
        {
        }

        void ZxdgOutputV1.IListener.LogicalPosition(ZxdgOutputV1 zxdgOutputV1, int x, int y)
        {
            Position = new Point(x, y);
        }

        void ZxdgOutputV1.IListener.LogicalSize(ZxdgOutputV1 zxdgOutputV1, int width, int height)
        {
            Width = width;
            Height = height;
        }

        void ZxdgOutputV1.IListener.Done(ZxdgOutputV1 zxdgOutputV1)
        {
        }

        void ZxdgOutputV1.IListener.Name(ZxdgOutputV1 zxdgOutputV1, string name)
        {
            Name = name;
        }

        void ZxdgOutputV1.IListener.Description(ZxdgOutputV1 zxdgOutputV1, string description)
        {
            Description = description;
        }
    }
}
