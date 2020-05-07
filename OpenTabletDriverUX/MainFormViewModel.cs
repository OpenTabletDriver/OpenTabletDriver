using TabletDriverLib;
using TabletDriverPlugin;

namespace OpenTabletDriverUX
{
    public class MainFormViewModel : Notifier
    {
        private Settings _settings;
        public Settings Settings
        {
            set => this.RaiseAndSetIfChanged(ref _settings, value);
            get => _settings;
        }
    }
}