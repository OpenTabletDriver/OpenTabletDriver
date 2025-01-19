using System;
using OpenTabletDriver.Plugin.Logging;

namespace OpenTabletDriver.Plugin
{
    public static class Log
    {
        /// <summary>
        /// Event hook to recieve log messages.
        /// </summary>
        public static event EventHandler<LogMessage>? Output;

        /// <summary>
        /// Invoke sending a log message.
        /// </summary>
        /// <param name="message">The message to be passed to the <see cref="Output"/> event.</param>
        public static void Write(LogMessage message)
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
            string text = message.ToString();

            // Append stack trace if an exception was caught.
            if (!string.IsNullOrWhiteSpace(message.StackTrace))
                text += Environment.NewLine + message.StackTrace;

            return text;
        }

        /// <summary>
        /// Write to the log event with a group, the text, and a log level.
        /// </summary>
        /// <param name="group">The group in which the <see cref="LogMessage"/> belongs to.</param>
        /// <param name="text">Text for the <see cref="LogMessage"/>.</param>
        /// <param name="level">The severity level of the <see cref="LogMessage"/>.</param>
        /// <param name="notify">Whether or not the log message should create a notification in the user's desktop environment.</param>
        public static void Write(string group, string text, LogLevel level = LogLevel.Info, bool createStackTrace = false, bool notify = false)
        {
            var message = new LogMessage
            {
                Group = group,
                Message = text,
                Level = level,
                StackTrace = createStackTrace ? Environment.StackTrace : null,
                Notification = notify
            };
            Write(message);
        }

        /// <summary>
        /// Writes to the log event with a group, text and severity level, creating a notification in the user's
        /// desktop environment.
        /// </summary>
        /// <param name="group">The group in which the <see cref="LogMessage"/> belongs to.</param>
        /// <param name="text">Text for the <see cref="LogMessage"/>.</param>
        /// <param name="level">Level of severity for the message, see <see cref="LogLevel"/>. Defaults to Info.</param>
        public static void WriteNotify(string group, string text, LogLevel level = LogLevel.Info)
        {
            Write(group, text, level, false, true);
        }

        /// <summary>
        /// Writes to the log event with a group and text to with the debug severity level.
        /// </summary>
        /// <param name="group">The group in which the <see cref="LogMessage"/> belongs to.</param>
        /// <param name="text">Text for the <see cref="LogMessage"/>.</param>
        public static void Debug(string group, string text)
        {
            Write(group, text, LogLevel.Debug);
        }

        /// <summary>
        /// Writes to the log event with an exception, encoding its stack trace.
        /// </summary>
        /// <param name="ex">The <see cref="System.Exception"/> object to create the <see cref="LogMessage"/> from.</param>
        public static void Exception(Exception ex, LogLevel level = LogLevel.Error)
        {
            if (ex == null)
                return;

            var message = new LogMessage(ex, level);
            Write(message);
        }
    }
}
