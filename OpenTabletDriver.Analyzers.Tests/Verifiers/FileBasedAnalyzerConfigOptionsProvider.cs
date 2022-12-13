using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace OpenTabletDriver.Analyzers.Tests.Verifiers
{
    public partial class FileBasedAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
    {
        private readonly Dictionary<string, CustomAnalyzerConfigOptions> _options = new();
        private readonly CustomAnalyzerConfigOptions _globalOptions = new();

        public override AnalyzerConfigOptions GlobalOptions => _globalOptions;

        /// <summary>
        /// Sets a function delegate that returns additional options for the given path.
        /// This is called when no options are configured for the given path.
        /// </summary>
        public Func<string, string?>? AdditionalConfigOptionsFactory { get; set; }

        public FileBasedAnalyzerConfigOptionsProvider()
        {
        }

        public FileBasedAnalyzerConfigOptionsProvider(IEnumerable<(string file, string content)> analyzerConfigOptions, Func<string, string?>? additionalConfigOptionsFactory = null)
        {
            AdditionalConfigOptionsFactory = additionalConfigOptionsFactory;
            foreach (var (file, content) in analyzerConfigOptions)
            {
                From(file, content);
            }
        }

        public void Add(string filePath, string key, string value)
        {
            var path = Path.GetFileName(filePath);
            if (!_options.TryGetValue(path, out var options))
            {
                options = new CustomAnalyzerConfigOptions();
                _options.Add(path, options);
            }

            options.Add(key, value);
        }

        public void AddGlobal(string key, string value)
        {
            _globalOptions.Add(key, value);
        }

        public void From(string filePath, string content)
        {
            filePath = Path.GetFileName(filePath);

            if (filePath == "global")
            {
                var matches = (IEnumerable<Match>)ValuePair().Matches(content)!;
                foreach (var match in matches)
                {
                    AddGlobal(match.Groups["key"].Captures[0].Value, match.Groups["value"].Captures[0].Value);
                }
            }
            else
            {
                var matches = (IEnumerable<Match>)ValuePair().Matches(content)!;
                foreach (var match in matches)
                {
                    Add(filePath, match.Groups["key"].Captures[0].Value, match.Groups["value"].Captures[0].Value);
                }
            }
        }

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
        {
            return GetOptions(Path.GetFileName(tree.FilePath));
        }

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
        {
            return GetOptions(Path.GetFileName(textFile.Path));
        }

        private AnalyzerConfigOptions GetOptions(string path)
        {
            if (!_options.TryGetValue(path, out var options))
            {
                if (AdditionalConfigOptionsFactory != null)
                {
                    var content = AdditionalConfigOptionsFactory(path);
                    if (content != null)
                    {
                        From(path, content);
                        _options.TryGetValue(path, out options);
                    }
                }

                options ??= new CustomAnalyzerConfigOptions();
            }

            return options;
        }

        [GeneratedRegex(@"^(?<key>(?:\w|_|-|\.)+) = (?<value>(?:\w|_|-|\.)+)$", RegexOptions.Multiline | RegexOptions.NonBacktracking)]
        private static partial Regex ValuePair();

        private class CustomAnalyzerConfigOptions : AnalyzerConfigOptions
        {
            private readonly Dictionary<string, string> _options = new();

            public void Add(string key, string value)
            {
                _options.Add(key, value);
            }

            public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
            {
                return _options.TryGetValue(key, out value);
            }
        }
    }
}
