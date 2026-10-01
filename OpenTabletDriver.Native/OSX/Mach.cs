using System;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Native.OSX
{
    public static class Mach
    {
        private const int THREAD_TIME_CONSTRAINT_POLICY = 2;
        private const uint THREAD_TIME_CONSTRAINT_POLICY_COUNT = 4;

        private static readonly MachTimebaseInfo timebase = GetTimebase();

        [StructLayout(LayoutKind.Sequential)]
        private struct MachTimebaseInfo
        {
            public uint numer;
            public uint denom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ThreadTimeConstraintPolicy
        {
            public uint period;
            public uint computation;
            public uint constraint;
            public int preemptible;
        }

        [DllImport(LibSystem.SysLib)]
        private static extern uint mach_task_self();

        [DllImport(LibSystem.SysLib)]
        private static extern uint mach_thread_self();

        [DllImport(LibSystem.SysLib)]
        private static extern int mach_port_deallocate(uint task, uint name);

        [DllImport(LibSystem.SysLib)]
        private static extern int mach_timebase_info(out MachTimebaseInfo info);

        [DllImport(LibSystem.SysLib)]
        private static extern int thread_policy_set(uint thread, int flavor, ref ThreadTimeConstraintPolicy policy, uint count);

        /// <summary>
        /// Puts the calling thread under the Mach time-constraint (real-time) policy.
        /// </summary>
        /// <remarks>
        /// QoS classes can't be set on threads created by .NET (EPERM); this policy can.
        /// </remarks>
        /// <returns>The <c>kern_return_t</c> of <c>thread_policy_set</c>, 0 on success.</returns>
        public static int SetCurrentThreadTimeConstraint(TimeSpan period, TimeSpan computation, TimeSpan constraint)
        {
            var thread = mach_thread_self();
            var result = SetTimeConstraint(thread, period, computation, constraint);
            mach_port_deallocate(mach_task_self(), thread);
            return result;
        }

        private static int SetTimeConstraint(uint thread, TimeSpan period, TimeSpan computation, TimeSpan constraint)
        {
            uint ToAbsolute(TimeSpan span) =>
                (uint)Math.Min(uint.MaxValue, span.Ticks * 100.0 * timebase.denom / timebase.numer);

            var policy = new ThreadTimeConstraintPolicy
            {
                period = ToAbsolute(period),
                computation = ToAbsolute(computation),
                constraint = ToAbsolute(constraint),
                preemptible = 1
            };
            return thread_policy_set(thread, THREAD_TIME_CONSTRAINT_POLICY, ref policy, THREAD_TIME_CONSTRAINT_POLICY_COUNT);
        }

        private static MachTimebaseInfo GetTimebase()
        {
            mach_timebase_info(out var info);
            return info;
        }
    }
}
