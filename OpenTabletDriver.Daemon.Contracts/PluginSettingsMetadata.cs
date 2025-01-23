using System.Collections.Generic;
using Newtonsoft.Json;

namespace OpenTabletDriver.Daemon.Contracts
{
    public class PluginSettingMetadata
    {
        [JsonConstructor]
        public PluginSettingMetadata(string propertyName, string? friendlyName, string? shortDescription, string? longDescription, SettingType type)
        {
            PropertyName = propertyName;
            FriendlyName = friendlyName ?? propertyName;
            ShortDescription = shortDescription;
            LongDescription = longDescription;
            Type = type;
        }

        public string PropertyName { get; set; }
        public string FriendlyName { get; set; }
        public string? ShortDescription { get; set; }
        public string? LongDescription { get; set; }
        public SettingType Type { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = new();

        public PluginSettingMetadata Default(object? value)
        {
            Attributes.Add("default", value?.ToString() ?? "null");
            return this;
        }

        public PluginSettingMetadata Range(float min, float max, float step)
        {
            Attributes.Add("min", min.ToString());
            Attributes.Add("max", max.ToString());
            Attributes.Add("step", step.ToString());
            return this;
        }

        public PluginSettingMetadata Enum(params string[] choices)
        {
            Attributes.Add("choices", string.Join(";", choices));
            return this;
        }

        public PluginSettingMetadata Unit(string unit)
        {
            Attributes.Add("unit", unit);
            return this;
        }
    }

    public enum SettingType
    {
        Unknown,
        Boolean,
        Integer,
        Number,
        String,
    }
}
