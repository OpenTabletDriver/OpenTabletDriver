using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.Daemon.Contracts.Persistence;
using OpenTabletDriver.UI.Models;
using OpenTabletDriver.UI.Services;
using OpenTabletDriver.UI.ViewModels.Plugin;

namespace OpenTabletDriver.UI.ViewModels;

public partial class TabletViewModel : ActivatableViewModelBase
{
    private readonly IDaemonService _daemonService;
    private readonly ITabletService _tabletService;
    private readonly IDispatcher _dispatcher;
    private readonly List<PluginDto> _bindings = new();
    private bool _modified;
    private bool _saved = true;
    private DateTime _lastApply;
    private const int ApplyThresholdMs = 120;

    [ObservableProperty]
    private bool _isInitialized;

    // TODO: remove
    [ObservableProperty]
    private Profile _profile;

    [ObservableProperty]
    private PluginDto? _selectedOutputMode;

    [ObservableProperty]
    private bool _isAbsoluteMode;

    [ObservableProperty]
    private bool _isRelativeMode;

    // Absolute Mode Settings
    [ObservableProperty]
    private AreaDisplayViewModel _displayArea = null!;

    [ObservableProperty]
    private TabletAreaDisplayViewModel _tabletArea = null!;

    // Relative Mode Settings
    [ObservableProperty]
    private double _sensitivityX;

    [ObservableProperty]
    private double _sensitivityY;

    [ObservableProperty]
    private double _relativeModeRotation;

    [ObservableProperty]
    private double _resetDelay;

    // Binding Settings

    [ObservableProperty]
    private double _penTipPressureThreshold;

    [ObservableProperty]
    private BindingSettingViewModel _penTipBinding = null!;

    [ObservableProperty]
    private double _penEraserPressureThreshold;

    [ObservableProperty]
    private BindingSettingViewModel _penEraserTipBinding = null!;

    public int TabletId => _tabletService.TabletId;
    public string Name => _tabletService.Name;
    public ObservableCollection<PluginDto> OutputModes { get; } = new();
    public ObservableCollection<PluginSettingViewModel> OutputModeSettings { get; } = new();
    public ObservableCollection<BindingSettingViewModel> PenButtonBindings { get; } = new();
    public ObservableCollection<BindingSettingViewModel> TabletButtonBindings { get; } = new();

    public bool Modified
    {
        get => _modified;
        set
        {
            SetProperty(ref _modified, value);
            ApplyCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
            DiscardCommand.NotifyCanExecuteChanged();
        }
    }

    public bool Saved
    {
        get => _saved;
        set
        {
            SetProperty(ref _saved, value);
            SaveCommand.NotifyCanExecuteChanged();
        }
    }

    public bool CanSave => !Saved;

    public TabletViewModel(ITabletService tabletService)
    {
        _daemonService = Ioc.Default.GetRequiredService<IDaemonService>();
        _dispatcher = Ioc.Default.GetRequiredService<IDispatcher>();
        _tabletService = tabletService;
        _profile = _tabletService.Profile;

        // propagate daemon-broadcasted profile changes to the UI
        // TODO: there's currently no fast way to check if this profile is the one
        // that UI just sent to daemon, so disable this for now to save CPU cycles

        _tabletService.HandleProperty(
            nameof(_tabletService.Profile),
            s => s.Profile,
            (s, p) => Profile = p,
            invokeOnCreation: false);

        // propagate plugin changes as well
        _daemonService.PluginContexts.CollectionChanged += (s, e) =>
        {
            // no need to optimize this since it's rarely called
            var lastSelectedOutputMode = SelectedOutputMode;

            SetupOutputModePlugins();
            SetupBindings(Profile);
            // SetupFilters();

            if (lastSelectedOutputMode is not null)
            {
                var selectedOutputMode = OutputModes.FirstOrDefault(m => m.Path == lastSelectedOutputMode?.Path);
                SelectedOutputMode = selectedOutputMode ?? OutputModes.FirstOrDefault();
            }
        };

        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        SetupOutputModePlugins();
        SetupBindings(Profile);
        await InitializeProfileAsync(Profile);
        IsInitialized = true;
    }

    [RelayCommand(CanExecute = nameof(Modified))]
    private async Task Apply()
    {
        await WriteProfileAsync(Profile);
        Modified = false;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        if (Modified)
            await WriteProfileAsync(Profile);

        await _daemonService.Instance!.SaveSettings();
        Modified = false;
        Saved = true;
    }

    [RelayCommand(CanExecute = nameof(Modified))]
    private async Task Discard()
    {
        await InitializeProfileAsync(Profile);
        Modified = false;
        Saved = false;
    }

    [RelayCommand]
    private async Task Reset()
    {
        await _tabletService.ResetProfile();
    }

