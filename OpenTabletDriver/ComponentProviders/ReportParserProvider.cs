using System;
using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Plugin.Components;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.ComponentProviders
{
    public class ReportParserProvider : IReportParserProvider
    {
        private readonly Dictionary<string, Func<IReportParser<IDeviceReport>>> _reportParsers;

        public ReportParserProvider()
        {
            _reportParsers = typeof(ReportParserProvider).Assembly.ExportedTypes
                .Where(t => t.IsAssignableTo(typeof(IReportParser<IDeviceReport>)))
                .ToDictionary(
                    t => t.FullName,
                    t => GetConstructor(t));
        }

        public IReportParser<IDeviceReport> GetReportParser(string reportParserName)
        {
            return _reportParsers[reportParserName].Invoke();
        }

        private static Func<IReportParser<IDeviceReport>> GetConstructor(Type reportParserType)
        {
            return () => (IReportParser<IDeviceReport>)Activator.CreateInstance(reportParserType);
        }
    }
}