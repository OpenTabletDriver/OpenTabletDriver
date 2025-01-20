namespace OpenTabletDriver.Plugin.Tablet.Wheel
{
    /// <summary>
    /// A report containing aux keys related to a wheel.
    /// </summary>
    public interface IWheelButtonReport
    {
        /// <summary>
        /// The buttons related to the wheel.
        /// </summary>
        public bool[] WheelButtons { set; get; }
    }
}