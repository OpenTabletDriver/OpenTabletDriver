using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OpenTabletDriver.UX.ViewModels
{
    public partial class OutputModeViewModel : BaseViewModel
    {
        private readonly TabletViewModel _tabletViewModel;

        [ObservableProperty]
        private double _outputXOffset;

        [ObservableProperty]
        private double _outputYOffset;

        [ObservableProperty]
        private double _outputWidth;

        [ObservableProperty]
        private double _outputHeight;

        [ObservableProperty]
        private double _inputXOffset;

        [ObservableProperty]
        private double _inputYOffset;

        [ObservableProperty]
        private double _inputWidth;

        [ObservableProperty]
        private double _inputHeight;

        [ObservableProperty]
        private double _inputRotation;

        [ObservableProperty]
        private bool _matchOutputAspectRatio;

        [ObservableProperty]
        private bool _clipInputWithinArea;

        [ObservableProperty]
        private bool _discardInputOutsideArea;

        [ObservableProperty]
        private bool _keepAreasWithinBounds;

        public ImmutableArray<Area> OutputAreaBounds { get; }
        public Area InputAreaBounds { get; }

        public OutputModeViewModel(TabletViewModel tabletViewModel)
        {
            _tabletViewModel = tabletViewModel;

            // setup output area
            // setup input area
            // setup output mode selector
            // setup advanced settings
        }
    }
}
