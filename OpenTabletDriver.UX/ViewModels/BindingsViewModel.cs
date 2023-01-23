namespace OpenTabletDriver.UX.ViewModels
{
    public class BindingsViewModel : BaseViewModel
    {
        private readonly TabletViewModel _tabletViewModel;

        public BindingsViewModel(TabletViewModel tabletViewModel)
        {
            _tabletViewModel = tabletViewModel;
        }
    }
}
