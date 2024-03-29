using System.Diagnostics;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace OpenTabletDriver.UI.Navigation;

public class Navigator : INavigator
{
    private readonly Stack<object> _navStack = new(5);
    private readonly IServiceProvider _serviceProvider;

    private CancellableNavigationEventData? _navigating;

    public object? Current => _navStack.TryPeek(out var curr) ? curr : null;
    public bool CanGoBack { get; private set; }
    public NavigationRouteCollection Routes { get; }

    public event EventHandler<CancellableNavigationEventData>? Navigating;
    public event EventHandler<NavigationEventData>? Navigated;

    public Navigator(IServiceProvider serviceProvider, NavigationRouteCollection routes)
    {
        _serviceProvider = serviceProvider;
        Routes = routes;
    }

    public void Push(object routeObject, bool asRoot = false)
    {
        ArgumentNullException.ThrowIfNull(routeObject);
        if (routeObject is not Control)
        {
            var route = Find(routeObject.GetType());
            routeObject = CreateViewOrMapNotFound(routeObject, route);
        }
        PushInternal((Control)routeObject, asRoot);
    }

    public void Push(string routeName, bool asRoot = false)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(routeName);
        var route = Find(routeName);

        PushInternal(route is not null
            ? Create(route)
            : MapNotFound(routeName, null), asRoot);
    }

    private void PushInternal(Control control, bool asRoot)
    {
        var kind = asRoot ? NavigationKind.PushAsRoot : NavigationKind.Push;
        Navigate(kind, control);
    }

    public void Pop(bool toRoot = false)
    {
        if (!toRoot && !CanGoBack)
            throw new InvalidOperationException($"Already root");
        if (toRoot && _navStack.Count == 0)
            throw new InvalidOperationException("Empty navigation stack");
        if (toRoot && _navStack.Count == 1)
            return; // already root
        var kind = toRoot ? NavigationKind.PopToRoot : NavigationKind.Pop;
        Navigate(kind, null);
    }

    private void Navigate(NavigationKind kind, object? next)
    {
        if (_navigating is not null)
        {
            if (_navigating.Cancel == NavigationCancellationKind.None)
            {
                throw new InvalidOperationException("Navigation is already in progress");
            }
            else if (_navigating.Cancel == NavigationCancellationKind.Replace)
            {
                TraceUtility.PrintTrace(this, $"Replacing navigation from {_navigating.Current} to {next}");
                // no-op
            }
            else if (_navigating.Cancel == NavigationCancellationKind.Redirect)
            {
                TraceUtility.PrintTrace(this, $"Redirecting navigation from {_navigating.Current} to {next}");
                CommitNavigation(_navigating.Kind, _navigating.Previous!, _navigating.Current);
            }
            else
            {
                throw new InvalidOperationException($"Unknown navigation cancellation kind: {_navigating.Cancel}");
            }
        }

        var prev = Current;
        var curr = kind switch
        {
            NavigationKind.Pop => _navStack.ElementAt(1), // curr will be the page before the top of the stack
            NavigationKind.PopToRoot => _navStack.Last(), // curr will be the root page
            _ => next ?? throw new ArgumentNullException(nameof(next))
        };

        var eventArg = new CancellableNavigationEventData(kind, prev, curr);
        _navigating = eventArg;
        Navigating?.Invoke(this, eventArg);
        if (eventArg.Cancel == NavigationCancellationKind.None)
        {
            CommitNavigation(kind, curr, next);
            _navigating = null; // at this point, navigation is "complete"
            CanGoBack = _navStack.Count > 1;
            Navigated?.Invoke(this, new NavigationEventData(kind, prev, curr));
        }
    }

    private void CommitNavigation(NavigationKind navigationKind, object curr, object? next)
    {
        switch (navigationKind)
        {
            case NavigationKind.Push:
                _navStack.Push(next!);
                break;
            case NavigationKind.Pop:
                _navStack.Pop();
                break;
            case NavigationKind.PushAsRoot:
                _navStack.Clear();
                _navStack.Push(next!);
                break;
            case NavigationKind.PopToRoot:
                _navStack.Clear();
                _navStack.Push(curr);
                break;
        }
    }

    private Control Create(NavigationRoute route)
    {
        var dataContext = _serviceProvider.GetRequiredService(route.ObjectType);
        return CreateViewOrMapNotFound(dataContext, route);
    }

    private Control CreateViewOrMapNotFound(object dataContext, NavigationRoute? route)
    {
        if (route is null)
            return MapNotFound(null, dataContext.GetType().Name);

        return CreateView(dataContext, route);
    }

    private static Control CreateView(object dataContext, NavigationRoute route)
    {
        var view = (Control)Activator.CreateInstance(route.ViewType)!;
        view.DataContext = dataContext;
        return view;
    }

    private Control MapNotFound(string? routeName, string? viewModelType)
    {
        Debug.WriteLine(routeName is not null
            ? $"Unknown route name '{routeName}'"
            : $"Unknown view model type '{viewModelType}'");

        var route = Find(typeof(NavigationMapNotFoundViewModel));
        if (route == null)
        {
            Debug.WriteLine("NavigationMapNotFoundViewModel not found, replacing with TextBlock");
            return new TextBlock()
            {
                Text = routeName is not null
                    ? $"Unknown route name '{routeName}'"
                    : $"Unknown view model type '{viewModelType}'"
            };
        }

        var vm = new NavigationMapNotFoundViewModel()
        {
            Route = routeName,
            ViewModelType = viewModelType
        };

        return CreateView(vm, route);
    }

    private NavigationRoute? Find(Type type)
    {
        return Routes.FirstOrDefault(r => r.ObjectType == type);
    }

    private NavigationRoute? Find(string routeName)
    {
        return Routes.FirstOrDefault(r => r.Name == routeName);
    }
}
