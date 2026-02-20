using System;
using System.Collections.Generic;
using OpenTabletDriver.Plugin.Logging;

#nullable enable

namespace OpenTabletDriver.Plugin
{
    public static class Log
    {
        private static List<LogMessage>? _backlog = new();
        private static Action<LogMessage> _logAction = WriteBacklog;
        private static event EventHandler<LogMessage>? _output;

        /// <summary>
        /// Event hook to receive log messages.
        /// </summary>
        public static event EventHandler<LogMessage>? Output
        {
            add
            {
                if (_output == null && value != null)
                {
                    foreach (var message in _backlog!)
                        value.Invoke(null, message);
                    _backlog = null;
                    _logAction = WriteLog;
                }

                _output += value;
            }
            remove
            {
                _output -= value;
                if (_output == null)
                {
                    _backlog = new List<LogMessage>();
                    _logAction = WriteBacklog;
                }
            }
        }

        /// <summary>
        /// Invoke sending a log message.
        /// </summary>
        /// <param name="message">The message to be passed to the <see cref="Output"/> event.</param>
        public static void Write(LogMessage message)
        {
            _logAction(message);
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
        /// <param name="createStackTrace">Whether to include stack trace in the log message</param>
        /// <param name="notify">Whether the log message should create a notification in the user's desktop environment. BUG: only works when GUI is handling log messages</param>
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
        /// <param name="level">The severity level to label the exception as</param>
        public static void Exception(Exception? ex, LogLevel level = LogLevel.Error)
        {
            if (ex == null)
                return;

            var message = new LogMessage(ex, level);
            Write(message);

            if (ex.InnerException != null)
                Exception(ex.InnerException);
        }

        private static void WriteBacklog(LogMessage message)
        {
            _backlog!.Add(message);
        }

        private static void WriteLog(LogMessage message)
        {
            _output?.Invoke(null, message);
        }
    }
}
