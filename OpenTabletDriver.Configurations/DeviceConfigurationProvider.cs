using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using OpenTabletDriver.Components;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations
{
    [PublicAPI]
    public class DeviceConfigurationProvider : IDeviceConfigurationProvider
    {
        public DeviceConfigurationProvider()
        {
            var asm = typeof(DeviceConfigurationProvider).Assembly;
            var jsonSerializer = new JsonSerializer();

            TabletConfigurations = asm.GetManifestResourceNames()
                .Where(path => path.Contains(".json"))
                .Select(path => Deserialize(jsonSerializer, asm.GetManifestResourceStream(path)!))
                .ToArray();
        }

        public IEnumerable<TabletConfiguration> TabletConfigurations { get; }

        private static TabletConfiguration Deserialize(JsonSerializer jsonSerializer, Stream stream)
        {
            using var reader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(reader);
            return jsonSerializer.Deserialize<TabletConfiguration>(jsonReader)!;
        }
    }
}
