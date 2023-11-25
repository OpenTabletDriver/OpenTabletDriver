using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

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
        public float Width { set; get; }

        /// <summary>
        /// The height of the digitizer in millimeters.
        /// </summary>
        [Required(ErrorMessage = $"Digitizer ${nameof(Height)} must be defined")]
        public float Height { set; get; }

        /// <summary>
        /// The maximum X coordinate for the digitizer.
        /// </summary>
        [Required(ErrorMessage = $"Digitizer ${nameof(MaxX)} must be defined")]
        [DisplayName("Max X")]
        public float MaxX { set; get; }

        /// <summary>
        /// The maximum Y coordinate for the digitizer.
        /// </summary>
        [Required(ErrorMessage = $"Digitizer ${nameof(MaxY)} must be defined")]
        [DisplayName("Max Y")]
        public float MaxY { set; get; }
    }
}