    private async Task InitializeProfileAsync(Profile profile)
    {
        if (DisplayArea != null)
        {
            DisplayArea.PropertyChanged -= HandleSettingsChanged;
            DisplayArea.Mapping.PropertyChanged -= HandleSettingsChanged;
        }

        if (TabletArea != null)
        {
            TabletArea.PropertyChanged -= HandleSettingsChanged;
            TabletArea.Mapping.PropertyChanged -= HandleSettingsChanged;
        }

        var daemon = _daemonService.Instance!;
        var displayDtos = await daemon.GetDisplays();
        var displayBounds = displayDtos
            .OrderBy(d => d.Index)
            .Select(d => Bounds.FromDto(d));
        PluginSettings? outputMode = profile.OutputMode;

        var tabletWidth = _tabletService.Configuration.Specifications.Digitizer!.Width;
        var tabletHeight = _tabletService.Configuration.Specifications.Digitizer!.Height;
        var tabletBounds = new Bounds(0, 0, tabletWidth, tabletHeight, 0);
        var tabletMapping = new Mapping(0, 0, tabletWidth, tabletHeight, 0, centerOrigin: true);

        var tabletAreaSetting = outputMode["Input"].Value!;

        DisplayArea = new AreaDisplayViewModel(displayBounds, null, "Full Virtual Desktop");
        TabletArea = new TabletAreaDisplayViewModel(DisplayArea, new Bounds[] { tabletBounds }, tabletMapping, "Full Area");

        SetOutputModeDefaultsIfNeeded(profile, TabletArea, DisplayArea);

        // propagate profile data to DisplayArea and TabletArea

        var displayAreaSetting = outputMode["Output"].Value!;
        var displayAreaOffsetX = Math.Abs(Math.Min(DisplayArea.MaximumBounds.X, 0));
        var displayAreaOffsetY = Math.Abs(Math.Min(DisplayArea.MaximumBounds.Y, 0));
        DisplayArea.Mapping.Width = (double)displayAreaSetting["Width"]!;
        DisplayArea.Mapping.Height = (double)displayAreaSetting["Height"]!;
        DisplayArea.Mapping.X = -displayAreaOffsetX + (double)displayAreaSetting["XPosition"]! - DisplayArea.Mapping.Width / 2.0;
        DisplayArea.Mapping.Y = -displayAreaOffsetY + (double)displayAreaSetting["YPosition"]! - DisplayArea.Mapping.Height / 2.0;

        DisplayArea.PropertyChanged += HandleSettingsChanged;
        DisplayArea.Mapping.PropertyChanged += HandleSettingsChanged;

        TabletArea.Mapping.Width = (double)tabletAreaSetting["Width"]!;
        TabletArea.Mapping.Height = (double)tabletAreaSetting["Height"]!;
        TabletArea.Mapping.X = (double)tabletAreaSetting["XPosition"]!;
        TabletArea.Mapping.Y = (double)tabletAreaSetting["YPosition"]!;
        TabletArea.Mapping.Rotation = (double)tabletAreaSetting["Rotation"]!;

        TabletArea.LockAspectRatio = (bool)outputMode["LockAspectRatio"].Value!;
        TabletArea.ClipInput = (bool)outputMode["AreaClipping"].Value!;
        TabletArea.DropInput = (bool)outputMode["AreaLimiting"].Value!;
        TabletArea.RestrictToMaximumBounds = (bool)outputMode["LockToBounds"].Value!;

        TabletArea.PropertyChanged += HandleSettingsChanged;
        TabletArea.Mapping.PropertyChanged += HandleSettingsChanged;

        var sensitivity = outputMode["Sensitivity"].Value!;
        SensitivityX = (double)sensitivity["X"]!;
        SensitivityY = (double)sensitivity["Y"]!;
        RelativeModeRotation = (double)outputMode["Rotation"].Value!;
        ResetDelay = ((TimeSpan)outputMode["ResetDelay"].Value!).TotalMilliseconds;

        PenTipPressureThreshold = profile.Bindings.TipActivationThreshold;
        PenEraserPressureThreshold = profile.Bindings.EraserActivationThreshold;

        var pluginDto = _daemonService.FindPlugin(outputMode.Path); // should never be null, has to be handled daemon-side
        Debug.Assert(pluginDto is not null);

        SelectedOutputMode = pluginDto;
    }

