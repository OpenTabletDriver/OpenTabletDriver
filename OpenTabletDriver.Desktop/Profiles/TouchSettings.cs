namespace OpenTabletDriver.Desktop.Profiles
{
    public class TouchSettings : ViewModel
    {
        private bool _disableTouch;

        public bool DisableTouch
        {
            set => this.RaiseAndSetIfChanged(ref this._disableTouch, value);
            get => this._disableTouch;
        }
    }
}
