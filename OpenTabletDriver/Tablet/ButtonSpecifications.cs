using System.ComponentModel;
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
        public uint ButtonCount { set; get; }
    }
}
