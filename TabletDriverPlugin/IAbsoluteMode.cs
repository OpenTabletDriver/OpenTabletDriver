namespace TabletDriverPlugin
{
    public interface IAbsoluteMode : IOutputMode
    {
        Area Input { set; get; }
        Area Output { set; get; }
        bool AreaClipping { set; get; }
    }
}