    // should use right after creating the area display view models
    private static void SetOutputModeDefaultsIfNeeded(Profile profile, AreaDisplayViewModel tabletArea, AreaDisplayViewModel displayArea)
    {
        // if AbsoluteMode settings are not set, set them to defaults
        var outputMode = profile.OutputMode;
        if (!outputMode["Input"].Value?.HasValues ?? true)
        {
            var input = outputMode["Input"];

            input.SetValue(new {
                XPosition = tabletArea.Mapping.X,
                YPosition = tabletArea.Mapping.Y,
                Width = (double)tabletArea.Mapping.Width,
                Height = (double)tabletArea.Mapping.Height,
                Rotation = (double)tabletArea.Mapping.Rotation,
            });

            var output = outputMode["Output"];

            var xOffset = Math.Abs(Math.Min(displayArea.MaximumBounds.X, 0));
            var yOffset = Math.Abs(Math.Min(displayArea.MaximumBounds.Y, 0));

            var x = xOffset + displayArea.Mapping.X + displayArea.Mapping.Width / 2.0;
            var y = yOffset + displayArea.Mapping.Y + displayArea.Mapping.Height / 2.0;

            output.SetValue(new
            {
                XPosition = x,
                YPosition = y,
                Width = (double)displayArea.Mapping.Width,
                Height = (double)displayArea.Mapping.Height,
            });

            var lockAspectRatio = outputMode["LockAspectRatio"];
            lockAspectRatio.SetValue(false);

            var areaClipping = outputMode["AreaClipping"];
            areaClipping.SetValue(true);

            var areaLimiting = outputMode["AreaLimiting"];
            areaLimiting.SetValue(false);

            var lockToBounds = outputMode["LockToBounds"];
            lockToBounds.SetValue(true);
        }
        // if RelativeMode settings are not set, set them to defaults
        if (!outputMode["Sensitivity"].Value?.HasValues ?? true)
        {
            var sensitivity = outputMode["Sensitivity"];
            sensitivity.SetValue(new
            {
                X = 10,
                Y = 10,
            });

            var rotation = outputMode["Rotation"];
            rotation.SetValue(0);

            var resetDelay = outputMode["ResetDelay"];
            resetDelay.SetValue(TimeSpan.FromMilliseconds(10));
        }
    }

    private void SetupOutputModePlugins()
    {
        OutputModes.Clear();

        var outputModes = _daemonService.PluginContexts
            .SelectMany(pCtx => pCtx.Plugins)
            .Where(p => p.IsOutputMode());

        OutputModes.AddRange(outputModes);
    }

    private void SetupBindings(Profile profile)
    {
        _bindings.Clear();

        var bindings = _daemonService.PluginContexts
            .SelectMany(pCtx => pCtx.Plugins)
            .Where(p => p.IsBinding());

        _bindings.AddRange(bindings);

        PenTipBinding = new BindingSettingViewModel(
            profile,
            "Pen Tip",
            "The action performed when the pen tip is pressed.",
            _bindings,
            p => p.Bindings.TipButton,
            (p, v) => p.Bindings.TipButton = v);
        PenEraserTipBinding = new BindingSettingViewModel(
            profile,
            "Eraser Tip",
            "The action performed when the eraser tip is pressed.",
            _bindings,
            p => p.Bindings.EraserButton,
            (p, v) => p.Bindings.EraserButton = v);

        PenTipBinding.SettingsChanged += HandleSettingsChanged;
        PenEraserTipBinding.SettingsChanged += HandleSettingsChanged;

        PenButtonBindings.Clear();
        var penButtonCount = _tabletService.Configuration.Specifications.Pen?.ButtonCount ?? 0;
        for (int i = 0; i < penButtonCount; i++)
        {
            int ii = i; // prevent closure capture bug
            var desc = $"The action performed when pen button {ii + 1} is pressed.";
            var binding = new BindingSettingViewModel(
                profile,
                $"Pen Button {ii + 1}",
                desc,
                _bindings,
                p => p.Bindings.PenButtons.Count > ii ? p.Bindings.PenButtons[ii] : null,
                (p, v) =>
                {
                    while (p.Bindings.PenButtons.Count <= ii)
                        p.Bindings.PenButtons.Add(null);
                    p.Bindings.PenButtons[ii] = v!;
                }
            );

            binding.SettingsChanged += HandleSettingsChanged;
            PenButtonBindings.Add(binding);
        }

        TabletButtonBindings.Clear();
        var auxButtonCount = _tabletService.Configuration.Specifications.AuxiliaryButtons?.ButtonCount ?? 0;
        for (int i = 0; i < auxButtonCount; i++)
        {
            int ii = i; // prevent closure capture bug
            var desc = $"The action performed when tablet button {ii + 1} is pressed.";
            var binding = new BindingSettingViewModel(
                profile,
                $"Tablet Button {ii + 1}",
                desc,
                _bindings,
                p => p.Bindings.AuxButtons.Count > ii ? p.Bindings.AuxButtons[ii] : null,
                (p, v) =>
                {
                    while (p.Bindings.AuxButtons.Count <= ii)
                        p.Bindings.AuxButtons.Add(null);
                    p.Bindings.AuxButtons[ii] = v!;
                }
            );

            binding.SettingsChanged += HandleSettingsChanged;
            TabletButtonBindings.Add(binding);
        }
    }

