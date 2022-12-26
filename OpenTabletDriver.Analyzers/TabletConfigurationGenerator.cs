using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Newtonsoft.Json;
using OpenTabletDriver.Analyzers.Emitters;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Analyzers
{
    [Generator]
    public class TabletConfigurationGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var json = context.AdditionalTextsProvider
                .Where(t => Path.GetExtension(t.Path) == ".json");

            var tabletConfigurationJsons = json
                .Combine(context.AnalyzerConfigOptionsProvider)
                .Where(t => IsTabletConfiguration(t.Left, t.Right))
                .Select((t, _) => ParseConfiguration(t.Left))
                .Collect();

            context.RegisterSourceOutput(tabletConfigurationJsons, (ctx, jsons) =>
            {
                // TODO: automatically get the namespace and class
                var target = new ProviderTarget("DeviceConfigurationProvider", "OpenTabletDriver.Configurations");

                try
                {
                    var emitter = new DeviceConfigurationProviderEmitter(target, jsons);
                    ctx.AddSource($"{target.ClassName}.g.cs", emitter.Emit());
                }
                catch (TabletConfigurationEmitter.EmitterException ex)
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.TabletConfigurationEmitterError,
                        Location.None,
                        ex.Message
                    ));
                }
            });
        }

        private static bool IsTabletConfiguration(AdditionalText text, AnalyzerConfigOptionsProvider optionsProvider)
        {
            var options = optionsProvider.GetOptions(text);
            return options.TryGetValue("build_metadata.AdditionalFiles.TabletConfiguration", out var value) && value == "true";
        }

        private static TabletConfiguration ParseConfiguration(AdditionalText text)
        {
            return JsonConvert.DeserializeObject<TabletConfiguration>(text.GetText()!.ToString())!;
        }
    }

    public static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor TabletConfigurationEmitterError = new(
            "OTD0001",
            "Tablet Configuration Emitter Error",
            "{0}",
            "OpenTabletDriver.Analyzers",
            DiagnosticSeverity.Error,
            true
        );
    }
}
