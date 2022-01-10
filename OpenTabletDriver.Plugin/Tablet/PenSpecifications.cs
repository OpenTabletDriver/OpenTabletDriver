namespace OpenTabletDriver.Plugin.Tablet
{
    public class PenSpecifications
    {
        /// <summary>
        /// The maximum pressure that the pen supports.
        /// </summary>
        public uint MaxPressure { set; get; }

        /// <summary>
        /// Specifications for the pen buttons.
        /// </summary>
        public ButtonSpecifications Buttons { set; get; } = new ButtonSpecifications();
    }
}
