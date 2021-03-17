using System;
using OpenTabletDriver.Plugin.Logging;

namespace OpenTabletDriver.Plugin
{
    public static class Log
    {
        /// <summary>
        /// Event hook to recieve log messages.
        /// </summary>
        public static event EventHandler<LogMessage> Output;

        /// <summary>
        /// Invoke sending a log message.
        /// </summary>
        /// <param name="message">The message to be passed to the <see cref="Output"/> event.</param>
        public static void OnOutput(LogMessage message)
        {
            Output?.Invoke(null, message);
        }

        /// <summary>
        /// Returns the log message formatted as a string.
        /// </summary>
        /// <param name="message">The message to format.</param>
        /// <returns>A formatted string.</returns>
        public static string GetStringFormat(LogMessage message)
        {
            string level = Enum.GetName(typeof(LogLevel), message.Level);
            var text = string.Format("[{0}:{1}]\t{2}", message.Group, level, message.Message);
            
            // Append stack trace if an exception was caught.
            if (message is ExceptionLogMessage exceptionMessage && exceptionMessage.StackTrace != null)
                text += Environment.NewLine + exceptionMessage.StackTrace;

            return text;
        }

        /// <summary>
        /// Write to the log event with a group, the text, and a log level.
        /// </summary>
        /// <param name="group">The group in which the <see cref="LogMessage"/> belongs to.</param>
        /// <param name="text">Text for the <see cref="LogMessage"/>.</param>
        /// <param name="level">The severity level of the <see cref="LogMessage"/>.</param>
        public static void Write(string group, string text, LogLevel level = LogLevel.Info)
        {
            var message = new LogMessage(group, text, level);
            OnOutput(message);
        }

        /// <summary>
        /// Writes to the log event with a group and text to with the debug severity level.
        /// </summary>
        /// <param name="group">The group in which the <see cref="LogMessage"/> belongs to.</param>
        /// <param name="text">Text for the <see cref="LogMessage"/>.</param>
        public static void Debug(string group, string text)
        {
            var message = new LogMessage(group, text, LogLevel.Debug);
            OnOutput(message);
        }

        /// <summary>
        /// Writes to the log event with an exception, encoding its stack trace.
        /// </summary>
        /// <param name="ex">The <see cref="System.Exception"/> object to create the <see cref="LogMessage"/> from.</param>
        public static void Exception(Exception ex)
        {
            if (ex == null)
                return;

            var message = new ExceptionLogMessage(ex.GetType().Name, ex.Message, ex.StackTrace);
            OnOutput(message);
        }
    }
}