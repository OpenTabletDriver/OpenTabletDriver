using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace OpenTabletDriver.Analyzers.Tests.Verifiers
{
    public static class TabletConfigurationVerifier
    {
        public static Task Verify(
            (string file, string content)[] sources,
            (string file, string content)[] tabletJsonFiles,
            (string file, string content)[] generatedSources,
            (string file, string content)[] analyzerConfigOptions)
        {
            return new IncrementalGeneratorVerifier<TabletConfigurationGenerator>()
            {
                Sources = sources.ToList(),
                AdditionalTexts = tabletJsonFiles.ToList(),
                AnalyzerConfigOptions = analyzerConfigOptions.ToList(),
                GeneratedSources = generatedSources.ToList(),
                AdditionalReferences = GetMetadataReferences()
            }.RunAsync();
        }

        public static Task Verify(
            (string file, string content)[] sources,
            (string file, string content)[] tabletJsonFiles,
            Func<string, string?> analyzerConfigOptionsFactory)
        {
            return new IncrementalGeneratorVerifier<TabletConfigurationGenerator>()
            {
                Sources = sources.ToList(),
                AdditionalTexts = tabletJsonFiles.ToList(),
                AnalyzerConfigOptionsFactory = analyzerConfigOptionsFactory,
                ShouldVerifyGeneratedSources = false,
                AdditionalReferences = GetMetadataReferences()
            }.RunAsync();
        }

        private static List<MetadataReference> GetMetadataReferences()
        {
            return new()
            {
                MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Collections, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Collections.Immutable, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a").Location),
                MetadataReference.CreateFromFile(typeof(TabletConfigurationVerifier).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(TabletConfigurationGenerator).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IEnumerable<>).Assembly.Location)
            };
        }
    }
}
