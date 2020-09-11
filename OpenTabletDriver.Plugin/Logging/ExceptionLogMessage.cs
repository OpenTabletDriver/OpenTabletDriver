namespace OpenTabletDriver.Plugin.Logging
{
    public class ExceptionLogMessage : LogMessage
    {
        public ExceptionLogMessage(string fullName, string message, string stackTrace) : base(fullName, message, LogLevel.Error)
        {
            StackTrace = stackTrace;
        }

        public string StackTrace { private set; get; }
    }
}