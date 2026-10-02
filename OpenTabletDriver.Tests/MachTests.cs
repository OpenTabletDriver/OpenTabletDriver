using System;
using System.Runtime.Versioning;
using System.Threading;
using OpenTabletDriver.Native.OSX;
using Xunit;

namespace OpenTabletDriver.Tests
{
    [SupportedOSPlatform("macos")]
    public class MachTests
    {
        private static readonly TimeSpan Computation = TimeSpan.FromMilliseconds(1);
        private static readonly TimeSpan Constraint = TimeSpan.FromMilliseconds(2);

        [SkippableFact]
        public void TimeConstraintPolicy_IsAcceptedForManagedThread()
        {
            int result = -1;
            var thread = new Thread(() => result = Mach.SetCurrentThreadTimeConstraint(TimeSpan.Zero, Computation, Constraint));
            thread.Start();
            thread.Join();

            Assert.Equal(0, result);
        }

        [SkippableFact]
        public void TimeConstraintPolicy_FindsOnlyNewThreadByName()
        {
            const string name = "OTD Test Named Thread";
            using var stop = new ManualResetEventSlim();
            StartNamedThread(name, stop);
            try
            {
                var existingThreadIds = Mach.GetThreadIds();
                Assert.False(Mach.TrySetNewNamedThreadTimeConstraint(name, existingThreadIds, Computation, Constraint, out _));

                StartNamedThread(name, stop);
                Assert.True(Mach.TrySetNewNamedThreadTimeConstraint(name, existingThreadIds, Computation, Constraint, out var result));
                Assert.Equal(0, result);

                Assert.False(Mach.TrySetNewNamedThreadTimeConstraint(name, Mach.GetThreadIds(), Computation, Constraint, out _));
            }
            finally
            {
                stop.Set();
            }
        }

        private static void StartNamedThread(string name, ManualResetEventSlim stop)
        {
            // Not disposed: the thread may still be inside Set() when Wait() returns.
            var started = new ManualResetEventSlim();
            var thread = new Thread(() => { started.Set(); stop.Wait(); }) { Name = name, IsBackground = true };
            thread.Start();
            started.Wait();
        }
    }
}
