namespace TabletDriverPlugin.Tablet
{
    public interface IFilter
    {
        Point Filter(Point point);
        FilterTime FilterTime { get; }
    }
}