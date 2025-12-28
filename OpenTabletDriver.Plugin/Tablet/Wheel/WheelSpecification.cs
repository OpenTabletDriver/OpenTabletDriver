namespace OpenTabletDriver.Plugin.Tablet.Wheel
{
    /// <summary>
    /// Describes wheel hardware capabilities (relative/absolute, step count, etc.)
    /// </summary>
    public class WheelSpecification
    {
        /// <summary>
        /// True if the wheel reports relative deltas (e.g. scroll wheel).
        /// </summary>
        public bool IsRelative { get; set; }

        /// <summary>
        /// True if the wheel reports absolute positions (e.g. dial with fixed range).
        /// </summary>
        public bool IsAbsolute { get; set; }

        /// <summary>
        /// Number of steps or resolution of the wheel, if applicable.
        /// </summary>
        public int StepCount { get; set; }

        /// <summary>
        /// Optional button count for wheels that have a center button.
        /// </summary>
        public int ButtonCount { get; set; }
    }
}
