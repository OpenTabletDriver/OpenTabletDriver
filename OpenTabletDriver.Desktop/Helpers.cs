using System;
using System.Linq;

namespace OpenTabletDriver.Desktop
{
    public static class Helpers
    {
        /// <summary>
        /// Distribute <paramref name="count"/> into buckets with max value of <paramref name="maxValuePerBucket"/>.
        /// Useful for distributing a number of elements into rows and columns.
        /// </summary>
        /// <param name="count">Amount to split into buckets</param>
        /// <param name="maxValuePerBucket">Maximum amount per bucket</param>
        /// <returns>Buckets with the amount of count to take for each element</returns>
        public static int[] SplitIntoBuckets(int count, int maxValuePerBucket = 4)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (count <= maxValuePerBucket) return [count];

            int bucketCount = (int)Math.Ceiling((double)count / maxValuePerBucket);

            // initialize number of elements
            int[] rv = [.. Enumerable.Repeat(0, bucketCount)];

            int remaining = count;
            int index = 0;
            while (remaining-- > 0)
                rv[index++ % bucketCount] += 1;

            return rv;
        }
    }
}
