using System;
using System.Collections.Generic;

namespace OpenTabletDriver.Daemon.Diagnostics
{
    public class EnvironmentDictionary : Dictionary<string, string>
    {
        private static readonly string[] EnvironmentVariables =
        {
            "USER",
            "TEMP",
            "TMP",
            "TMPDIR"
        };

        public EnvironmentDictionary()
        {
            AddVariables(EnvironmentVariables);
        }

        protected EnvironmentDictionary(IEnumerable<string> additionalVariables) : this()
        {
            AddVariables(additionalVariables);
        }

        private void AddVariables(IEnumerable<string> variables)
        {
            foreach (var variable in variables)
                if (Environment.GetEnvironmentVariable(variable) is string value)
                    TryAdd(variable, value);
        }
    }
}
