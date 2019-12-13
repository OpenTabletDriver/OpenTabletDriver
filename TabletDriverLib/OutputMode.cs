using TabletDriverLib.Tablet;

namespace TabletDriverLib
{
    public abstract class OutputMode
    {
        public virtual void Read(IDeviceReport report)
        {
            if (report is ITabletReport tabletReport)
                Position(tabletReport);
            else if (report is IAuxReport auxReport)
                Aux(auxReport);
        }

        public abstract void Position(ITabletReport report);
        public abstract void Aux(IAuxReport auxReport);
        public virtual TabletProperties TabletProperties { set; get; }
    }
}