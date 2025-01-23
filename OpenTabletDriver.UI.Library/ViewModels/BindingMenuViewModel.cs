using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenTabletDriver.UI.Services;
using OpenTabletDriver.UI.ViewModels.Plugin;

namespace OpenTabletDriver.UI.ViewModels;

// wrapper to BindingSettingViewModel to setup transparency
public partial class BindingMenuViewModel : WindowViewModelBase
{
    [ObservableProperty]
    private BindingSettingViewModel _setting = null!;

    public BindingMenuViewModel(IUISettingsProvider uiSettingsProvider) : base(uiSettingsProvider)
    {
    }

    [RelayCommand]
    private void Clear()
    {
        Setting.SelectedBinding = null;
    }
}
