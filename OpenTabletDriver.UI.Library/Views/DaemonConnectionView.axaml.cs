using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Input;
using OpenTabletDriver.UI.Controls;
using OpenTabletDriver.UI.ViewModels;

namespace OpenTabletDriver.UI.Views;

public partial class DaemonConnectionView : ActivatableUserControl
{
    public DaemonConnectionView()
    {
        InitializeComponent();
        Window.SizeChangedEvent.AddClassHandler<DaemonConnectionView>(HandleClientSizeChanged);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is DaemonConnectionViewModel vm)
        {
            vm.QolHintText.CollectionChanged += HandleItemsChanged;
            vm.HandleProperty(
                nameof(DaemonConnectionViewModel.IsConnecting),
                vm => vm.IsConnecting,
                (vm, connecting) =>
                {
                    this.Cursor = connecting
                        ? new Cursor(StandardCursorType.Wait)
                        : new Cursor(StandardCursorType.Arrow);
                }
            );
        }
        base.OnDataContextChanged(e);
    }

    private void HandleItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            VIEW_Scroller.ScrollToEnd();
        }
    }

    private static void HandleClientSizeChanged(DaemonConnectionView view, SizeChangedEventArgs args)
    {
        view.VIEW_Scroller.ScrollToEnd();
    }
}
