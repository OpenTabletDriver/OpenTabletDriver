using System.Collections.Generic;
using Eto.Drawing;

namespace OpenTabletDriver.UX.Controls
{
    public class AreaViewModel : ViewModelBase
    {
        private float _w, _h, _x, _y, _r;
        private string _unit;
        private IEnumerable<RectangleF> _bg;

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
        public IEnumerable<RectangleF> Background
        {
            set => this.RaiseAndSetIfChanged(ref _bg, value);
            get => _bg;
        }

        public string Unit
        {
            set => this.RaiseAndSetIfChanged(ref _unit, value);
            get => _unit;
        }
    }
}