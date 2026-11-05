using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Xml;
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
        public DebugReportData(TabletReference tablet, scoped in DeviceReport report)
        {
            this.Tablet = tablet;
            this.Data = [.. report.Raw];
            this.Path = report.Parser.FullName ?? throw new InvalidOperationException("Could not determine path of type");
        }

        public required TabletReference Tablet { set; get; }
        public required string Path { set; get; }
        public required byte[] Data { set; get; }

        // TODO/FIXME: this isn't perfect for stateful parsers, but given that all(?) parsers can be reset by pressing all buttons and then removing pens/hands from tablet, this shouldn't be a huge problem
        //   It can however cause some surprising results - if this is important for tablet data recording, data recording needs to be implemented daemon-side (doesn't have to be into a file, can just asynchronously return strings?)
        public bool Reinit(scoped ref DeviceReport report)
        {
            var parser = AppInfo.PluginManager.ConstructObject<IReportParser>(Path) ??
                         throw new InvalidOperationException($"Could not look up parser for path '{Path}'");
            return parser.Parse(Data, ref report);
        }
    }
}
