using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace OpenTabletDriver.Plugin.Tablet
{
    public class PenSpecifications
    {
        /// <summary>
        /// The maximum pressure that the pen supports.
        /// </summary>
        [Required(ErrorMessage = $"Pen {nameof(MaxPressure)} must be defined")]
        public uint MaxPressure { set; get; }

        /// <summary>
        /// Specifications for the pen buttons.
        /// </summary>
        public uint? ButtonCount { set; get; }

        private bool _legacyButtonsHaveBeenSet;

        [Obsolete(Globals.LegacyTabletConfigurationProperty)]
        [JsonIgnore]
        public ButtonSpecifications Buttons
        {
            set
            {
                _legacyButtonsHaveBeenSet = true;
                ButtonCount = value.ButtonCount;
            }
            get => new() { ButtonCount = ButtonCount ?? 0 };
        }

        // hack which allows us to deserialize the object for backwards compatibility, but not emit it in serialization
        [JsonProperty("Buttons")]
        private ButtonSpecifications Buttons2
        {
#pragma warning disable CS0618 // Type or member is obsolete
            set => Buttons = value;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public bool HasLegacyProperties() => _legacyButtonsHaveBeenSet;
    }
}
