using System.Numerics;
using OpenTabletDriver.Platform.Display;
using WaylandNET.Client.Protocol;

namespace OpenTabletDriver.Daemon.Interop.Display
{
    public class WaylandOutput : IDisplay
    {
        internal WlOutput? WlOutput { set; get; }
        internal ZxdgOutputV1? XdgOutput { set; get; }

        public int Index { set; get; }
        public float Width { set; get; }
        public float Height { set; get; }
        public Vector2 Position { set; get; }
        public string? Name { set; get; }
        public string? Description { set; get; }

        public override string ToString() => $"{Name} {Description} ({Width}x{Height}@{Position})";
    }
}
