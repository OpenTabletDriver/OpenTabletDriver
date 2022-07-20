using System;
using System.Collections.Generic;

namespace OpenTabletDriver.Desktop.Diagnostics
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

        protected EnvironmentDictionary(string[] additionalVariables) : this()
        {
            AddVariables(additionalVariables);
        }

        private void AddVariables(string[] variables)
        {
            foreach (var variable in variables)
            {
                var value = Environment.GetEnvironmentVariable(variable);
                Add(variable, value);
            }
        }
    }
}
