using JetBrains.Annotations;

namespace OpenTabletDriver.Output
{
    /// <summary>
    /// Marks the position in a positioned pipeline element.
    /// </summary>
    [PublicAPI]
    public enum PipelinePosition
    {
        None = 0,
        PreTransform = 1 << 0,
        PostTransform = 1 << 1,
        Raw = PreTransform,
        Pixels = PostTransform
    }
}
