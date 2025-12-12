using System;
using System.Collections.Generic;
using System.Text;
using StreamJsonRpc;

namespace OpenTabletDriver.Plugin.Logging
{
    public class LogMessage
    {
        public LogMessage()
        {
        }

        public LogMessage(RemoteSerializationException rex, LogLevel level)
        {
            fillException(rex);
            Level = level;
            foreach (string exceptionDetails in getRemoteSerializationExceptionData(rex))
                Message += exceptionDetails + Environment.NewLine;
        }

        public LogMessage(Exception exception, LogLevel level = LogLevel.Error)
        {
            fillException(exception);
            Level = level;
        }

        private void fillException(Exception exception)
        {
            Group = exception.GetType().Name;
            Message = exception.Message;
            StackTrace = exception.StackTrace;
        }

        private static IEnumerable<string> getRemoteSerializationExceptionData(RemoteSerializationException rex)
        {
            if (rex.ErrorCode.HasValue)
                yield return rex.ErrorCode.ToString();
            if (rex.ErrorData != null)
                yield return rex.ErrorData.ToString();
            if (rex.DeserializedErrorData != null)
                yield return rex.DeserializedErrorData.ToString();
        }

        /// <summary>
        /// The time in which a log message was created.
        /// </summary>
        public DateTime Time { set; get; } = DateTime.Now;

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

        /// <summary>
        /// True if the log message should create a notification in the user's desktop environment.
        /// </summary>
        public bool Notification { set; get; } = false;

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
