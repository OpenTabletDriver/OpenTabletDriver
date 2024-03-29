using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using OpenTabletDriver.UI.Messages;
using OpenTabletDriver.UI.Navigation;
using OpenTabletDriver.UI.ViewModels;

namespace OpenTabletDriver.UI.Views;

public partial class MainWindowView : Window
{
    private readonly INavigator _navigator;
    private readonly IDispatcher _dispatcher;

    public MainWindowView()
    {
        InitializeComponent();
        DataContext = Ioc.Default.GetRequiredService<MainWindowViewModel>();
        _navigator = Ioc.Default.GetRequiredService<INavigatorFactory>().GetOrCreate(AppRoutes.MainHost);
        _dispatcher = Ioc.Default.GetRequiredService<IDispatcher>();
        var messenger = Ioc.Default.GetRequiredService<IMessenger>();

        HookInputEvents();
        this.BootstrapTransparency((MainWindowViewModel)DataContext!, _dispatcher);

        // ensure we have window-level focus on startup
        Activate();
        Focus();

        VIEW_SplitView.PaneOpened += (sender, e) =>
        {
            messenger.Send(new UILayoutChangedMessage(UILayoutChange.SidebarOpen));
        };

        VIEW_SplitView.PaneClosed += (sender, e) =>
        {
            messenger.Send(new UILayoutChangedMessage(UILayoutChange.SidebarHidden));
        };
    }

    private void HookInputEvents()
    {
        PointerPressed += (sender, e) =>
        {
            if (e.Handled)
                return;

            e.Handled = true;
            // is there a need to implement forward?
            if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == Avalonia.Input.PointerUpdateKind.XButton1Pressed
                && _navigator.CanGoBack)
            {
                _navigator.Pop();
                // TODO: don't rely on to-be-internal IPseudoClasses interface
                ((IPseudoClasses)VIEW_BackButton.Classes).Set(":pressed", true);
                _dispatcher.ScheduleOnce(() =>
                {
                    ((IPseudoClasses)VIEW_BackButton.Classes).Set(":pressed", false);
                }, 100);
            }

            FocusManager?.ClearFocus();
        };

        KeyDown += (sender, e) =>
        {
            if (e.Handled)
                return;

            // show a menu when alt is pressed for navigation,
            // mostly useful when nav sidebar is hidden
            if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
            {
                e.Handled = true;
                if (!TryGetResource("VIEW_Resource_MenuFlyout", null, out var menuFlyoutObj))
                    return;
                if (menuFlyoutObj is not MenuFlyout menuFlyout)
                    return;

                if (!menuFlyout.IsOpen)
                {
                    var target = VIEW_MenuButton.IsVisible ? VIEW_MenuButton : VIEW_BackButton;
                    menuFlyout.ShowAt(target);
                }
            }
        };
    }

    protected override void OnResized(WindowResizedEventArgs args)
    {
        var openPaneLength = VIEW_SplitView.OpenPaneLength;
        var currOpenPane = VIEW_SplitView.IsPaneOpen;
        var newOpenPane = args.ClientSize.Width > openPaneLength + MinWidth + 48;
        // var wideUI = args.ClientSize.Width >= 900;

        if (currOpenPane != newOpenPane)
        {
            VIEW_SplitView.IsPaneOpen = newOpenPane;
            VIEW_NavigationHost.Classes.Set("SidePaneHidden", !newOpenPane);
            VIEW_MenuButton.IsVisible = !newOpenPane;
        }

        base.OnResized(args);
    }
}
