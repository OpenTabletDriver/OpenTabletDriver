using System.Collections.Generic;

namespace TabletDriverPlugin.Tablet
{
    public interface IBindingHandler<T>
    {
        float TipActivationPressure { set; get; }
        T TipBinding { set; get; } // TODO: Replace this with a proper binding class
        Dictionary<int, T> PenButtonBindings { set; get; }
        Dictionary<int, T> AuxButtonBindings { set; get; }

        void HandleBinding(IDeviceReport report);
    }
}