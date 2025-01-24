using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Daemon.Contracts;

namespace OpenTabletDriver.Daemon.Library.Diagnostics
{
    public class EnvironmentDictionary : Dictionary<string, string>, IEnvironmentDictionary
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

        public Dictionary<string, string> Variables => this;

        private void AddVariables(IEnumerable<string> variables)
        {
            foreach (var variable in variables)
                if (Environment.GetEnvironmentVariable(variable) is string value)
                    TryAdd(variable, value);
        }
    }
}
