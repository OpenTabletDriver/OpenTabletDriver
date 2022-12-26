using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace OpenTabletDriver.Analyzers.Tests.Verifiers
{
    public sealed class CustomAdditionalText : AdditionalText
    {
        private readonly SourceText _text;

        public override string Path { get; }

        public CustomAdditionalText(string file, string content)
        {
            Path = file;
            _text = SourceText.From(content, encoding: Encoding.UTF8);
        }

        public override SourceText? GetText(CancellationToken cancellationToken = default)
        {
            return _text;
        }
    }
}
