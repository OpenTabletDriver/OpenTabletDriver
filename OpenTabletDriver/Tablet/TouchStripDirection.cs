using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    [Flags]
    [PublicAPI]
    public enum TouchStripDirection
    {
        None,
        Up,
        Down,
    }
}
