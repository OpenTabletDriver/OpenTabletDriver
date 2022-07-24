using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace OpenTabletDriver.Tests
{
    [CollectionDefinition(nameof(TimerTests), DisableParallelization = true)]
    [Collection(nameof(TimerTests))]
    public class TimerTests
    {
        private const double TOLERANCE = 0.075;
        private readonly ITimer _timer;

        public TimerTests()
        {
            var serviceProvider = Utility.GetServices().BuildServiceProvider();
            _timer = serviceProvider.GetRequiredService<ITimer>();
        }

        [Theory]
        [InlineData(0.1f, 5f)]
        [InlineData(0.25f, 5f)]
        [InlineData(0.5f, 5f)]
        [InlineData(1f, 5f)]
        public void TimerAccuracy(float interval, float duration)
        {
            // Skip test when running on Github CI due to high variance in timer latency.
            if (Environment.GetEnvironmentVariable("CI") == "true")
                return;

            var expectedFires = (int)(interval * 1000 / interval * duration);
            var list = new List<double>(expectedFires);
            var watch = new HPETDeltaStopwatch();

            _timer.Interval = interval;
            _timer.Elapsed += () =>
            {
                list.Add(watch.Restart().TotalMilliseconds);
            };

            _timer.Start();
            Thread.Sleep(TimeSpan.FromSeconds(duration));
            _timer.Stop();

            // Windows timers are not guaranteed to stop immediately
            Thread.Sleep(TimeSpan.FromMilliseconds(50));

            var average = list.Average();
            var intervalTolerance = interval * TOLERANCE;
            var minimum = interval - intervalTolerance;
            var maximum = interval + intervalTolerance;

            var withinTolerance = average > minimum && average < maximum;

            Assert.True(withinTolerance);
        }
    }
}
