using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using OpenTabletDriver.UI.ViewModels;


namespace OpenTabletDriver.UI.Views.Dialogs;

public partial class BindingMenuDialog : Window
{
    public BindingMenuDialog()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is not BindingMenuViewModel dataContext)
            return;

        var dispatcher = Ioc.Default.GetRequiredService<IDispatcher>();
        this.BootstrapTransparency(dataContext, dispatcher);
    }
}
