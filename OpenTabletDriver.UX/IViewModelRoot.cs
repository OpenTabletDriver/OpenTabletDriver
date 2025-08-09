using System.ComponentModel;

namespace OpenTabletDriver.UX
{
    public interface IViewModelRoot<T> where T : INotifyPropertyChanged
    {
        T ViewModel { set; get; }
    }
}
