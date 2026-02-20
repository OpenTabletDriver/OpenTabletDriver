using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using OpenTabletDriver.Plugin.Components;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations
{
    public class DeviceConfigurationProvider : IDeviceConfigurationProvider
    {
        public DeviceConfigurationProvider()
        {
            var asm = typeof(DeviceConfigurationProvider).Assembly;

            TabletConfigurations = asm.GetManifestResourceNames()
                .Where(path => path.Contains(".json"))
                .Select(path => Deserialize(asm.GetManifestResourceStream(path)))
                .ToArray();
        }

        public IEnumerable<TabletConfiguration> TabletConfigurations { get; }

        private readonly JsonSerializer jsonSerializer = new JsonSerializer();

        private TabletConfiguration Deserialize(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            using var reader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(reader);
            return jsonSerializer.Deserialize<TabletConfiguration>(jsonReader);
        }
    }
}
