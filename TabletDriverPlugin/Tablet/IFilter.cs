namespace TabletDriverPlugin.Tablet
{
    public interface IFilter
    {
        Point Filter(Point point);
        FilterStage FilterStage { get; }
    }
}