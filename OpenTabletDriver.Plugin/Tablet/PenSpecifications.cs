using System.ComponentModel.DataAnnotations;

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
        [Required(ErrorMessage = $"{nameof(Buttons)} must be defined")]
        public ButtonSpecifications Buttons { set; get; } = new ButtonSpecifications();
    }
}
