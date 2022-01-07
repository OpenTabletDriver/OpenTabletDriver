using System.Numerics;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Benchmarks.Output
{
    public sealed class NoopAbsoluteMode : AbsoluteOutputMode
    {
        public override IAbsolutePointer Pointer { set; get; } = new NoopPointer();

        public class NoopPointer : IAbsolutePointer
        {
            public Vector2 Position { private set; get; }

            public void SetPosition(Vector2 pos)
            {
                Position = pos;
            }
        }
    }
}
