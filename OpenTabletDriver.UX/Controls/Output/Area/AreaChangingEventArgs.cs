using System;

namespace OpenTabletDriver.UX.Controls.Output.Area
{
    public class AreaChangingEventArgs : EventArgs
    {
        public AreaChangingEventArgs(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; set; }
        public float Y { get; set; }
    }
}