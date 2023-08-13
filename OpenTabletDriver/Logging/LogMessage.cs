using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace OpenTabletDriver.Logging
{
    /// <summary>
    /// A driver log message.
    /// </summary>
    [JsonObject]
    [PublicAPI]
    public class LogMessage
    {
        public LogMessage()
        {
        }

        public LogMessage(Exception exception, LogLevel level = LogLevel.Error, bool notify = false)
        {
            Group = exception.GetType().Name;
            Message = exception.Message;
            Level = level;
            StackTrace = exception.StackTrace;
            Notification = notify;
        }

        /// <summary>
        /// The time in which a log message was created.
        /// </summary>
        public DateTime Time { private set; get; } = DateTime.Now;

        /// <summary>
        /// The group in which the log message belongs to.
        /// </summary>
        public string? Group { set; get; }

        /// <summary>
        /// The content of the log message.
        /// </summary>
        public string? Message { set; get; }

        /// <summary>
        /// The stack trace at the time of the log message.
        /// </summary>
        public string? StackTrace { set; get; }

        /// <summary>
        /// The severity level of the log message.
        /// </summary>
        public LogLevel Level { set; get; }

        /// <summary>
        /// True if the log message should create a notification in the user's desktop environment.
        /// </summary>
        public bool Notification { set; get; }

        public override string ToString()
        {
            return string.Format(
                "[{0}:{1}]\t{2}",
                this.Group,
                Enum.GetName(this.Level),
                this.Message
            );
        }
    }
}
