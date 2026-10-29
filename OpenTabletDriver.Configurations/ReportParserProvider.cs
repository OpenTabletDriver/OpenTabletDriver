using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Components;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations
{
    public class ReportParserProvider : IReportParserProvider
    {
        private readonly Dictionary<string, Func<IReportParser>> _reportParsers;

        public ReportParserProvider()
        {
            var assemblies = new[]
            {
                typeof(ReportParserProvider).Assembly,
                typeof(IDriver).Assembly
            };

            _reportParsers = CreateParsersFromAssembly(assemblies);
        }

        public IReportParser GetReportParser(string reportParserName)
        {
            return _reportParsers[reportParserName].Invoke();
        }

        private static Func<IReportParser> GetConstructor(Type reportParserType)
        {
            return () => (IReportParser)(Activator.CreateInstance(reportParserType)
                                                        ?? throw new InvalidOperationException($"Unable to create instance of {reportParserType}"));
        }

        private static Dictionary<string, Func<IReportParser>> CreateParsersFromAssembly(params Assembly[] assemblies)
        {
            return assemblies.SelectMany(asm => asm.ExportedTypes)
                .Where(t => t.IsAssignableTo(typeof(IReportParser)))
                .ToDictionary(
                    t => t.FullName!,
                    GetConstructor
                );
        }
    }
}
