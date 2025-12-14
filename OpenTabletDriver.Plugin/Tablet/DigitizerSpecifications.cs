using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace OpenTabletDriver.Plugin.Tablet
{
    public class DigitizerSpecifications
    {
        /// <summary>
        /// The width of the digitizer in millimeters.
        /// </summary>
        [JsonIgnore]
        public float Width
        {
            set => WidthAsDecimal = (decimal)value;
            get => (float)WidthAsDecimal;
        }

        /// <summary>
        /// Width decoded as `decimal` type
        ///
        /// TODO: On API bump, make 'Width' decimal type instead?
        /// </summary>
        [Required(ErrorMessage = $"Digitizer ${nameof(Height)} must be defined")]
        [JsonConverter(typeof(DecimalJsonConverter)), JsonProperty("Width")]
        public decimal WidthAsDecimal { set; get; }

        /// <summary>
        /// The height of the digitizer in millimeters.
        /// </summary>
        [JsonIgnore]
        public float Height
        {
            set => HeightAsDecimal = (decimal)value;
            get => (float)HeightAsDecimal;
        }

        /// <summary>
        /// Height decoded as `decimal` type
        ///
        /// TODO: On API bump, make 'Height' decimal type instead?
        /// </summary>
        [Required(ErrorMessage = $"Digitizer ${nameof(Width)} must be defined")]
        [JsonConverter(typeof(DecimalJsonConverter)), JsonProperty("Height")]
        public decimal HeightAsDecimal { set; get; }

        /// <summary>
        /// The maximum X coordinate for the digitizer.
        /// </summary>
        [Required(ErrorMessage = $"Digitizer ${nameof(MaxX)} must be defined")]
        [JsonConverter(typeof(DecimalJsonConverter))]
        public float MaxX { set; get; }

        /// <summary>
        /// The maximum Y coordinate for the digitizer.
        /// </summary>
        [Required(ErrorMessage = $"Digitizer ${nameof(MaxY)} must be defined")]
        [JsonConverter(typeof(DecimalJsonConverter))]
        public float MaxY { set; get; }
    }

    public class DecimalJsonConverter : JsonConverter
    {
        public override bool CanRead => false;
        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(double) || objectType == typeof(decimal);
        }

#nullable enable

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
            return value switch
            {
                float floatValue => floatValue == Math.Truncate(floatValue),
                decimal decimalValue => decimalValue == Math.Truncate(decimalValue),
                _ => false,
            };
        }
    }
}
