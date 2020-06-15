using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.CommandLine;

namespace OpenTabletDriver.Console
{
    public static class Tools
    {
        public static void AddRange(this Command command, IEnumerable<Symbol> symbols)
        {
            foreach (var sym in symbols)
                command.Add(sym);
        }

        public static IEnumerable<string> GetFormattedBindings(Collection<string> bindings)
        {
            for (int i = 0; i < bindings.Count; i++)
                yield return $"[{i}, {bindings[i]}]";
        }
    }
}