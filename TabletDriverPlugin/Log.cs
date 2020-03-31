using System;
using System.Diagnostics;
using System.IO;
using TabletDriverPlugin.Logging;

namespace TabletDriverPlugin
{
    public static class Log
    {
        public static event EventHandler<LogMessage> Output;
        
        private static void Post(LogMessage message)
        {
            Output?.Invoke(null, message);
            Console.WriteLine(string.Format("[{0}:{1}]\t{2}", message.IsError ? "Error" : "Normal", message.Group, message.Message));
        }

        public static string GetStringFormat(LogMessage message)
        {
            var text = string.Format("[{0}:{1}]\t{2}", message.IsError ? "Error" : "Normal", message.Group, message.Message);
            
            // Append stack trace if an exception was caught.
            if (message is ExceptionLogMessage exceptionMessage)
                text += Environment.NewLine + exceptionMessage.Exception.StackTrace;

            return text;
        }

        public static void Write(string group, string text, bool isError = false)
        {
            var message = new LogMessage(group, text, isError);
            Post(message);
        }

        public static void Debug(string text, bool isError = false)
        {
            var message = new DebugLogMessage(text, isError);
            Post(message);
        }

        public static void Exception(Exception ex)
        {
            var message = new ExceptionLogMessage(ex);
            Post(message);
        }
    }
}