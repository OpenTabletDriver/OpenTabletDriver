namespace OpenTabletDriver.Plugin.Logging
{
    public class ExceptionLogMessage : LogMessage
    {
        public ExceptionLogMessage(string fullName, string message, string stackTrace) : base(fullName, message, LogLevel.Error)
        {
            StackTrace = stackTrace;
        }

        /// <summary>
        /// The stack trace of the exception to be passed along in the log message
        /// </summary>
        public string StackTrace { private set; get; }
    }
}