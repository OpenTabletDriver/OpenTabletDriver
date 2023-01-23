namespace OpenTabletDriver.UX.ViewModels
{
    public class FiltersViewModel : BaseViewModel
    {
        private readonly TabletViewModel _tabletViewModel;

        public FiltersViewModel(TabletViewModel tabletViewModel)
        {
            _tabletViewModel = tabletViewModel;
        }
    }
}
