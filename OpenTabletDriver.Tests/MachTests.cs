using System;
using System.Threading;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Native.OSX;
using OpenTabletDriver.Plugin;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class MachTests
    {
        private static readonly TimeSpan Computation = TimeSpan.FromMilliseconds(1);
        private static readonly TimeSpan Constraint = TimeSpan.FromMilliseconds(2);

        [SkippableFact]
        public void TimeConstraintPolicy_IsAcceptedForManagedThread()
        {
            Skip.IfNot(SystemInterop.CurrentPlatform == PluginPlatform.MacOS);

            int result = -1;
            var thread = new Thread(() => result = Mach.SetCurrentThreadTimeConstraint(TimeSpan.Zero, Computation, Constraint));
            thread.Start();
            thread.Join();

            Assert.Equal(0, result);
        }

        [SkippableFact]
        public void TimeConstraintPolicy_FindsAnotherThreadByName()
        {
            Skip.IfNot(SystemInterop.CurrentPlatform == PluginPlatform.MacOS);

            using var stop = new ManualResetEventSlim();
            using var started = new ManualResetEventSlim();
            var thread = new Thread(() => { started.Set(); stop.Wait(); }) { Name = "OTD Test Named Thread", IsBackground = true };
            thread.Start();
            started.Wait();
            try
            {
                Assert.True(Mach.SetNamedThreadsTimeConstraint("OTD Test Named Thread", Computation, Constraint));
                Assert.False(Mach.SetNamedThreadsTimeConstraint("OTD No Such Thread", Computation, Constraint));
            }
            finally
            {
                stop.Set();
            }
        }
    }
}
