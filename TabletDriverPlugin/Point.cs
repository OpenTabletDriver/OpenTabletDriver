using System;
using System.Numerics;

namespace TabletDriverPlugin
{
    public class Point
    {
        public Point()
        {
        }

        public Point(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { set; get; }
        public float Y { set; get; }

        public float DistanceFrom(Point value)
        {
            double x = X - value.X;
            double y = Y - value.Y;
            return (float)Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
        }

        #region Overrides

        public override string ToString()
        {
            return $"{X},{Y}";
        }

        #endregion

        #region Operators

        /// <summary>
        /// Returns the sum of two <see cref="Point"/> objects.
        /// </summary>
        /// <param name="a">Augend</param>
        /// <param name="b">Addend</param>
        public static Point operator +(Point a, Point b)
        {
            var x = a.X + b.X;
            var y = a.Y + b.Y;
            return new Point(x, y);
        }

        /// <summary>
        /// Returns the difference of two <see cref="Point"/> objects.
        /// </summary>
        /// <param name="a">Minuend</param>
        /// <param name="b">Subtrahend</param>
        /// <returns></returns>
        public static Point operator -(Point a, Point b)
        {
            var x = a.X - b.X;
            var y = a.Y - b.Y;
            return new Point(x, y);
        }

        public static implicit operator Point(Vector2 vector)
        {
            return new Point(vector.X, vector.Y);
        }

        public static implicit operator Vector2(Point point)
        {
            return new Vector2(point.X, point.Y);
        }

        public static implicit operator Point(System.Drawing.Point pt)
        {
            return new Point(pt.X, pt.Y);
        }

        public static implicit operator System.Drawing.Point(Point point)
        {
            return new System.Drawing.Point((int)point.X, (int)point.Y);
        }

        public static implicit operator string(Point point)
        {
            return point.ToString();
        }

        #endregion
    }
}