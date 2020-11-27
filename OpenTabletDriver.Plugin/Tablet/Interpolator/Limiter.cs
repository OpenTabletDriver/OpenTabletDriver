using System;
using System.Numerics;

namespace OpenTabletDriver.Plugin.Tablet.Interpolator
{
    internal static class Limiter
    {
        private static readonly Vector2 minimum = new Vector2(1, 3);
        private static readonly Vector2 maximum = new Vector2(7.5f, 7.5f * 1.5f);

        public static double Transform(double reportMs)
        {
            var scale = (float)((reportMs - minimum.X) / (maximum.X - minimum.X));
            var value = Vector2.Lerp(minimum, maximum, scale);
            return Math.Max(value.Y, minimum.Y);
        }
    }
}