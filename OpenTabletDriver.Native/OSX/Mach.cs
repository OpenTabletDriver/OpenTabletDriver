using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenTabletDriver.Native.OSX
{
    /// <summary>
    /// Bindings to the Mach (macOS kernel) thread scheduling calls.
    /// </summary>
    public static partial class Mach
    {
        // From <mach/thread_policy.h>.
        // 2 is the ID of the time-constraint (real-time) policy.
        private const int THREAD_TIME_CONSTRAINT_POLICY = 2;
        // 4 is how many fields the policy struct has; the kernel uses it to check that the struct is complete.
        private const uint THREAD_TIME_CONSTRAINT_POLICY_COUNT = 4;

        // From <sys/proc_info.h>: 64 is the longest thread name macOS stores.
        private const int MAXTHREADNAMESIZE = 64;

        private static readonly MachTimebaseInfo timebase = GetTimebase();

        /// <summary>
        /// The ratio that converts Mach absolute time units to nanoseconds.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private record struct MachTimebaseInfo(uint Numer, uint Denom);

        /// <summary>
        /// Mirrors <c>thread_time_constraint_policy</c>. Times are in Mach absolute time units.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private record struct ThreadTimeConstraintPolicy(
            uint Period,
            uint Computation,
            uint Constraint,
            int Preemptible
        );

        [LibraryImport(LibSystem.SysLib)]
        private static partial uint mach_task_self();

        [LibraryImport(LibSystem.SysLib)]
        private static partial uint mach_thread_self();

        [LibraryImport(LibSystem.SysLib)]
        private static partial int mach_port_deallocate(uint task, uint name);

        [LibraryImport(LibSystem.SysLib)]
        private static partial int mach_timebase_info(out MachTimebaseInfo info);

        [LibraryImport(LibSystem.SysLib)]
        private static partial int thread_policy_set(uint thread, int flavor, ref ThreadTimeConstraintPolicy policy, uint count);

        [LibraryImport(LibSystem.SysLib)]
        private static partial int task_threads(uint task, out IntPtr threads, out uint count);

        [LibraryImport(LibSystem.SysLib)]
        private static partial int vm_deallocate(uint task, nuint address, nuint size);

        [LibraryImport(LibSystem.SysLib)]
        private static partial IntPtr pthread_from_mach_thread_np(uint thread);

        [LibraryImport(LibSystem.SysLib)]
        private static partial int pthread_getname_np(IntPtr thread, [Out] byte[] name, nuint length);

        [LibraryImport(LibSystem.SysLib)]
        private static partial int pthread_threadid_np(IntPtr thread, out ulong id);

        /// <summary>
        /// Puts the calling thread under the Mach time-constraint (real-time) policy.
        /// </summary>
        /// <remarks>
        /// The thread asks for up to <paramref name="computation"/> of CPU time, finished within
        /// <paramref name="constraint"/> after it wakes; a zero <paramref name="period"/> means it wakes on events rather
        /// than at a fixed rate. macOS's usual priority mechanism, QoS classes, can't be set on threads .NET creates
        /// (EPERM); this policy can.
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
        /// Returns the unique IDs of the threads currently in this process.
        /// </summary>
        public static IReadOnlySet<ulong> GetThreadIds()
        {
            var ids = new HashSet<ulong>();
            VisitThreads((_, pthread) =>
            {
                if (pthread_threadid_np(pthread, out var id) == 0)
                    ids.Add(id);
                return false;
            });
            return ids;
        }

        /// <summary>
        /// Sets the time-constraint policy, without a period, on the first thread with the given name that is not in
        /// <paramref name="existingThreadIds"/>, i.e. one started after those IDs were taken with <see cref="GetThreadIds"/>.
        /// </summary>
        /// <param name="result">The <c>kern_return_t</c> of <c>thread_policy_set</c>, 0 on success.</param>
        /// <returns><c>true</c> if such a thread was found.</returns>
        public static bool TrySetNewNamedThreadTimeConstraint(string name, IReadOnlySet<ulong> existingThreadIds,
            TimeSpan computation, TimeSpan constraint, out int result)
        {
            var buffer = new byte[MAXTHREADNAMESIZE];
            int? setResult = null;
            VisitThreads((thread, pthread) =>
            {
                if (pthread_threadid_np(pthread, out var id) != 0 || existingThreadIds.Contains(id)
                    || GetThreadName(pthread, buffer) != name)
                    return false;

                setResult = SetTimeConstraint(thread, TimeSpan.Zero, computation, constraint);
                return true;
            });
            result = setResult ?? 0;
            return setResult.HasValue;
        }

        /// <summary>
        /// Calls <paramref name="visit"/> with the Mach port and pthread of each thread in this process, until it returns
        /// <c>true</c>.
        /// </summary>
        private static void VisitThreads(Func<uint, IntPtr, bool> visit)
        {
            var task = mach_task_self();
            if (task_threads(task, out var threads, out var count) != 0)
                return;

            try
            {
                for (int i = 0; i < count; i++)
                {
                    var thread = ThreadAt(threads, i);
                    var pthread = pthread_from_mach_thread_np(thread);
                    if (pthread != IntPtr.Zero && visit(thread, pthread))
                        return;
                }
            }
            finally
            {
                for (int i = 0; i < count; i++)
                    mach_port_deallocate(task, ThreadAt(threads, i));
                vm_deallocate(task, (nuint)threads, count * sizeof(uint));
            }
        }

        private static uint ThreadAt(IntPtr threads, int index) => (uint)Marshal.ReadInt32(threads, index * sizeof(uint));

        private static string? GetThreadName(IntPtr pthread, byte[] buffer)
        {
            if (pthread_getname_np(pthread, buffer, (nuint)buffer.Length) != 0)
                return null;

            var length = Array.IndexOf(buffer, (byte)0);
            return Encoding.UTF8.GetString(buffer, 0, length < 0 ? buffer.Length : length);
        }

        private static int SetTimeConstraint(uint thread, TimeSpan period, TimeSpan computation, TimeSpan constraint)
        {
            uint ToAbsolute(TimeSpan span) =>
                (uint)Math.Min(uint.MaxValue, span.TotalNanoseconds * timebase.Denom / timebase.Numer);

            var policy = new ThreadTimeConstraintPolicy(
                Period: ToAbsolute(period),
                Computation: ToAbsolute(computation),
                Constraint: ToAbsolute(constraint),
                Preemptible: 1
            );
            return thread_policy_set(thread, THREAD_TIME_CONSTRAINT_POLICY, ref policy, THREAD_TIME_CONSTRAINT_POLICY_COUNT);
        }

        private static MachTimebaseInfo GetTimebase()
        {
            mach_timebase_info(out var info);
            return info;
        }
    }
}
