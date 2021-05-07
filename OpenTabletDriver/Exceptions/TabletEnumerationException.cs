using System;

namespace OpenTabletDriver
{
    public class TabletEnumerationException : Exception
    {
        public TabletEnumerationException() : base()
        {
        }

        public TabletEnumerationException(string msg) : base(msg)
        {
        }
    }
}