using System;

#nullable enable

namespace OpenTabletDriver
{
    public class DriverAlreadyBuiltException : Exception
    {
        public DriverAlreadyBuiltException() : this("A driver instance has already been built")
        {
        }

        public DriverAlreadyBuiltException(string? message) : base(message)
        {
        }

        public DriverAlreadyBuiltException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
