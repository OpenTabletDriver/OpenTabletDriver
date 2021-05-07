using System;

namespace OpenTabletDriver.Plugin
{
    public struct TabletHandlerID : IEquatable<TabletHandlerID>
    {
        public int Value { get; init; }

        public static TabletHandlerID Invalid => new TabletHandlerID { Value = -1 };

        public static bool operator ==(TabletHandlerID a, TabletHandlerID b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(TabletHandlerID a, TabletHandlerID b)
        {
            return a.Value != b.Value;
        }

        public bool Equals(TabletHandlerID other)
        {
            return this.Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is TabletHandlerID b && this == b;
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return $"TabletHandlerID {{{Value}}}";
        }
    }
}