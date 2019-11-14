using TabletDriverLib.Component;
using TabletDriverLib.Interop.Cursor;

namespace TabletDriverLib
{
    public abstract class OutputMode
    {
        protected OutputMode(TabletProperties tabletProperties)
        {
            TabletProperties = tabletProperties;
        }

        public virtual void Read(TabletReport report)
        {
            Position(report);
        }

        public abstract void Position(TabletReport report);
        public virtual TabletProperties TabletProperties { set; get; }
    }
}