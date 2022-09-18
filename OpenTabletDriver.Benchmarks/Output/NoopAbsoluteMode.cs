using System.Numerics;
using OpenTabletDriver.Output;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Benchmarks.Output
{
    public sealed class NoopAbsoluteMode : AbsoluteOutputMode
    {
        public NoopAbsoluteMode(InputDevice tablet, ISettingsProvider settingsProvider) : base(tablet, new NoopPointer())
        {
            settingsProvider.Inject(this);
        }

        public class NoopPointer : IAbsolutePointer
        {
            public Vector2 Position { set; get; }

            public void SetPosition(Vector2 pos)
            {
                Position = pos;
            }

            public void MouseDown(MouseButton button)
            {
            }

            public void MouseUp(MouseButton button)
            {
            }
        }
    }
}
