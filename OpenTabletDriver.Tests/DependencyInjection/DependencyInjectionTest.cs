using System;
using System.Timers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin.Timers;

namespace OpenTabletDriver.Tests.DependencyInjection
{
    [TestClass]
    public class DependencyInjectionTest
    {
        IServiceProvider pluginManager = new ServiceManager();

        [TestInitialize]
        public void Initialize()
        {
            var serviceManager = this.pluginManager as ServiceManager;
            serviceManager.AddService<ITimer>(() => new CLRTimer());
        }

        [TestMethod]
        public void TestGetService()
        {
            var timer = (ITimer)pluginManager.GetService(typeof(ITimer));
            timer.Interval = 1500; // 1.5 seconds
            timer.Start();
            timer.Elapsed += timer.Stop;

            while (timer.Enabled)
            {
                // Busy wait until timer elapses.
            }
        }

        private class CLRTimer : ITimer
        {
            public CLRTimer()
            {
                clrTimer.Elapsed += (sender, e) => Elapsed?.Invoke();
            }

            private Timer clrTimer = new Timer
            {
                AutoReset = true
            };

            public bool Enabled => clrTimer.Enabled;

            public float Interval
            {
                set => clrTimer.Interval = value;
                get => (float)clrTimer.Interval;
            }

            public event Action Elapsed;

            public void Dispose()
            {
                Stop();
                clrTimer?.Dispose();
                clrTimer = null;
            }

            public void Start()
            {
                clrTimer?.Start();
            }

            public void Stop()
            {
                clrTimer?.Stop();
            }
        }
    }
}