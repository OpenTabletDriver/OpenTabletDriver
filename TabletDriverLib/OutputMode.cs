using System;
using TabletDriverLib.Component;

namespace TabletDriverLib
{
    public abstract class OutputMode
    {
        public virtual void Read(TabletReport report)
        {
            try
            {
                Position(report);
            }
            catch {}
        }

        public abstract void Position(TabletReport report);
        public virtual TabletProperties TabletProperties { set; get; }
    }
}