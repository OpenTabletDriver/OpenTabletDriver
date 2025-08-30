namespace OpenTabletDriver.Plugin.Platform.Pointer;

public enum PenAction
{
    /// <summary>
    /// The tip of the pen
    /// Pressure usually included in the tablet report
    ///
    /// Mouse output action: Left click
    /// Tablet output action: "Pen touches tablet", unless handled by pressure
    /// </summary>
    Tip,

    /// <summary>
    /// The opposite end of the pen
    /// May include pressure in the tablet report
    ///
    /// The significant majority of tablets do not have a dedicated eraser button reading,
    /// but rather uses a flag in the report, or a separate tool report
    ///
    /// Mouse output action: Left click
    /// Tablet output action: "Pen touches tablet", unless handled by pressure
    /// </summary>
    Eraser,

    /// <summary>
    /// First button on the pen barrel from the tip
    ///
    /// Mouse output action: Right click
    /// Tablet output action: Right click equivalent, or first button
    /// </summary>
    BarrelButton1,

    /// <summary>
    /// Second button on the pen barrel from the tip
    /// Not always present
    ///
    /// Mouse output action: Middle click
    /// Tablet output action: Middle click equivalent
    /// </summary>
    BarrelButton2,

    /// <summary>
    /// Third button on the pen barrel from the tip
    ///
    /// Mouse output action: No-op
    /// Tablet output action: Undefined
    ///
    /// Linux Artist Mode does implement support for this (by sending "tablet button 3"),
    /// but is not widely supported in end-user software
    /// </summary>
    BarrelButton3,
}
