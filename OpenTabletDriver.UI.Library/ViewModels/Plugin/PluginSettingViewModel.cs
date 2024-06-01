using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.Daemon.Contracts.Persistence;
using OpenTabletDriver.UI.Models;

namespace OpenTabletDriver.UI.ViewModels.Plugin;

public abstract class PluginSettingViewModel : ViewModelBase
{
    protected ProfileBinding ProfileBinding { get; private set; }

    public PluginSettingMetadata Metadata { get; }

    public string FriendlyName => Metadata.FriendlyName;
    public string? Description => Metadata.ShortDescription;
    public string? ToolTip => Metadata.LongDescription;

    public event EventHandler? SettingsChanged;

    protected PluginSettingViewModel(PluginSettingMetadata metadata, ProfileBinding profileBinding)
    {
        ProfileBinding = profileBinding;
        Metadata = metadata;
    }

    public abstract void Read(Profile profile);
    public abstract void Write(Profile profile);

    protected void OnSettingChanged(object sender, EventArgs e)
    {
        SettingsChanged?.Invoke(sender, e);
    }

    public static PluginSettingViewModel? CreateBindable(PluginSettingMetadata metadata, ProfileBinding binding, Profile? profile = null)
    {
        PluginSettingViewModel? vm = metadata.Type switch
        {
            SettingType.Boolean => new BoolViewModel(metadata, binding),
            SettingType.Integer => new IntegerViewModel(metadata, binding),
            SettingType.Number => new NumberViewModel(metadata, binding),
            SettingType.String => new StringViewModel(metadata, binding),
            _ => null
        };

        if (vm is not null && profile is not null)
            vm.Read(profile);

        return vm;
    }
}
