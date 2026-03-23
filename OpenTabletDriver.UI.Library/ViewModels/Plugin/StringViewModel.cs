using CommunityToolkit.Mvvm.ComponentModel;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.Daemon.Contracts.Persistence;
using OpenTabletDriver.UI.Models;

namespace OpenTabletDriver.UI.ViewModels.Plugin;

public partial class StringViewModel : PluginSettingViewModel
{
    [ObservableProperty]
    private string? _value;

    public string[]? Choices { get; }

    public StringViewModel(PluginSettingMetadata metadata, ProfileBinding binding)
        : base(metadata, binding)
    {
        if (metadata.Attributes.TryGetValue("choices", out var choices))
        {
            Choices = choices.Split(';');
        }
    }

    public override void Read(Profile profile)
    {
        Value = ProfileBinding.GetValue<string>(profile);
    }

    public override void Write(Profile profile)
    {
        ProfileBinding.SetValue(profile, Value);
    }

    partial void OnValueChanged(string? value)
    {
        OnSettingChanged(this, EventArgs.Empty);
    }
}
