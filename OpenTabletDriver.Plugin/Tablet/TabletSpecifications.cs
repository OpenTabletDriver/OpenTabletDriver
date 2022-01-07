namespace OpenTabletDriver.Plugin.Tablet
{
    public class TabletSpecifications
    {
        /// <summary>
        /// Specifications for the tablet digitizer.
        /// </summary>
        public DigitizerSpecifications Digitizer { set; get; }

        /// <summary>
        /// Specifications for the tablet's pen.
        /// </summary>
        public PenSpecifications Pen { set; get; }

        /// <summary>
        /// Specifications for the auxiliary buttons.
        /// </summary>
        public ButtonSpecifications AuxiliaryButtons { set; get; }

        /// <summary>
        /// Specifications for the mouse buttons.
        /// </summary>
        public ButtonSpecifications MouseButtons { set; get; }

        /// <summary>
        /// Specifications for the touch digitizer.
        /// </summary>
        public DigitizerSpecifications Touch { set; get; }
    }
}
