namespace OpenTabletDriver.Plugin.Tablet.Wheel
{
    /// <summary>
    /// An auxiliary report containing states of a relative wheel/ring/strip input.
    /// </summary>
    public interface IAbsoluteWheelReport : ISingleAbsoluteAnalogReport, IRelativeSingleAnalogReport
    {
        /// <summary>
        /// The buttons related to the wheel.
        /// </summary>
        public bool[] WheelButtons { set; get; }
    }
}
