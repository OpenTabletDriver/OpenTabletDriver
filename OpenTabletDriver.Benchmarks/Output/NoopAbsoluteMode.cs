using System.Numerics;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Benchmarks.Output
{
    public sealed class NoopAbsoluteMode : AbsoluteOutputMode
    {
        public override IPointer Pointer { get; } = new NoopPointer();

        public class NoopPointer : IPointer
        {
            public Vector2 Position { private set; get; }

            public void HandlePoint(Vector2 pos)
            {
                Position = pos;
            }
        }
    }
}