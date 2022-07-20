using System.Collections.Generic;
using Newtonsoft.Json;
using StreamJsonRpc;

#nullable enable

namespace OpenTabletDriver.Desktop.RPC.Messages
{
    public class MessageFormatter : JsonMessageFormatter
    {
        public MessageFormatter(IEnumerable<JsonConverter> additionalConverters)
        {
            foreach (var converter in additionalConverters)
            {
                JsonSerializer.Converters.Add(converter);
            }
        }
    }
}
