using JetBrains.Annotations;

namespace OpenTabletDriver.Plugin.Output
{
    [PublicAPI]
    public enum PipelinePosition
    {
        None = 0,
        PreTransform = 1 << 0,
        PostTransform = 1 << 1,
        Raw = PreTransform,
        Pixels = PostTransform,
        Internal = 1 << 2,
    }
}
