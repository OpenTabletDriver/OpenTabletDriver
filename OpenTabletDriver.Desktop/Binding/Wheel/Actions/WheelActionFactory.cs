using OpenTabletDriver.Plugin.Tablet.Wheel.Actions;

namespace OpenTabletDriver.Desktop.Binding.Wheel.Actions
{
    public class WheelActionFactory : IWheelActionFactory
    {
        public IWheelAction CreateScroll() => new ScrollAction();
        public IWheelAction CreateZoom() => new ZoomAction();
        public IWheelAction CreateBrush() => new BrushAction();
        public IWheelAction CreateUndoRedo() => new UndoRedoAction();
        public IWheelAction CreateLayer() => new LayerAction();
        public IWheelAction CreateCustom() => new CustomAction();
    }
}
