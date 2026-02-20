using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Components;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Configurations
{
    public class ReportParserProvider : IReportParserProvider
    {
        private readonly Dictionary<string, Func<IReportParser<IDeviceReport>>> _reportParsers;

        public ReportParserProvider()
        {
            var assemblies = new[]
            {
                typeof(ReportParserProvider).Assembly,
                typeof(IDriver).Assembly
            };

            _reportParsers = CreateParsersFromAssembly(assemblies);
        }

        [Pure]
        public IReportParser<IDeviceReport> GetReportParser(string reportParserName)
        {
            return _reportParsers[reportParserName].Invoke();
        }

        [Pure]
        private static Func<IReportParser<IDeviceReport>> GetConstructor(Type reportParserType)
        {
            return () => (IReportParser<IDeviceReport>)(Activator.CreateInstance(reportParserType)
                                                        ?? throw new InvalidOperationException($"Unable to create instance of {reportParserType}"));
        }

        [Pure]
        private static Dictionary<string, Func<IReportParser<IDeviceReport>>> CreateParsersFromAssembly(params Assembly[] assemblies)
        {
            return assemblies.SelectMany(asm => asm.ExportedTypes)
                .Where(t => !string.IsNullOrEmpty(t.FullName) && t.IsAssignableTo(typeof(IReportParser<IDeviceReport>)))
                .ToDictionary(
                    t => t.FullName!,
                    GetConstructor
                );
        }
    }
}