    private async Task WriteProfileAsync(Profile profile)
    {
        // propagate DisplayArea and TabletArea changes to the profile
        var outputMode = profile.OutputMode;
        outputMode.Path = SelectedOutputMode!.Path;

        var input = outputMode["Input"];

        input.SetValue(new {
            XPosition = TabletArea.Mapping.X,
            YPosition = TabletArea.Mapping.Y,
            Width = (double)TabletArea.Mapping.Width,
            Height = (double)TabletArea.Mapping.Height,
            Rotation = (double)TabletArea.Mapping.Rotation,
        });

        var output = outputMode["Output"];

        var xOffset = Math.Abs(Math.Min(DisplayArea.MaximumBounds.X, 0));
        var yOffset = Math.Abs(Math.Min(DisplayArea.MaximumBounds.Y, 0));

        var x = xOffset + DisplayArea.Mapping.X + DisplayArea.Mapping.Width / 2.0;
        var y = yOffset + DisplayArea.Mapping.Y + DisplayArea.Mapping.Height / 2.0;

        output.SetValue(new
        {
            XPosition = x,
            YPosition = y,
            Width = (double)DisplayArea.Mapping.Width,
            Height = (double)DisplayArea.Mapping.Height,
        });

        var lockAspectRatio = outputMode["LockAspectRatio"];
        lockAspectRatio.SetValue(TabletArea.LockAspectRatio);

        var areaClipping = outputMode["AreaClipping"];
        areaClipping.SetValue(TabletArea.ClipInput);

        var areaLimiting = outputMode["AreaLimiting"];
        areaLimiting.SetValue(TabletArea.DropInput);

        var lockToBounds = outputMode["LockToBounds"];
        lockToBounds.SetValue(TabletArea.RestrictToMaximumBounds);

        var sensitivity = outputMode["Sensitivity"];
        sensitivity.SetValue(new
        {
            X = SensitivityX,
            Y = SensitivityY,
        });

        var rotation = outputMode["Rotation"];
        rotation.SetValue(RelativeModeRotation);

        var resetDelay = outputMode["ResetDelay"];
        resetDelay.SetValue(TimeSpan.FromMilliseconds(ResetDelay));

        profile.Bindings.TipActivationThreshold = (float)Math.Round(PenTipPressureThreshold, 2);
        profile.Bindings.EraserActivationThreshold = (float)Math.Round(PenEraserPressureThreshold, 2);

        OutputModeSettings.ForEach(o => o.Write(profile));
        PenButtonBindings.ForEach(b => b.Write(profile));
        TabletButtonBindings.ForEach(b => b.Write(profile));
        PenTipBinding.Write(profile);
        PenEraserTipBinding.Write(profile);

        // record the time of apply to ignore profile updates most likely caused
        // by us for a short period of time
        _lastApply = DateTime.Now;

        // send the updated profile to the daemon
        await _tabletService.ApplyProfile();
    }

    partial void OnProfileChanged(Profile value)
    {
        // if profile changes but the last apply was recent, ignore the change.
        // this is most likely caused by us, so we don't want to feed the
        // same profile settings again to UI viewmodels.
        var timeNow = DateTime.Now;
        var applyDiff = timeNow - _lastApply;
        if (applyDiff.TotalMilliseconds < ApplyThresholdMs)
        {
            Debug.WriteLine($"Skipping profile update, last apply was {applyDiff.TotalMilliseconds}ms ago and is likely us.");
            return;
        }

        Debug.WriteLine("Updating profile");
        _dispatcher.ProbablySynchronousPost(() => InitializeAsync());
    }

    partial void OnSelectedOutputModeChanged(PluginDto? value)
    {
        OutputModeSettings.Clear();

        if (value is null)
            return;

        IsAbsoluteMode = value.IsAbsoluteMode();
        IsRelativeMode = value.IsRelativeMode();

        var profile = Profile;
        var settings = value.GetCustomOutputModeSettings()
            .Select(s => PluginSettingViewModel.CreateBindable(
                s,
                new ProfileBinding(
                    p => p.OutputMode[s.PropertyName],
                    (p, v) => p.OutputMode[s.PropertyName] = v),
                profile)!)
            .Where(s => s is not null);

        settings.ForEach(s => s.PropertyChanged += HandleSettingsChanged);
        OutputModeSettings.AddRange(settings);
    }
    partial void OnSensitivityXChanged(double value) => HandleSettingsChanged(null, null!);
    partial void OnSensitivityYChanged(double value) => HandleSettingsChanged(null, null!);
    partial void OnResetDelayChanged(double value) => HandleSettingsChanged(null, null!);
    partial void OnRelativeModeRotationChanged(double value) => HandleSettingsChanged(null, null!);

    private void HandleSettingsChanged(object? sender, EventArgs e)
    {
        Modified = true;
        Saved = false;
    }
}
