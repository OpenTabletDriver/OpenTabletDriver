using System.Collections.ObjectModel;

namespace OpenTabletDriver.UI.Navigation;

public class NavigationRouteCollection : Collection<NavigationRoute>
{
    public NavigationRouteCollection()
    {
    }

    public NavigationRouteCollection(IEnumerable<NavigationRoute> routes) : base(routes.ToArray())
    {
    }
}
