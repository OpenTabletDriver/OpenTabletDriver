using OpenTabletDriver.UI.Controls;
using OpenTabletDriver.UI.Navigation;

namespace OpenTabletDriver.UI.Views;

public partial class NavigationMapNotFoundView : ActivatableUserControl
{
    public NavigationMapNotFoundView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is NavigationMapNotFoundViewModel vm)
        {
            VIEW_TextBlock.Text = vm.Route is not null
                ? $"Oops! The route '{vm.Route}' was not found."
                : $"Oops! No view registered for view model of type '{vm.ViewModelType}' was found.";
        }

        base.OnDataContextChanged(e);
    }
}
