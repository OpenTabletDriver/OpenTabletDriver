using TabletDriverLib;
using TabletDriverPlugin;

namespace OpenTabletDriver.UX
{
    public class MainFormViewModel : Notifier
    {
        public Settings Settings
        {
            set
            {
                App.Settings = value;
                RaiseChanged();
            }
            get => App.Settings;
        }
    }
}