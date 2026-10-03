using System;
using HidSharp;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Native.OSX;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Devices;

namespace OpenTabletDriver.Devices.HidSharpBackend
{
    public sealed class HidSharpEndpointStream : IDeviceEndpointStream
    {
        internal HidSharpEndpointStream(HidStream stream)
        {
            this.stream = stream;
            stream.ReadTimeout = int.MaxValue;
        }

        private HidStream stream;
        private bool realtimePolicyPending = SystemInterop.CurrentPlatform == PluginPlatform.MacOS;

        public byte[] Read()
        {
            var data = stream.Read();
            if (realtimePolicyPending)
            {
                realtimePolicyPending = false;
                SetMacOSRealtimePolicy();
            }
            return data;
        }

        public void Write(byte[] buffer) => stream.Write(buffer);

        public void GetFeature(byte[] buffer) => stream.GetFeature(buffer);
        public void SetFeature(byte[] buffer) => stream.SetFeature(buffer);

        public void Dispose() => stream.Dispose();

        // Under CPU load both threads a report passes fall behind: HidSharp's "HID Reader" and the device reader calling Read().
        // TODO: Set the policy where HIDSharpCore's MacHidStream creates its reader thread, instead of finding it by name.
        private static void SetMacOSRealtimePolicy()
        {
            var computation = TimeSpan.FromMilliseconds(1);
            var constraint = TimeSpan.FromMilliseconds(2);

            var result = Mach.SetCurrentThreadTimeConstraint(TimeSpan.Zero, computation, constraint);
            if (result != 0)
                Log.Write("Device", $"Failed to set real-time thread policy: kern_return {result}", LogLevel.Warning);

            if (!Mach.SetNamedThreadsTimeConstraint("HID Reader", computation, constraint))
                Log.Write("Device", "Failed to set real-time thread policy on HidSharp's 'HID Reader' thread", LogLevel.Warning);
        }
    }
}
