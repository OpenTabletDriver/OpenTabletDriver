using TabletDriverLib.Tablet;

namespace TabletDriverLib
{
    public abstract class OutputMode
    {
        public virtual void Read(ITabletReport report)
        {
            Position(report);
        }

        public abstract void Position(ITabletReport report);
        public virtual TabletProperties TabletProperties { set; get; }
    }
}