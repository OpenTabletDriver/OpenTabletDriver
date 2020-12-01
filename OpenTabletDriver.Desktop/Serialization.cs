using System.IO;
using Newtonsoft.Json;

namespace OpenTabletDriver.Desktop
{
    public static class Serialization
    {
        private static readonly JsonSerializer serializer = new JsonSerializer
        {
            Formatting = Formatting.Indented
        };

        public static T Deserialize<T>(JsonTextReader textReader)
        {
            return serializer.Deserialize<T>(textReader);
        }

        public static void Serialize(JsonTextWriter textWriter, object value)
        {
            serializer.Serialize(textWriter, value);
        }

        public static T Deserialize<T>(FileInfo file)
        {
            using (var stream = file.OpenRead())
            using (var sr = new StreamReader(stream))
            using (var jr = new JsonTextReader(sr))
                return Deserialize<T>(jr);
        }

        public static void Serialize(FileInfo file, object value)
        {
            if (file.Exists)
                file.Delete();

            using (var sw = file.CreateText())
            using (var jw = new JsonTextWriter(sw))
                Serialize(jw, value);
        }
    }
}
