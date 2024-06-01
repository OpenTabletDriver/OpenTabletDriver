using CommunityToolkit.Mvvm.ComponentModel;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.Daemon.Contracts.Persistence;
using OpenTabletDriver.UI.Models;

namespace OpenTabletDriver.UI.ViewModels.Plugin;

public partial class BoolViewModel : PluginSettingViewModel
{
    [ObservableProperty]
    private bool _value;

    public BoolViewModel(PluginSettingMetadata metadata, ProfileBinding profileBinding)
        : base(metadata, profileBinding)
    {
    }

    public override void Read(Profile profile)
    {
        Value = ProfileBinding.GetValue<bool>(profile);
    }

    public override void Write(Profile profile)
    {
        ProfileBinding.SetValue(profile, Value);
    }

    partial void OnValueChanged(bool value)
    {
        OnSettingChanged(this, EventArgs.Empty);
    }
}
