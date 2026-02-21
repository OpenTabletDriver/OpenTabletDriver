using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.RPC
{
    public class DebugReportData
    {
        [JsonConstructor]
        public DebugReportData()
        {
        }

        [SetsRequiredMembers]
        public DebugReportData(TabletReference tablet, IDeviceReport report)
        {
            this.Tablet = tablet;
            this.Data = JToken.FromObject(report);
            this.Path = report.GetType().FullName ?? throw new InvalidOperationException("Could not determine path of type");
        }

        public required TabletReference Tablet { set; get; }
        public required string Path { set; get; }
        public required JToken Data { set; get; }

        public object? ToObject()
        {
            var type = AppInfo.PluginManager.PluginTypes.First(t => t.FullName == Path);
            return Data.ToObject(type);
        }
    }
}
