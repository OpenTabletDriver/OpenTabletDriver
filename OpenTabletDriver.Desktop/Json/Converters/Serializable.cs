using System;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

#nullable enable

namespace OpenTabletDriver.Desktop.Json.Converters
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
