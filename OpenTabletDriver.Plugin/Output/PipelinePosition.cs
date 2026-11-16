using JetBrains.Annotations;

namespace OpenTabletDriver.Plugin.Output
{
    /// <summary>
    /// The position of the plugin relative to notable known stages in the pipeline.
    /// </summary>
    /// <remarks>There is no developer-side mechanism to force a specific ordering of a plugin within a pipeline position,
    /// instead this is decided by the end-user configuration (in v0.6, this is accidentally determined by ordering in user's settings file).</remarks>
    [PublicAPI]
    public enum PipelinePosition
    {
        /// <summary>
        /// Ignored position and will not be considered for usage in the pipeline
        /// </summary>
        None = 0,

        /// <summary>
        /// Raw device units, after applying configured user settings, but before applying the output mode transform.
        /// </summary>
        /// <remarks>You may want to assign a <see cref="TabletReference"/> property with <see cref="Attributes.TabletReferenceAttribute"/>
        ///    so that you can look up the tablet specifications and e.g. normalize position to millimeters.</remarks>
        PreTransform = 1 << 0,

        /// <summary>
        /// Transformed output device units, almost always in pixels.
        /// </summary>
        /// <remarks>Note that this pipeline position is generally undesirable if your plugin significantly inspects
        /// <see cref="Tablet.IAbsolutePositionReport"/>, as relative and absolute modes may behave significantly different.
        /// It may also be difficult to calculate the physical unit size.</remarks>
        PostTransform = 1 << 1,

        /// <summary><see cref="PreTransform"/></summary>
        Raw = PreTransform,

        /// <summary><see cref="PostTransform"/></summary>
        Pixels = PostTransform,

        /// <summary>
        /// Rawest option, prior to fixing up tablet data with tablet specifications and settings (e.g. unmangled pressure)
        /// </summary>
        /// <remarks>You may want to assign a <see cref="TabletReference"/> property with <see cref="Attributes.TabletReferenceAttribute"/>
        ///    so that you can look up the tablet specifications and e.g. normalize position to millimeters.</remarks>
        Internal = 1 << 2,
    }
}
