using CommunityToolkit.Mvvm.DependencyInjection;
using OpenTabletDriver.UI.Controls;
using OpenTabletDriver.UI.Navigation;
using OpenTabletDriver.UI.ViewModels;

namespace OpenTabletDriver.UI.Views;

// TODO: add preset management
public partial class TabletsOverview : ActivatableUserControl
{
    private readonly INavigator _navigator;

    public TabletsOverview()
    {
        _navigator = Ioc.Default.GetRequiredService<INavigatorFactory>().GetOrCreate(AppRoutes.MainHost);
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is TabletsOverviewViewModel vm)
        {
            vm.TabletDisconnected += PopToOverview;
        }
        base.OnDataContextChanged(e);
    }

    private void PopToOverview(TabletViewModel viewModel)
    {
        if (_navigator.Current?.GetType() == typeof(TabletView))
        {
            // navigate back to overview
            _navigator.Pop(toRoot: true);

            // TODO: notify user of tablet disconnection/error
        }
    }
}
