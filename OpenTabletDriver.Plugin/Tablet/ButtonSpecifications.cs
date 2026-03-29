using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OpenTabletDriver.Plugin.Tablet
{
    // TODO: On API bump, get rid of this
    public class ButtonSpecifications
    {
        /// <summary>
        /// The amount of buttons.
        /// </summary>
        [Required(ErrorMessage = $"{nameof(ButtonCount)} must be defined")]
        public uint ButtonCount { set; get; }

        /// <summary>
        /// Optional human-readable names for buttons, indexed by button number.
        /// </summary>
        public List<string> ButtonNames { set; get; }
    }
}
