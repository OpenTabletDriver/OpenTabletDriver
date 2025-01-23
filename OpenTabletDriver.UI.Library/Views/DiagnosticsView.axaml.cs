using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.DependencyInjection;
using Newtonsoft.Json;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.UI.Controls;
using OpenTabletDriver.UI.Services;

namespace OpenTabletDriver.UI.Views;

public partial class DiagnosticsView : ActivatableUserControl
{
    private readonly IDaemonService _daemonService;

    public DiagnosticsView()
    {
        InitializeComponent();
        _daemonService = Ioc.Default.GetRequiredService<IDaemonService>();
    }

    private async void ExportDiagnostics_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_daemonService.Instance == null) throw new Exception();
        var toplevel = TopLevel.GetTopLevel(this);
        var result = await Task.Run(_daemonService.Instance.GetDiagnostics);
        var outputString = JsonConvert.SerializeObject(result, Formatting.Indented);
        var file = await toplevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions(){
            Title = "Export Diagnostics",
            DefaultExtension = "json",
        });

        if (file is not null)
        {
            await using var stream = await file.OpenWriteAsync();
            await using var streamWriter = new StreamWriter(stream);
            await streamWriter.WriteLineAsync(outputString);
        }
    }
}
