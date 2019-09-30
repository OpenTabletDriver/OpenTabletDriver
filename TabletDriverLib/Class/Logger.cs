using System.Diagnostics;
using System.IO;
using TabletDriverLib.Interface;

namespace TabletDriverLib.Class
{
    public class Logger : ILogger
    {
        public void Write(string text)
        {
            Trace.Write(text);
        }

        public void WriteLine(string text)
        {
            Trace.WriteLine(text);
        }

        public void WriteLine(string prefix, string text)
        {
            Trace.WriteLine($"{prefix}: {text}");
        }
    }
}