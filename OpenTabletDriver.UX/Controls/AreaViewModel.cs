using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;

namespace OpenTabletDriver.UX.Controls
{
    public class AreaViewModel : ViewModelBase
    {
        private float w, h, x, y, r;
        private string unit;
        private IEnumerable<RectangleF> bg;
        private RectangleF fullbg;

        public float Width
        {
            set => this.RaiseAndSetIfChanged(ref this.w, value);
            get => this.w;
        }
        
        public float Height
        {
            set => this.RaiseAndSetIfChanged(ref this.h, value);
            get => this.h;
        }

        public float X
        {
            set => this.RaiseAndSetIfChanged(ref this.x, value);
            get => this.x;
        }

        public float Y
        {
            set => this.RaiseAndSetIfChanged(ref this.y, value);
            get => this.y;
        }

        public float Rotation
        {
            set => this.RaiseAndSetIfChanged(ref this.r, value);
            get => this.r;
        }
        public IEnumerable<RectangleF> Background
        {
            set
            {
                this.RaiseAndSetIfChanged(ref this.bg, value);
                if (Background != null)
                {
                    this.FullBackground = new RectangleF
                    {
                        Left = this.Background.Min(r => r.Left),
                        Top = this.Background.Min(r => r.Top),
                        Right = this.Background.Max(r => r.Right),
                        Bottom = this.Background.Max(r => r.Bottom),
                    };
                }
            }
            get => this.bg;
        }

        public RectangleF FullBackground
        {
            private set => this.RaiseAndSetIfChanged(ref this.fullbg, value);
            get => this.fullbg;
        }

        public string Unit
        {
            set => this.RaiseAndSetIfChanged(ref unit, value);
            get => unit;
        }
    }
}