using System;
using System.Text;

namespace OpenTabletDriver.Plugin.Logging
{
    public class LogMessage
    {
        public LogMessage()
        {
        }

        public LogMessage(Exception exception)
        {
            Group = exception.GetType().Name; 
            Message = exception.Message;
            Level = LogLevel.Error;
            StackTrace = exception.StackTrace;
        }

        /// <summary>
        /// The time in which a log message was created.
        /// </summary>
        public DateTime Time { private set; get; } = DateTime.Now;

        /// <summary>
        /// The group in which the log message belongs to.
        /// </summary>
        public string Group { set; get; }

        /// <summary>
        /// The content of the log message.
        /// </summary>
        public string Message { set; get; }

        /// <summary>
        /// The stack trace at the time of the log message.
        /// </summary>
        public string StackTrace { set; get; }

        /// <summary>
        /// The severity level of the log message.
        /// </summary>
        public LogLevel Level { set; get; }

        public override string ToString()
        {
            return string.Format(
                "[{0}:{1}]\t{2}",
                this.Group,
                Enum.GetName(typeof(LogLevel), this.Level),
                this.Message
            );
        }
    }
}