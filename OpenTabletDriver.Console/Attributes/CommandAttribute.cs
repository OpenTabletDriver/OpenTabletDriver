using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Console.Attributes
{
    [AttributeUsage(AttributeTargets.Method), MeansImplicitUse]
    public class CommandAttribute : Attribute
    {
        public CommandAttribute(string command)
        {
            Command = command;
        }

        public CommandAttribute(string command, string description)
            : this(command)
        {
            Description = description;
        }

        public string Command { get; }
        public string? Description { get; }
    }
}
