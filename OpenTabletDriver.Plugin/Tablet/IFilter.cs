using System.Numerics;

namespace OpenTabletDriver.Plugin.Tablet
{
    public interface IFilter
    {
        Vector2 Filter(Vector2 point);
        void FinalizeConfiguration();
        FilterStage FilterStage { get; }
    }
}