namespace OpenTabletDriver.UI.Navigation;

public class NavigationEventData : EventArgs
{
    public NavigationKind Kind { get; }
    public object? Previous { get; }
    public object? Current { get; }

    public NavigationEventData(NavigationKind kind, object? prev, object? curr)
    {
        Kind = kind;
        Previous = prev;
        Current = curr;
    }
}

public class CancellableNavigationEventData : NavigationEventData
{
    public NavigationCancellationKind Cancel { get; set; }

    public CancellableNavigationEventData(NavigationKind kind, object? prev, object? curr) : base(kind, prev, curr)
    {
    }
}

public enum NavigationCancellationKind
{
    None,
    Replace,
    Redirect
}
