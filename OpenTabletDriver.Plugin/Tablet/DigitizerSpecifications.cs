using System.ComponentModel.DataAnnotations;

namespace OpenTabletDriver.Plugin.Tablet
{
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
        public float MaxX { set; get; }

        /// <summary>
        /// The maximum Y coordinate for the digitizer.
        /// </summary>
        [Required(ErrorMessage = $"Digitizer ${nameof(MaxY)} must be defined")]
        public float MaxY { set; get; }
    }
}
