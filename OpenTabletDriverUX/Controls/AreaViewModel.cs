namespace OpenTabletDriverUX.Controls
{
    public class AreaViewModel : ViewModelBase
    {
        private float _w, _h, _x, _y, _r, _fW, _fH;
        private string _unit;

        public float Width
        {
            set => this.RaiseAndSetIfChanged(ref _w, value);
            get => _w;
        }
        
        public float Height
        {
            set => this.RaiseAndSetIfChanged(ref _h, value);
            get => _h;
        }

        public float X
        {
            set => this.RaiseAndSetIfChanged(ref _x, value);
            get => _x;
        }

        public float Y
        {
            set => this.RaiseAndSetIfChanged(ref _y, value);
            get => _y;
        }

        public float Rotation
        {
            set => this.RaiseAndSetIfChanged(ref _r, value);
            get => _r;
        }

        public float MaxWidth
        {
            set => this.RaiseAndSetIfChanged(ref _fW, value);
            get => _fW;
        }

        public float MaxHeight
        {
            set => this.RaiseAndSetIfChanged(ref _fH, value);
            get => _fH;
        }

        public string Unit
        {
            set => this.RaiseAndSetIfChanged(ref _unit, value);
            get => _unit;
        }
    }
}