using System;
using System.Numerics;
using CommunityToolkit.HighPerformance.Buffers;
using OpenTabletDriver.Plugin.Tablet.Touch;

namespace OpenTabletDriver.Plugin.Tablet
{
    /// <summary>
    /// The core reference struct used for handling device reports.
    /// New features must be added directly to this struct.
    /// </summary>
    public ref struct DeviceReport(ReadOnlySpan<byte> raw)
    {
        public readonly ReadOnlySpan<byte> Raw = raw;
        public Type Parser;

        /// <summary>
        /// Can be set to false while parsing to prevent report from entering pipeline.
        /// May be checked multiple times throughout the pipeline to stop it from propagating further
        /// TODO: Determine exact behavior - should it block pipeline, or should Parse(data, ref report) return value be used for this instead?
        /// </summary>
        public bool Continue = true;

        /// <summary>
        /// The pen has left detection range, and pipeline should clear relevant states (buttons released, etc).
        /// </summary>
        public bool OutOfRange;

        /// <summary>
        /// The exact position reported by the pen.
        /// <para/>
        /// Pre-transpose, this should be in the range of <c>{0..x}</c>, where <c>x</c> is the maximum position defined in the respective tablet configuration.
        /// <para/>
        /// Post-transpose, <c>x</c> instead refers to the expected maximum position of the output mode.
        /// </summary>
        /// <remarks>
        /// The transpose step often changes this to fit the output mapping.
        /// May otherwise be adjusted throughout the pipeline, via plugins.
        /// </remarks>
        public Vector2? PenPosition; // IAbsolutePositionReport

        // ITabletReport
        public uint? PenPressure;
        public MemoryOwner<bool>? PenButtons;

        // IProximityReport
        public bool? NearProximity;
        public uint? HoverDistance;

        // TODO: redefine range to [-1..1] or similar, do it on the other side of finishing megastruct
        public Vector2? Tilt; // ITiltReport

        public bool? Eraser; // IEraserReport

        /// <summary>
        /// The buttons per wheel, e.g. <c>[[True], [False]]</c> for multi-wheel.
        /// Or <c>[[True]]</c> for single wheel
        /// </summary>
        // TODO: surely there's a better way to do 2D MemoryOwner arrays
        public MemoryOwner<MemoryOwner<bool>>? WheelButtons; // IWheelButtonReport

        // IToolReport
        public ulong? ToolSerial;
        public uint? RawToolID;
        public ToolType? ToolType;

        /// <summary>
        /// Raw rotation data from the tablet.
        /// </summary>
        /// <remarks>
        /// The rotation range is stored in the tablet's config as <see cref="PenSpecifications.MinRotation"/> and <see cref="PenSpecifications.MaxRotation"/>.
        /// </remarks>
        public int? Rotation { set; get; } // IRotationReport

        public MemoryOwner<bool>? AuxButtons; // IAuxReport

        /// <summary>
        /// The absolute position readings of an input, or null to indicate no relevant reading for this report.
        /// For example, a touch-based absolute wheel may report no touch or no state change.
        /// </summary>
        public MemoryOwner<uint?>? WheelPositions; // IAbsoluteAnalogReport

        /// <summary>
        /// The delta in analog gadget positions. 0 equals no movement
        /// </summary>
        public MemoryOwner<int>? WheelDeltas; // IRelativeAnalogReport

        // IMouseReport
        public Vector2? MousePosition;
        public MemoryOwner<bool>? MouseButtons;
        public Vector2? MouseScroll;

        public MemoryOwner<TouchPoint?>? Touches; // ITouchReport

        public void Dispose()
        {
            if (WheelButtons != null)
                foreach (var wheelButton in WheelButtons.Span)
                    wheelButton.Dispose();
            WheelButtons?.Dispose();
            AuxButtons?.Dispose();
            Touches?.Dispose();
        }
    }
}
