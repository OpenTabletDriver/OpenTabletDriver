namespace OpenTabletDriver.UI.Navigation;

public interface INavigator
{
    object? Current { get; }
    bool CanGoBack { get; }
    NavigationRouteCollection Routes { get; }
    event EventHandler<CancellableNavigationEventData> Navigating;
    event EventHandler<NavigationEventData> Navigated;
    void Push(object routeObject, bool asRoot = false);
    void Push(string routeName, bool asRoot = false);
    void Pop(bool toRoot = false);
}
