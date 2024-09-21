using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    [PublicAPI]
    public class TouchStripSpecifications
    {
        /// <summary>
        /// The amount of touch strips.
        /// </summary>
        [DisplayName("Touch Strips Count")]
        [Required(ErrorMessage = $"{nameof(Count)} must be defined")]
        public uint Count { set; get; }
    }
}
