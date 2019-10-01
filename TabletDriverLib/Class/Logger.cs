using System.Diagnostics;
using System.IO;

namespace TabletDriverLib.Class
{
    internal class Logger
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
            Trace.WriteLine($"[{prefix}] {text}");
        }
    }
}