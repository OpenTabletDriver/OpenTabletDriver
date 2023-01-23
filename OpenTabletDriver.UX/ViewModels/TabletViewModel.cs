using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenTabletDriver.UX.Models;

namespace OpenTabletDriver.UX.ViewModels
{
    public partial class TabletViewModel : BaseViewModel
    {
        private readonly TabletHandler _handler;

        [ObservableProperty]
        private OutputModeViewModel _outputModeViewModel;

        [ObservableProperty]
        private BindingsViewModel _bindingsViewModel;

        [ObservableProperty]
        private FiltersViewModel _filtersViewModel;

        [ObservableProperty]
        private InputDeviceState _tabletState;

        [ObservableProperty]
        private bool _profileDirty;

        public int TabletId => _handler.TabletId;
        public IAsyncRelayCommand StartPipelineCommand { get; }
        public IAsyncRelayCommand ApplyProfileCommand { get; }
        public IAsyncRelayCommand DiscardProfileCommand { get; }
        public IAsyncRelayCommand ResetProfileCommand { get; }

        public TabletViewModel(TabletHandler handler)
        {
            _handler = handler;

            _handler.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(_handler.Profile))
                {
                    OutputModeViewModel = new OutputModeViewModel(this);
                    BindingsViewModel = new BindingsViewModel(this);
                    FiltersViewModel = new FiltersViewModel(this);
                }
                else if (e.PropertyName == nameof(_handler.TabletState))
                {
                    TabletState = _handler.TabletState;
                }
            };

            _tabletState = _handler.TabletState;
            _outputModeViewModel = new OutputModeViewModel(this);
            _bindingsViewModel = new BindingsViewModel(this);
            _filtersViewModel = new FiltersViewModel(this);

            StartPipelineCommand = new AsyncRelayCommand(StartPipelineAsync, canExecute: () => TabletState != InputDeviceState.Normal);
            ApplyProfileCommand = new AsyncRelayCommand(ApplyProfileAsync, canExecute: () => ProfileDirty);
            DiscardProfileCommand = new AsyncRelayCommand(DiscardProfileAsync, canExecute: () => ProfileDirty);
            ResetProfileCommand = new AsyncRelayCommand(ResetProfileAsync);
        }

        private async Task StartPipelineAsync()
        {
            await _handler.SetTabletState(InputDeviceState.Normal);
        }

        private async Task ApplyProfileAsync()
        {
            await _handler.ApplyProfile();
            ProfileDirty = false;
        }

        private async Task DiscardProfileAsync()
        {
            await _handler.DiscardProfile();
            ProfileDirty = false;
        }

        private async Task ResetProfileAsync()
        {
            await _handler.ResetProfile();
            ProfileDirty = false;
        }
    }
}
