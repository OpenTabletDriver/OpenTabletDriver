using System;

namespace TabletDriverLib.Component
{
    public struct LogMessage
    {
        internal LogMessage(string group, string message, bool isError)
        {
            Time = DateTime.Now;
            Group = group;
            Message = message;
            IsError = isError;
        }

        public DateTime Time { private set; get; }
        public string Group { private set; get; }
        public string Message { private set; get; }
        public bool IsError { private set; get; }
    }
}