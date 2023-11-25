using CommunityToolkit.Mvvm.ComponentModel;
using OpenTabletDriver.UI.Models;
using OpenTabletDriver.UI.Services;

namespace OpenTabletDriver.UI.ViewModels;

public abstract partial class WindowViewModelBase : ViewModelBase, IDisposable
{
    private readonly IDisposable _uiSettingsSubscription;

    [ObservableProperty]
    private bool _transparencyEnabled;

    protected WindowViewModelBase(IUISettingsProvider uiSettingsProvider)
    {
        _uiSettingsSubscription = uiSettingsProvider.WhenLoadedOrSet((d, settings) =>
        {
            settings.HandleProperty(
                nameof(UISettings.Transparency),
                s => s.Transparency,
                (s, v) => TransparencyEnabled = v
            ).DisposeWith(d);
        });
    }

    public void Dispose()
    {
        _uiSettingsSubscription.Dispose();
        GC.SuppressFinalize(this);
    }
}
