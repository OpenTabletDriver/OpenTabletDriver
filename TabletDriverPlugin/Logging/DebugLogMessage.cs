namespace TabletDriverPlugin.Logging
{
    public class DebugLogMessage : LogMessage
    {
        public DebugLogMessage(string message, bool isError) : base("Debug", message, isError)
        {
        }
    }
}