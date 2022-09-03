using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Attributes
{
    /// <summary>
    /// Assigns a build date to a target assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    [PublicAPI]
    public class BuildDateAttribute : Attribute
    {
        public BuildDateAttribute(string buildDate)
        {
            BuildDate = buildDate;
        }

        /// <summary>
        /// The date in which the assembly was built.
        /// </summary>
        public string BuildDate { get; }
    }
}
