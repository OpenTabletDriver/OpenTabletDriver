using OpenTabletDriver.UI.ViewModels;

namespace OpenTabletDriver.UI.Navigation;

public sealed class NavigationMapNotFoundViewModel : ActivatableViewModelBase
{
    private string? _route;
    private string? _viewModelType;

    public string? Route
    {
        get => _route;
        set
        {
            if (value != null && _viewModelType != null)
                throw new InvalidOperationException("Route and ViewModelType cannot be set at the same time");
            _route = value;
        }
    }

    public string? ViewModelType
    {
        get => _viewModelType;
        set
        {
            if (value != null && _route != null)
                throw new InvalidOperationException("Route and ViewModelType cannot be set at the same time");
            _viewModelType = value;
        }
    }
}
