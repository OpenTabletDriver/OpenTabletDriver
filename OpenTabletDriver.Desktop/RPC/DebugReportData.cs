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

        public DebugReportData(TabletReference tablet, IDeviceReport report)
        {
            this.Tablet = tablet;
            this.Data = JToken.FromObject(report);
            this.Path = report.GetType().FullName;
        }

        public TabletReference Tablet { set; get; }
        public string Path { set; get; }
        public JToken Data { set; get; }
        
        public object ToObject()
        {
            var type = AppInfo.PluginManager.Types.First(t => t.FullName == Path);
            return Data.ToObject(type);
        } 
    }
}