namespace OpenTabletDriver.Plugin.Tablet.Wheel.Actions
{
    public interface IWheelActionFactory
    {
        IWheelAction CreateScroll();
        IWheelAction CreateZoom();
        IWheelAction CreateBrush();
        IWheelAction CreateUndoRedo();
        IWheelAction CreateLayer();
        IWheelAction CreateCustom();
    }
}
