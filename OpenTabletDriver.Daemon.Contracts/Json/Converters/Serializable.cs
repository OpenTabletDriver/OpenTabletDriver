using System;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace OpenTabletDriver.Daemon.Contracts.Json.Converters
{
    [JsonObject]
    internal abstract class Serializable
    {
        protected static Exception NotSupported([CallerMemberName] string? memberName = null)
        {
            return new NotSupportedException($"\"{memberName}\" cannot be used in a serialized proxy.");
        }

        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}
