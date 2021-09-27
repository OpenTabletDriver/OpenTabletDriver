using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using OpenTabletDriver.Plugin.Components;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.ComponentProviders
{
    public class DeviceConfigurationProvider : IDeviceConfigurationProvider
    {
        public DeviceConfigurationProvider()
        {
            var asm = typeof(Driver).Assembly;
            var jsonSerializer = new JsonSerializer();

            TabletConfigurations = asm.GetManifestResourceNames()
                .Where(path => path.Contains(".json"))
                .Select(path => Deserialize(jsonSerializer, asm.GetManifestResourceStream(path)))
                .ToArray();
        }

        public IEnumerable<TabletConfiguration> TabletConfigurations { get; }

        private static TabletConfiguration Deserialize(JsonSerializer jsonSerializer, Stream stream)
        {
            using var reader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(reader);
            return jsonSerializer.Deserialize<TabletConfiguration>(jsonReader);
        }
    }
}