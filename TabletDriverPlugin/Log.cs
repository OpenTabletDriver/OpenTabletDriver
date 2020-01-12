using System;
using System.Diagnostics;
using System.IO;
using TabletDriverPlugin.Logging;

namespace TabletDriverPlugin
{
    public static class Log
    {
        public static event EventHandler<LogMessage> Output;

        public static void Write(string group, string text, bool isError = false)
        {
            var message = new LogMessage(group, text, isError);
            Output?.Invoke(null, message);
            Console.WriteLine(string.Format("[{0}:{1}]\t{2}", message.IsError ? "Error" : "Normal", message.Group, message.Message));
        }

        public static void Debug(string text)
        {
            Write("Debug", text);
        }

        public static void Exception(Exception ex)
        {
            Write(ex.GetType().Name, ex.Message, true);
        }
    }
}