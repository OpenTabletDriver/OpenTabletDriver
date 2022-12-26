using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace OpenTabletDriver.Analyzers.Tests.Verifiers
{
    public partial class IncrementalGeneratorVerifier<TSourceGenerator>
        where TSourceGenerator : IIncrementalGenerator, new()
    {
        private bool _hasRun;

        public List<(string file, string content)> Sources { get; init; } = new();
        public List<(string file, string content)> AdditionalTexts { get; init; } = new();
        public List<(string file, string content)> AnalyzerConfigOptions { get; init; } = new();
        public List<(string file, string content)> GeneratedSources { get; init; } = new();
        public List<MetadataReference> AdditionalReferences { get; init; } = new();

        public bool ShouldVerifyGeneratedSources { get; init; } = true;

        /// <summary>
        /// Sets a function delegate that returns additional options for the given path.
        /// This is called when no options are configured for the given path.
        /// </summary>
        public Func<string, string?>? AnalyzerConfigOptionsFactory { get; init; }

        public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.Default;

        public Task RunAsync()
        {
            if (_hasRun)
                throw new InvalidOperationException("Test has already been run");
            _hasRun = true;

            return Task.Run(() =>
            {
                GeneratorDriver driver = CSharpGeneratorDriver.Create(new TSourceGenerator())
                    .WithUpdatedParseOptions(CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion))
                    .WithUpdatedAnalyzerConfigOptions(new FileBasedAnalyzerConfigOptionsProvider(AnalyzerConfigOptions, AnalyzerConfigOptionsFactory))
                    .AddAdditionalTexts(AdditionalTexts.Select(text =>
                        (AdditionalText)new CustomAdditionalText(text.file, text.content)).ToImmutableArray());

                var compilation = CreateCompilation();
                driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

                var runResult = driver.GetRunResult();

                if (ShouldVerifyGeneratedSources)
                    VerifyGeneratedSources(runResult, GeneratedSources);

                VerifyZeroDiagnostics(outputCompilation, runResult);
            });
        }

        private Compilation CreateCompilation()
        {
            return CSharpCompilation.Create("test",
                references: AdditionalReferences.Append(MetadataReference.CreateFromFile(typeof(object).Assembly.Location)),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddSyntaxTrees(Sources.Select(source => CSharpSyntaxTree.ParseText(source.content, path: source.file)));
        }

        private static void VerifyZeroDiagnostics(Compilation compilation, GeneratorDriverRunResult runResult)
        {
            var compilationDiagnostics = compilation.GetDiagnostics()
                                                  .Where(d => d.Severity == DiagnosticSeverity.Error)
                                                  .ToArray();
            var generatorDiagnostics = runResult.Results
                                                .SelectMany(r => r.Diagnostics)
                                                .Where(d => d.Severity == DiagnosticSeverity.Error)
                                                .ToArray();

            if (compilationDiagnostics.Any() || generatorDiagnostics.Any())
            {
                var sb = new StringBuilder();

                sb.AppendLine("Expected no diagnostics, but found:");

                foreach (var d in compilationDiagnostics)
                    sb.AppendLine($"    Compilation: {d}");

                foreach (var d in generatorDiagnostics)
                    sb.AppendLine($"    Generator: {d}");

                throw new Xunit.Sdk.XunitException(sb.ToString());
            }
        }

        private static void VerifyGeneratedSources(GeneratorDriverRunResult runResult, List<(string file, string content)> expected)
        {
            var generatedSources = runResult.Results
                                            .SelectMany(r => r.GeneratedSources)
                                            .ToDictionary(s => s.HintName);

            if (generatedSources.Count != expected.Count)
                throw new Xunit.Sdk.XunitException($"Expected {expected.Count} generated sources, but found {generatedSources.Count}");

            var matches = 0;

            foreach (var (file, content) in expected)
            {
                if (!generatedSources.TryGetValue(file, out var source))
                    throw new Xunit.Sdk.XunitException($"Expected generated source '{file}', but it was not found");

                var actual = source.SourceText.ToString();

                new XUnitVerifier().EqualOrDiff(content, actual, $"Generated source '{file}' did not match expected content");

                matches++;
            }

            if (matches != expected.Count)
                throw new Xunit.Sdk.XunitException($"Expected {expected.Count} generated sources, but found {matches}");
        }
    }
}
