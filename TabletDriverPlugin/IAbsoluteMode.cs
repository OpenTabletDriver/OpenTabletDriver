using TabletDriverPlugin.Platform.Display;

namespace TabletDriverPlugin
{
    public interface IAbsoluteMode : IOutputMode
    {
        Area Input { set; get; }
        Area Output { set; get; }
        IDisplay SelectedDisplay { set; get; }
        bool AreaClipping { set; get; }
    }
}