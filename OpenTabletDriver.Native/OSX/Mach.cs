using System;
using System.Runtime.InteropServices;
using System.Text;

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

        [DllImport(LibSystem.SysLib)]
        private static extern int task_threads(uint task, out IntPtr threads, out uint count);

        [DllImport(LibSystem.SysLib)]
        private static extern int vm_deallocate(uint task, nuint address, nuint size);

        [DllImport(LibSystem.SysLib)]
        private static extern IntPtr pthread_from_mach_thread_np(uint thread);

        [DllImport(LibSystem.SysLib)]
        private static extern int pthread_getname_np(IntPtr thread, byte[] name, nuint length);

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

        /// <summary>
        /// Puts every thread of this process with the given name under the time-constraint policy, without a period.
        /// </summary>
        /// <returns><c>true</c> if at least one thread has the name and the policy was set on all of them.</returns>
        public static bool SetNamedThreadsTimeConstraint(string name, TimeSpan computation, TimeSpan constraint)
        {
            var task = mach_task_self();
            if (task_threads(task, out var threads, out var count) != 0)
                return false;

            int found = 0;
            int set = 0;
            var buffer = new byte[64];
            try
            {
                for (int i = 0; i < count; i++)
                {
                    var thread = ThreadAt(threads, i);
                    if (GetThreadName(thread, buffer) != name)
                        continue;

                    found++;
                    if (SetTimeConstraint(thread, TimeSpan.Zero, computation, constraint) == 0)
                        set++;
                }
            }
            finally
            {
                for (int i = 0; i < count; i++)
                    mach_port_deallocate(task, ThreadAt(threads, i));
                vm_deallocate(task, (nuint)threads, count * sizeof(uint));
            }
            return found > 0 && set == found;
        }

        private static uint ThreadAt(IntPtr threads, int index) => (uint)Marshal.ReadInt32(threads, index * sizeof(uint));

        private static string? GetThreadName(uint thread, byte[] buffer)
        {
            var pthread = pthread_from_mach_thread_np(thread);
            if (pthread == IntPtr.Zero || pthread_getname_np(pthread, buffer, (nuint)buffer.Length) != 0)
                return null;

            var length = Array.IndexOf(buffer, (byte)0);
            return Encoding.UTF8.GetString(buffer, 0, length < 0 ? buffer.Length : length);
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
