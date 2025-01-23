using System.Numerics;
using OpenTabletDriver.Platform.Display;

namespace OpenTabletDriver.Daemon.Contracts.Json.Converters.Implementations
{
    internal sealed class SerializableDisplay : Serializable, IDisplay
    {
        public int Index { set; get; }
        public float Width { set; get; }
        public float Height { set; get; }
        public Vector2 Position { set; get; }
    }
}
