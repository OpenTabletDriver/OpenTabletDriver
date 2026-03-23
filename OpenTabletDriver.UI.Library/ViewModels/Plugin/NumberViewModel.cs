using CommunityToolkit.Mvvm.ComponentModel;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.Daemon.Contracts.Persistence;
using OpenTabletDriver.UI.Models;

namespace OpenTabletDriver.UI.ViewModels.Plugin;

public partial class NumberViewModel : PluginSettingViewModel
{
    [ObservableProperty]
    private double _value;

    public bool Slider { get; }
    public double Min { get; }
    public double Max { get; }
    public double Step { get; }

    public NumberViewModel(PluginSettingMetadata metadata, ProfileBinding binding)
        : base(metadata, binding)
    {
        if (metadata.Attributes.ContainsKey("min"))
        {
            Slider = true;
            Min = int.Parse(metadata.Attributes["min"]);
            Max = int.Parse(metadata.Attributes["max"]);
            Step = int.Parse(metadata.Attributes["step"]);
        }
    }

    public override void Read(Profile profile)
    {
        Value = ProfileBinding.GetValue<int>(profile);
    }

    public override void Write(Profile profile)
    {
        ProfileBinding.SetValue(profile, Value);
    }

    partial void OnValueChanged(double value)
    {
        OnSettingChanged(this, EventArgs.Empty);
    }
}
