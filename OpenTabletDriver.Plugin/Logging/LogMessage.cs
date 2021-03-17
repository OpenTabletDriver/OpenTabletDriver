using System;

namespace OpenTabletDriver.Plugin.Logging
{
    public class LogMessage
    {
        public LogMessage(string group, string message, LogLevel level)
        {
            Group = group;
            Message = message;
            Level = level;
        }

        /// <summary>
        /// The time in which a log message was created.
        /// </summary>
        public DateTime Time { private set; get; } = DateTime.Now;

        /// <summary>
        /// The group in which the log message belongs to.
        /// </summary>
        public string Group { private set; get; }

        /// <summary>
        /// The content of the log message.
        /// </summary>
        public string Message { private set; get; }

        /// <summary>
        /// The severity level of the log message.
        /// </summary>
        public LogLevel Level { private set; get; }
    }
}