using System;
using System.Diagnostics;
using System.IO;
using TabletDriverPlugin.Logging;

namespace TabletDriverPlugin
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    public static class Log
    {
        public static event EventHandler<LogMessage> Output;

        private static void Post(LogMessage message)
        {
            Output?.Invoke(null, message);
        }

        public static string GetStringFormat(LogMessage message)
        {
            string level = Enum.GetName(typeof(LogLevel), message.Level);
            var text = string.Format("[{0}:{1}]\t{2}", message.Group, level, message.Message);
            
            // Append stack trace if an exception was caught.
            if (message is ExceptionLogMessage exceptionMessage)
                text += Environment.NewLine + exceptionMessage.Exception.StackTrace;

            return text;
        }

        public static void Write(string group, string text, LogLevel level = LogLevel.Info)
        {
            var message = new LogMessage(group, text, level);
            Post(message);
        }

        public static void Debug(string group, string text)
        {
            var message = new LogMessage(group, text, LogLevel.Debug);
            Post(message);
        }

        public static void Exception(Exception ex)
        {
            var message = new ExceptionLogMessage(ex);
            Post(message);
        }
    }
}