using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenTabletDriver.UI.Models;
using OpenTabletDriver.UI.Navigation;
using OpenTabletDriver.UI.Services;
using OpenTabletDriver.UI.Views;

namespace OpenTabletDriver.UI.ViewModels;

public partial class TabletsOverviewViewModel : ActivatableViewModelBase
{
    private readonly IDaemonService _daemonService;
    private readonly INavigator _navigator;

    [ObservableProperty]
    private TabletViewModel? _selectedTablet;

    public ObservableCollection<TabletViewModel> Tablets { get; } = new();

    public event Action<TabletViewModel>? TabletDisconnected;

    public TabletsOverviewViewModel(IDaemonService daemonService, INavigatorFactory navigatorFactory)
    {
        _navigator = navigatorFactory.GetOrCreate(AppRoutes.MainHost);

        // TODO: move this to TabletsOverview to avoid referencing a View from a ViewModel
        _navigator.Navigating += (_, ev) =>
        {
            if (ev.Current?.GetType() != typeof(TabletsOverview))
                return;

            // Redirect to last selected tablet if we're navigating back to the overview
            // from somewhere else
            if ((ev.Kind == NavigationKind.Push || ev.Kind == NavigationKind.PushAsRoot) && SelectedTablet != null)
            {
                // TODO: improve cancel API
                ev.Cancel = NavigationCancellationKind.Redirect;
                _navigator.Push(SelectedTablet);
            }
            // Clear selection if we're navigating back to the overview from the tablet view
            else if (ev.Kind == NavigationKind.Pop)
            {
                SelectedTablet = null;
            }
        };

        _daemonService = daemonService;
        _daemonService.Tablets.CollectionChanged += (sender, e) =>
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    e.NewItems!.Cast<ITabletService>().ForEach(AddTablet);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    e.OldItems!.Cast<ITabletService>().ForEach(RemoveTablet);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Tablets.Clear();
                    _daemonService.Tablets.ForEach(AddTablet);
                    break;
            }
        };

        _daemonService.Tablets.ForEach(AddTablet);
    }

    private void AddTablet(ITabletService tablet)
    {
        var tabletViewModel = new TabletViewModel(tablet);
        Tablets.Add(tabletViewModel);
    }

    private void RemoveTablet(ITabletService tablet)
    {
        var tabletViewModel = Tablets.First(t => t.TabletId == tablet.TabletId);
        if (SelectedTablet == tabletViewModel)
            TabletDisconnected?.Invoke(tabletViewModel);

        // this should happen after invoking TabletDisconnected, Avalonia will
        // set SelectedTablet to null automatically if the removed item is selected
        Tablets.Remove(tabletViewModel);
    }

    partial void OnSelectedTabletChanged(TabletViewModel? value)
    {
        if (value is not null)
        {
            _navigator.Push(value);
        }
    }
}
