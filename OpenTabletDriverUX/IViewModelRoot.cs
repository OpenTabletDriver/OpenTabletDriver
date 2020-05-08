using System.ComponentModel;

namespace OpenTabletDriverUX
{
    public interface IViewModelRoot<T> where T : INotifyPropertyChanged
    {
        T ViewModel { set; get; }
    }
}