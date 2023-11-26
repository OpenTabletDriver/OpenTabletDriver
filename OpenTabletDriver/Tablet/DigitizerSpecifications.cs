using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// Specifications for a digitizer.
    /// </summary>
    [PublicAPI]
    public class DigitizerSpecifications
    {
        /// <summary>
        /// The width of the digitizer in millimeters.
        /// </summary>
        [Required(ErrorMessage = $"Digitizer ${nameof(Width)} must be defined")]
        [JsonConverter(typeof(DecimalJsonConverter))]
        public float Width { set; get; }

        /// <summary>
        /// The height of the digitizer in millimeters.
        /// </summary>
        [Required(ErrorMessage = $"Digitizer ${nameof(Height)} must be defined")]
        [JsonConverter(typeof(DecimalJsonConverter))]
        public float Height { set; get; }

        /// <summary>
        /// The maximum X coordinate for the digitizer.
        /// </summary>
        [Required(ErrorMessage = $"Digitizer ${nameof(MaxX)} must be defined")]
        [JsonConverter(typeof(DecimalJsonConverter))]
        [DisplayName("Max X")]
        public float MaxX { set; get; }

        /// <summary>
        /// The maximum Y coordinate for the digitizer.
        /// </summary>
        [Required(ErrorMessage = $"Digitizer ${nameof(MaxY)} must be defined")]
        [JsonConverter(typeof(DecimalJsonConverter))]
        [DisplayName("Max Y")]
        public float MaxY { set; get; }
    }

    public class DecimalJsonConverter : JsonConverter
    {
        public override bool CanRead => false;
        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(double);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
                throw new ArgumentNullException();

            writer.WriteRawValue(IsWholeValue(value)
                ? JsonConvert.ToString(Convert.ToInt64(value))
                : JsonConvert.ToString(value));
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        private static bool IsWholeValue(object value)
        {
            if (value is float floatValue)
                return floatValue == Math.Truncate(floatValue);

            return false;
        }
    }
}
