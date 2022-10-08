using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// A device report containing information for the pen.
    /// </summary>
    [PublicAPI]
    public interface ITabletReport : IAbsolutePositionReport
    {
        /// <summary>
        /// The current pressure level in device reported units.
        /// </summary>
        /// <remarks>
        /// This is converted by <see cref="PenSpecifications.MaxPressure"/> to get the pressure percentage.
        /// </remarks>
        uint Pressure { set; get; }

        /// <summary>
        /// The current states of the pen buttons.
        /// </summary>
        bool[] PenButtons { set; get; }
    }
}
