using System.ComponentModel.DataAnnotations;

namespace OpenTabletDriver.Plugin.Tablet
{
    public class ButtonSpecifications
    {
        /// <summary>
        /// The amount of buttons.
        /// </summary>
        [Required(ErrorMessage = $"{nameof(ButtonCount)} must be defined")]
        public uint ButtonCount { set; get; }
    }
}
