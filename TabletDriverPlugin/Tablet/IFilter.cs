using System.Numerics;

namespace TabletDriverPlugin.Tablet
{
    public interface IFilter
    {
        Vector2 Filter(Vector2 point);
        FilterStage FilterStage { get; }
    }
}