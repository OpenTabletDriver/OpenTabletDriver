using System.IO;
using Newtonsoft.Json;
using OpenTabletDriver.Daemon.Converters;
using OpenTabletDriver.Daemon.Contracts.Json;

namespace OpenTabletDriver.Daemon
{
    public static class Serialization
    {
        static Serialization()
        {
            Serializer.Error += SerializationErrorHandler;
            Serializer.Converters.Add(new VersionConverter());
        }

        internal static JsonSerializer Serializer { get; } = new AdvancedJsonSerializer
        {
            Formatting = Formatting.Indented
        };

        private static void SerializationErrorHandler(object? sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
        {
            Log.Exception(args.ErrorContext.Error);
            args.ErrorContext.Handled = true;
        }

        public static T? Deserialize<T>(FileInfo file)
        {
            using (var fs = file.OpenRead())
                return Deserialize<T>(fs);
        }

        public static void Serialize(FileInfo file, object value)
        {
            if (file.Exists)
                file.Delete();

            using (var fs = file.Create())
                Serialize(fs, value);
        }

        public static T? Deserialize<T>(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            using (var jr = new JsonTextReader(sr))
                return Deserialize<T>(jr);
        }

        public static void Serialize(Stream stream, object value)
        {
            using (var sw = new StreamWriter(stream))
            using (var jw = new JsonTextWriter(sw))
                Serialize(jw, value);
        }

        public static T? Deserialize<T>(JsonTextReader textReader)
        {
            return Serializer.Deserialize<T>(textReader);
        }

        public static void Serialize(JsonTextWriter textWriter, object value)
        {
            Serializer.Serialize(textWriter, value);
        }
    }
}
