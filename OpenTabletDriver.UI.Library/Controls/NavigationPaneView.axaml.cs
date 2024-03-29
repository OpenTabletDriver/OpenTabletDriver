using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using OpenTabletDriver.UI.ViewModels;

namespace OpenTabletDriver.UI.Controls;

public partial class NavigationPaneView : UserControl
{
    public NavigationPaneView()
    {
        DataContext = Ioc.Default.GetRequiredService<NavigationPaneViewModel>();
        InitializeComponent();
        ConnectDisjointedList((NavigationPaneViewModel)DataContext);
    }

    private void ConnectDisjointedList(NavigationPaneViewModel vm)
    {
        // Since the settings button is not actually part of the navigation pane,
        // we will need to manually handle the click event.
        VIEW_SettingsButton.PointerPressed += (object? s, PointerPressedEventArgs e) =>
            vm.SettingsOpened = e.GetCurrentPoint(s as Visual).Properties.IsLeftButtonPressed;
    }
}
