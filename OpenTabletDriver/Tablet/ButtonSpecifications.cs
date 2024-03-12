using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// Device specifications for buttons.
    /// </summary>
    [PublicAPI]
    public class ButtonSpecifications
    {
        /// <summary>
        /// The amount of buttons.
        /// </summary>
        [DisplayName("Buttons")]
        [Required(ErrorMessage = $"{nameof(ButtonCount)} must be defined")]
        public uint ButtonCount { set; get; }
    }
}
