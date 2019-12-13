using System;
using HidSharp;
using TabletDriverLib.Tablet;

namespace TabletDriverLib
{
    public abstract class DeviceReader<T> : IDisposable where T : IDeviceReport
    {
        public virtual HidStream ReportStream { protected set; get; }
        public bool Reading { protected set; get; }
        public abstract IReportParser<T> Parser { set; get; }
        public virtual event EventHandler<T> Report;

        public abstract void Start();
        public virtual void Stop()
        {
            Reading = false;
        }

        public virtual void Dispose()
        {
            Reading = false;
            ReportStream.Dispose();
        }

        protected void Main()
        {
            try
            {
                Reading = true;
                ReportStream.ReadTimeout = int.MaxValue;
                while (Reading)
                {
                    var data = ReportStream.Read();
                    var report = Parser.Parse(data);
                    Report?.Invoke(this, report);
                    // Logging
                    if (Driver.Debugging)
                        Log.Write("Report", report.ToString());
                }
            }
            catch (ObjectDisposedException)
            {
                // Ignore, this is due to re-detecting tablets
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }
    }
}