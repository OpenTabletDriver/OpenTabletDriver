namespace OpenTabletDriver.Plugin.Tablet.Wheel
{
    /// <summary>
    /// An auxiliary report containing states of a Absolute wheel/ring/strip input.
    /// </summary>
    public interface IAbsoluteWheelReport : IAbsoluteSingleAnalogReport, IRelativeSingleAnalogReport
    {
        /// <summary>
        /// The buttons related to the wheel.
        /// </summary>
        public bool[] WheelButtons { set; get; }
    }
}
