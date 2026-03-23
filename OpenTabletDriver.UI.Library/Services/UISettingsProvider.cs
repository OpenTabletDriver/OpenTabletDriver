using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenTabletDriver.UI.Models;

namespace OpenTabletDriver.UI.Services;

public interface IUISettingsProvider : INotifyPropertyChanged
{
    UISettings? Settings { get; set; }
    // TODO: remove this and instead make this service interact with a notification service.
    // TODO: do the recovery in the service itself instead of the view model.
    UISettingsLoadException? LoadException { get; }
    Task SaveSettingsAsync();
}

public static class UISettingsProviderExtensions
{
    internal static CompositeDisposable WhenLoadedOrSetWeak(
        this IUISettingsProvider provider,
        Action<CompositeDisposable, UISettings> onLoad,
        Action<CompositeDisposable, UISettingsLoadException>? onException = null)
    {
        var compositeDisposable = new CompositeDisposable();

        var onLoadHandler = provider.HandlePropertyWeak(
            nameof(provider.Settings),
            p => p.Settings,
            (p, s) => { if (s is not null) onLoad(compositeDisposable, s); }
        );

        compositeDisposable.Add(onLoadHandler);

        if (onException is null)
            return compositeDisposable;

        var onExceptionHandler = provider.HandlePropertyWeak(
            nameof(provider.LoadException),
            p => p.LoadException,
            (p, e) => { if (e is not null) onException(compositeDisposable, e); }
        );

        compositeDisposable.Add(onExceptionHandler);
        return compositeDisposable;
    }

    internal static CompositeDisposable WhenLoadedOrSet(
        this IUISettingsProvider provider,
        Action<CompositeDisposable, UISettings> onLoad,
        Action<CompositeDisposable, UISettingsLoadException>? onException = null)
    {
        var compositeDisposable = new CompositeDisposable();

        var onLoadHandler = provider.HandleProperty(
            nameof(provider.Settings),
            p => p.Settings,
            (p, s) => { if (s is not null) onLoad(compositeDisposable, s); }
        );

        compositeDisposable.Add(onLoadHandler);

        if (onException is null)
            return compositeDisposable;

        var onExceptionHandler = provider.HandleProperty(
            nameof(provider.LoadException),
            p => p.LoadException,
            (p, e) => { if (e is not null) onException(compositeDisposable, e); }
        );

        compositeDisposable.Add(onExceptionHandler);
        return compositeDisposable;
    }
}

public partial class UISettingsProvider : ObservableObject, IUISettingsProvider
{
    private readonly string _settingsPath;
    private TaskCompletionSource _settingsLoaded = new TaskCompletionSource();

    [ObservableProperty]
    private UISettings? _settings;

    private UISettingsLoadException? _loadException;

    public UISettingsLoadException? LoadException
    {
        get => _loadException;
        private set => SetProperty(ref _loadException, value);
    }

    public UISettingsProvider(UIEnvironment environment, IDispatcher dispatcher)
    {
        _settingsPath = Path.Combine(environment.AppDataPath, "ui-settings.json");

        dispatcher.Post(async () =>
        {
            try
            {
                Settings = await Task.Run(GetSettingsAsync).ConfigureAwait(false); // avoid I/O on UI thread
                _settingsLoaded.SetResult();
            }
            catch (UISettingsLoadException ex)
            {
                LoadException = ex;
                _settingsLoaded.SetResult();
            }
        }, DispatcherPriority.MaxValue);
    }

    public async Task SaveSettingsAsync()
    {
        await SaveSettingsAsync(Settings);
    }

    private async Task SaveSettingsAsync(UISettings? settings)
    {
        using var fileStream = File.Open(_settingsPath, FileMode.Create, FileAccess.Write);
        await JsonSerializer.SerializeAsync(fileStream, settings, UISettingsContext.Default.Options);
    }

    private async Task<UISettings> GetSettingsAsync()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                var defaultSettings = new UISettings();
                await SaveSettingsAsync(defaultSettings).ConfigureAwait(false);
                return defaultSettings;
            }

            using var fileStream = File.OpenRead(_settingsPath);
            UISettings? settings = null;
            try
            {
                settings = await JsonSerializer.DeserializeAsync<UISettings>(
                    fileStream, UISettingsContext.Default.Options)
                        .ConfigureAwait(false);
            }
            catch (JsonException ex)
            {
                throw new UISettingsLoadException(UISettingsLoadError.ParseError, ex);
            }

            return settings ?? throw new UISettingsLoadException(UISettingsLoadError.FileEmpty);
        }
        catch (Exception ex) when (ex is not UISettingsLoadException)
        {
            throw new UISettingsLoadException(UISettingsLoadError.Unspecified, ex);
        }
    }

    [JsonSerializable(typeof(UISettings))]
    [JsonSourceGenerationOptions(WriteIndented = true)]
    internal partial class UISettingsContext : JsonSerializerContext
    {
    }
}

/// <summary>
/// The exception that is thrown when the UI settings file is invalid.
/// </summary>
public class UISettingsLoadException : Exception
{
    private string? _message;

    public override string Message => GetExceptionMessage();
    public UISettingsLoadError Kind { get; set; }

    public UISettingsLoadException(UISettingsLoadError kind)
    {
        Kind = kind;
    }

    public UISettingsLoadException(UISettingsLoadError kind, Exception innerException)
        : base(null, innerException)
    {
        Kind = kind;
    }

    private string GetExceptionMessage()
    {
        if (string.IsNullOrEmpty(_message))
            _message = "Failed to load UI settings";

        var append = Kind switch
        {
            UISettingsLoadError.FileEmpty => "File is empty",
            UISettingsLoadError.ParseError => "Failed to parse file",
            _ => string.Empty
        };

        return $"{_message}: {append}";
    }
}

/// <summary>
/// The kind of exception that occurred when loading the UI settings.
/// </summary>
public enum UISettingsLoadError
{
    Unspecified,
    FileEmpty,
    ParseError
}
