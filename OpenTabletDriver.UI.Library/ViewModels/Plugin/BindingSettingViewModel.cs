using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.Daemon.Contracts.Persistence;
using OpenTabletDriver.UI.Models;

namespace OpenTabletDriver.UI.ViewModels.Plugin;

public partial class BindingSettingViewModel : ViewModelBase
{
    private readonly Func<Profile, PluginSettings?> _bindableGetter;
    private readonly Action<Profile, PluginSettings?> _bindableSetter;

    [ObservableProperty]
    private PluginDto? _selectedBinding;

    [ObservableProperty]
    private List<PluginDto> _usableBindings;

    [ObservableProperty]
    private string? _bindingDescription;

    public string BindableName { get; }
    public string Description { get; }
    public ObservableCollection<PluginSettingViewModel> PropertySettings { get; } = new();

    public event EventHandler? SettingsChanged;

    public BindingSettingViewModel(
        Profile profile,
        string bindableName,
        string description,
        List<PluginDto> usableBindings,
        Func<Profile, PluginSettings?> bindableGetter,
        Action<Profile, PluginSettings?> bindableSetter)
    {
        _bindableSetter = bindableSetter;
        _bindableGetter = bindableGetter;

        _usableBindings = usableBindings;
        BindableName = bindableName;
        Description = description + " Right click for more options. Backspace to clear.";

        Read(profile);
    }

    private PluginSettings GetPluginSettingsOrDefault(Profile profile)
    {
        var currentPluginSettings = _bindableGetter(profile);
        if (currentPluginSettings is not null)
            return currentPluginSettings;

        Debug.Assert(SelectedBinding is not null);
        currentPluginSettings = new PluginSettings()
        {
            Path = SelectedBinding.Path,
            Enable = true
        };
        _bindableSetter(profile, currentPluginSettings);

        return currentPluginSettings;
    }

    public void Read(Profile profile)
    {
        var currentPluginSettings = _bindableGetter(profile);
        SelectedBinding = currentPluginSettings is not null
            ? _usableBindings.FirstOrDefault(b => b.Path == currentPluginSettings.Path)
            : null;
        PropertySettings.ForEach(p => p.Read(profile));
        UpdateBindingDescription();
    }

    public void Write(Profile profile)
    {
        if (SelectedBinding is null)
        {
            _bindableSetter(profile, null);
            return;
        }

        var currentPluginSettings = GetPluginSettingsOrDefault(profile);
        currentPluginSettings.Path = SelectedBinding.Path;

        PropertySettings.ForEach(p => p.Write(profile));
    }

    partial void OnSelectedBindingChanged(PluginDto? value)
    {
        PropertySettings.Clear();

        if (value is not null)
        {
            var propertySettings = value.SettingsMetadata
                .Select(x => PluginSettingViewModel.CreateBindable(x, new ProfileBinding(
                    p => GetPluginSettingsOrDefault(p)[x.PropertyName],
                    (p, v) => GetPluginSettingsOrDefault(p)[x.PropertyName] = v))!)
                .Where(x => x is not null)
                .ToList();

            propertySettings.ForEach(p => p.SettingsChanged += OnSettingsChanged);
            PropertySettings.AddRange(propertySettings);
        }

        OnSettingsChanged(this, EventArgs.Empty);
    }

    partial void OnUsableBindingsChanged(List<PluginDto> value)
    {
        var selectedBinding = SelectedBinding;

        if (selectedBinding is not null)
        {
            SelectedBinding = null; // force update
            SelectedBinding = value.FirstOrDefault(x => x.Path == selectedBinding.Path);
        }
    }

    public void SetToCapturedMouseKey(string keys)
    {
        var mouseBinding = UsableBindings.FirstOrDefault(b => b.IsMouseBinding())
            ?? throw new InvalidOperationException("Default mouse binding not found.");

        SelectedBinding = mouseBinding;

        var mouseKey = PropertySettings.FirstOrDefault(s => s.FriendlyName == TypeConstants.MouseBindingMainProperty)
            ?? throw new InvalidOperationException("Default mouse binding main property not found.");

        ((StringViewModel)mouseKey).Value = null;
        ((StringViewModel)mouseKey).Value = keys;
    }

    public void SetToCapturedKey(string keys)
    {
        if (keys.Contains('+'))
        {
            var keyboardBinding = UsableBindings.FirstOrDefault(b => b.IsMultiKeyBinding())
                ?? throw new InvalidOperationException("Default keyboard binding not found.");

            SelectedBinding = keyboardBinding;

            var keyboardKey = PropertySettings.FirstOrDefault(s => s.FriendlyName == TypeConstants.MultiKeyBindingMainProperty)
                ?? throw new InvalidOperationException("Default keyboard binding main property not found.");

            ((StringViewModel)keyboardKey).Value = null; // force update
            ((StringViewModel)keyboardKey).Value = keys;
        }
        else
        {
            var keyboardBinding = UsableBindings.FirstOrDefault(b => b.IsKeyBinding())
                ?? throw new InvalidOperationException("Default keyboard binding not found.");

            SelectedBinding = keyboardBinding;

            var keyboardKey = PropertySettings.FirstOrDefault(s => s.FriendlyName == TypeConstants.KeyBindingMainProperty)
                ?? throw new InvalidOperationException("Default keyboard binding main property not found.");

            ((StringViewModel)keyboardKey).Value = null; // force update
            ((StringViewModel)keyboardKey).Value = keys;
        }
    }

    private void OnSettingsChanged(object? sender, EventArgs args)
    {
        UpdateBindingDescription();
        SettingsChanged?.Invoke(sender, args);
    }

    private void UpdateBindingDescription()
    {
        var pluginDto = SelectedBinding;
        var settings = PropertySettings;

        if (pluginDto is null)
        {
            BindingDescription = "None";
            return;
        }

        var sb = new StringBuilder(pluginDto.Name.Length);
        sb.Append(pluginDto.Name);

        var mainProperty = settings
            .Where(s => s.FriendlyName.Contains("Key") || s.FriendlyName.Contains("Button"))
            .FirstOrDefault();
        mainProperty ??= settings.FirstOrDefault();

        if (mainProperty is not null)
        {
            sb.Append(": ");
            sb.Append(((StringViewModel)mainProperty).Value ?? "None");
        }

        BindingDescription = sb.ToString();
    }
}
