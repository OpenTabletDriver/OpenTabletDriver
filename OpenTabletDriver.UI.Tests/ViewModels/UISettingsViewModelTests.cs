using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.UI.Models;
using OpenTabletDriver.UI.Services;
using OpenTabletDriver.UI.ViewModels;

namespace OpenTabletDriver.UI.Tests.ViewModels;

public partial class UISettingsViewModelTests
{
    [Fact]
    public async Task TestModified()
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IAutoStartService, NullAutoStartService>()
            .AddSingleton<UISettingsProviderMock>()
            .AddSingleton<IUISettingsProvider, UISettingsProviderMock>(s => s.GetRequiredService<UISettingsProviderMock>())
            .AddTransient<UISettingsViewModel>()
            .BuildServiceProvider();

        var settingsProvider = serviceProvider.GetRequiredService<UISettingsProviderMock>();
        var viewModel = serviceProvider.GetRequiredService<UISettingsViewModel>();

        viewModel.OnActivated();
        viewModel.Settings.Should().Be(settingsProvider.Settings);
        viewModel.Modified.Should().BeFalse();

        using (new AssertionScope("Modified is true when settings are modified"))
        {
            var kaomoji = viewModel.Settings!.Kaomoji;
            viewModel.Settings!.Kaomoji = !kaomoji;
            viewModel.Modified.Should().BeTrue();
        }

        using (new AssertionScope("Modified is false when settings are saved"))
        {
            await viewModel.SaveSettingsCommand.ExecuteAsync(null);
            viewModel.Modified.Should().BeFalse();
        }
    }

    private partial class UISettingsProviderMock : ObservableObject, IUISettingsProvider
    {
        [ObservableProperty]
        private UISettings? _settings = new();

        public UISettingsLoadException? LoadException { get; }

        public int SaveSettingsAsyncCalled { get; private set; }

        public Task SaveSettingsAsync()
        {
            SaveSettingsAsyncCalled++;
            return Task.CompletedTask;
        }
    }
}
