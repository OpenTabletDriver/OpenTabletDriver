using System;
using System.Diagnostics;
using System.IO;
using TabletDriverLib.Component;

namespace TabletDriverLib
{
    public static class Log
    {
        public static event EventHandler<LogMessage> Output;

        public static void Write(string group, string text, bool isError = false)
        {
            var logMessage = new LogMessage(group, text, isError);
            Output?.Invoke(null, logMessage);
            Console.WriteLine($"[{group}:{(isError ? "Error" : "Normal")}]\t{text}");
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