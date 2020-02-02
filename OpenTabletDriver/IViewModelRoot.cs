namespace OpenTabletDriver
{
    public interface IViewModelRoot<T> where T : ViewModelBase
    {
        T ViewModel { set; get; }
    }
}