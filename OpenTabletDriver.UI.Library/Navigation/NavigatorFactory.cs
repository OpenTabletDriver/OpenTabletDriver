namespace OpenTabletDriver.UI.Navigation;

public class NavigatorFactory : INavigatorFactory
{
    private readonly IServiceProvider _provider;
    private readonly NavigationRoute[] _staticRoutes;
    private readonly Dictionary<string, INavigator> _navigators = new();

    public NavigatorFactory(IServiceProvider provider, IEnumerable<NavigationRoute> routes)
    {
        _provider = provider;
        _staticRoutes = routes.ToArray();
    }

    public INavigator GetOrCreate(string navHostName)
    {
        if (_navigators.TryGetValue(navHostName, out var navigator))
            return navigator;

        var routes = _staticRoutes.Where(r => r.Host is null || r.Host == navHostName);
        var routeCollection = new NavigationRouteCollection(routes);
        navigator = new Navigator(_provider, routeCollection);
        _navigators.Add(navHostName, navigator);
        return navigator;
    }
}
