namespace TabletDriverPlugin
{
    public interface IAbsoluteMode : IOutputMode
    {
        Area Input { set; get; }
        Area Output { set; get; }
        Area Screen { set; get; }
        bool AreaClipping { set; get; }
    }
}