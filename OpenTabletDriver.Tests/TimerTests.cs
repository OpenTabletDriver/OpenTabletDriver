using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Plugin.Timing;

namespace OpenTabletDriver.Tests
{
    [TestClass]
    public class TimerTests
    {
        private const double TOLERANCE = 0.05;

        [DataTestMethod]
        [DataRow(0.1f, 5f)]
        [DataRow(0.25f, 5f)]
        [DataRow(0.5f, 5f)]
        [DataRow(1f, 5f)]
        public void TimerAccuracy(float interval, float duration)
        {
            var expectedFires = (int)(interval * 1000 / interval * duration);
            var timer = DesktopInterop.Timer;
            var list = new List<double>(expectedFires);
            var watch = new HPETDeltaStopwatch(true);

            timer.Interval = interval;
            timer.Elapsed += () =>
            {
                list.Add(watch.Restart().TotalMilliseconds);
            };

            Console.WriteLine($"Running timer with {interval}ms interval for {duration} seconds");

            timer.Start();
            Thread.Sleep(TimeSpan.FromSeconds(duration));
            timer.Stop();

            var average = list.Average();
            var intervalTolerance = interval * TOLERANCE;
            var minimum = interval - intervalTolerance;
            var maximum = interval + intervalTolerance;

            Console.WriteLine($"Timer fired with an interval average of {average}");

            if ((average > minimum) && (average < maximum))
            {
                Console.WriteLine($"{average} is within {interval} +- {intervalTolerance}");
            }
            else
            {
                Console.WriteLine($"{average} is not within {interval} +- {intervalTolerance}");
                Assert.Fail();
            }
        }
    }
}