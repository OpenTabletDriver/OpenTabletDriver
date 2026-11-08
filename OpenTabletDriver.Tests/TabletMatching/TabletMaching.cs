using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenTabletDriver.Plugin.Devices;
using Xunit;

// NOTE: This file uses System.Text.Json instead of Newtonsoft.Json to test source gen capabilities -- doesn't source gen from existing json!

namespace OpenTabletDriver.Tests.TabletMatching
{
    // TODO: would probably make more sense to call this TestTabletIdentifier and then in other functions return a List<TestTabletIdentifier> to support multiple devices in HID lists?
    /// <summary>
    /// A record of a test case for a single tablet
    /// </summary>
    /// <param name="DetectionName">The name the tablet should be detected as</param>
    /// <param name="DeviceIdentifiers">The relevant list of HID Devices from an OpenTabletDriver diagnostics</param>
    /// <param name="DeviceStrings">A manually dumped list of strings</param>
    /// <remarks>At the time of writing it may be helpful to generate this file with the following cli+jq oneliner:
    /// <code>
    /// $ otd getdiagnostics |\
    /// jq '{ "DetectionName": "FIXME, optionally clean up identifiers", "DeviceIdentifiers": ."HID Devices", DeviceStrings: {}}'
    /// </code>
    /// </remarks>
    public record struct SingleTabletIdentifier(
        string DetectionName,
        IReadOnlyCollection<SerializedDeviceEndpoint> DeviceIdentifiers,
        IReadOnlyDictionary<byte, string> DeviceStrings
    );

    [JsonSourceGenerationOptions(PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate)]
    [JsonSerializable(typeof(SingleTabletIdentifier))]
    internal partial class SerializationModeOptionsContext : JsonSerializerContext;

    public static class TabletMatching
    {
        public static TheoryData<SingleTabletIdentifier> SingleTabletIdentifiers { get; } = GetSingleTabletIdentifiers();

        private static TheoryData<SingleTabletIdentifier> GetSingleTabletIdentifiers()
        {
            var result = new TheoryData<SingleTabletIdentifier>();

            foreach (string testFile in Directory.EnumerateFiles(GetSingleTabletIdentifiersDir(), "*.json"))
            {
                var reader = new Utf8JsonReader(File.ReadAllBytes(testFile));
                result.Add(JsonSerializer.Deserialize(ref reader, SerializationModeOptionsContext.Default.SingleTabletIdentifier));
            }

            return result;
        }

        private static string GetSingleTabletIdentifiersDir([CallerFilePath] string sourceFilePath = "") =>
            Path.GetFullPath(Path.Join(Path.GetDirectoryName(sourceFilePath), nameof(SingleTabletIdentifiers)));
    }
}
