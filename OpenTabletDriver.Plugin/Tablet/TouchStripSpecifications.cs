using System.ComponentModel.DataAnnotations;

namespace OpenTabletDriver.Plugin.Tablet
{
    public class TouchStripSpecifications
    {
        /// <summary>
        /// The amount of touch strips.
        /// </summary>
        [Required(ErrorMessage = $"{nameof(Count)} must be defined")]
        public uint Count { set; get; }
    }
}
