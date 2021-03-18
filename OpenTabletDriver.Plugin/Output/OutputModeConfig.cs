namespace OpenTabletDriver.Plugin.Output
{
    public class OutputModeConfig : Notifier
    {
        private Area input, output;
        private bool areaLimiting, areaClipping;

        /// <summary>
        /// The area in which the tablet's input is transformed to.
        /// </summary>
        public Area Input
        {
            set => RaiseAndSetIfChanged(ref input, value);
            get => input;
        }

        /// <summary>
        /// The area in which the final processed output is transformed to.
        /// </summary>
        public Area Output
        {
            set => RaiseAndSetIfChanged(ref output, value);
            get => output;
        }

        /// <summary>
        /// Whether to stop accepting input outside of the assigned areas.
        /// </summary>
        /// <remarks>
        /// If true, <see cref="AreaClipping"/> is automatically implied true.
        /// </remarks>
        public bool AreaLimiting
        {
            set => RaiseAndSetIfChanged(ref areaLimiting, value);
            get => areaLimiting;
        }

        /// <summary>
        /// Whether to clip all tablet inputs to the assigned areas.
        /// </summary>
        /// <remarks>
        /// If false, input outside of the area can escape the assigned areas, but still will be transformed.
        /// If true, input outside of the area will be clipped to the edges of the assigned areas.
        /// </remarks>
        public bool AreaClipping
        {
            set => RaiseAndSetIfChanged(ref areaClipping, value);
            get => areaClipping;
        }
    }
}