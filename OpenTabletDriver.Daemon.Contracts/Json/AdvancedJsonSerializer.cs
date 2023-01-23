using Newtonsoft.Json;

namespace OpenTabletDriver.Daemon.Contracts.Json
{
    public sealed class AdvancedJsonSerializer : JsonSerializer
    {
        public AdvancedJsonSerializer()
        {
            foreach (var converter in Utilities.Converters)
            {
                Converters.Add(converter);
            }
        }
    }
}
