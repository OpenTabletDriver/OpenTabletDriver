using Avalonia.Controls;
using Avalonia.Layout;
using OpenTabletDriver.UI.Controls;
using OpenTabletDriver.UI.ViewModels;

namespace OpenTabletDriver.UI.Views;

public partial class TabletView : ActivatableUserControl
{
    private TabletViewModel? _viewModel;

    public TabletView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (_viewModel is not null)
            _viewModel.DisplayLayoutRefreshConfirmationRequested -= ConfirmDisplayLayoutRefresh;

        _viewModel = DataContext as TabletViewModel;
        if (_viewModel is not null)
            _viewModel.DisplayLayoutRefreshConfirmationRequested += ConfirmDisplayLayoutRefresh;

        base.OnDataContextChanged(e);
    }

    private async Task<bool> ConfirmDisplayLayoutRefresh()
    {
        var owner = TopLevel.GetTopLevel(this) as Window;
        if (owner is null)
            return false;

        var dialog = new Window
        {
            Title = "Refresh displays?",
            Width = 420,
            Height = 190,
            MinWidth = 360,
            MinHeight = 170,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Content = CreateDisplayLayoutRefreshDialogContent()
        };

        return await dialog.ShowDialog<bool>(owner);
    }

    private static Control CreateDisplayLayoutRefreshDialogContent()
    {
        var message = new TextBlock
        {
            Text = "The display layout changed, but this tablet has unsaved settings. Apply the current changes and refresh the display layout?",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        };

        var refreshButton = new Button
        {
            Content = "Apply and refresh",
            HorizontalAlignment = HorizontalAlignment.Right,
            IsDefault = true
        };

        var cancelButton = new Button
        {
            Content = "Not now",
            HorizontalAlignment = HorizontalAlignment.Right,
            IsCancel = true
        };

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8,
            Children =
            {
                cancelButton,
                refreshButton
            }
        };

        var panel = new StackPanel
        {
            Margin = new Avalonia.Thickness(18),
            Spacing = 18,
            Children =
            {
                message,
                buttons
            }
        };

        refreshButton.Click += (_, _) => (TopLevel.GetTopLevel(refreshButton) as Window)?.Close(true);
        cancelButton.Click += (_, _) => (TopLevel.GetTopLevel(cancelButton) as Window)?.Close(false);

        return panel;
    }
}
