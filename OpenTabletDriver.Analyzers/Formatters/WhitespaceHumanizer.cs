using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenTabletDriver.Analyzers.Formatters
{
    public class WhitespaceHumanizer : CSharpSyntaxRewriter
    {
        public WhitespaceHumanizer() : base(true)
        {
        }

        public override SyntaxNode? VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            if (node.Initializer != null && node.Initializer.Expressions.Any())
            {
                // from: new something() { ..., ... }
                // to:   new something()
                //       {
                //           ...,
                //           ...
                //       };
                node = node.ReplaceNode(node.Initializer, IndentInitializer(node, node.Initializer));

                if (node.ArgumentList != null)
                {
                    // remove extra newline coming from ArgumentList
                    var oldArgumentList = node.ArgumentList;
                    var newArgumentList = oldArgumentList
                        .WithCloseParenToken(oldArgumentList.CloseParenToken
                            .WithTrailingTrivia(SyntaxFactory.TriviaList()));

                    node = node.ReplaceNode(oldArgumentList, newArgumentList);
                }
            }

            return base.VisitObjectCreationExpression(node);
        }

        public override SyntaxNode? VisitArrayCreationExpression(ArrayCreationExpressionSyntax node)
        {
            if (node.Initializer == null)
                return base.VisitArrayCreationExpression(node);

            if (node.Initializer.Expressions.All(e => e is LiteralExpressionSyntax))
            {
                // from: new something[]{0x02, 0x02}
                // to:   new something[] {0x02, 0x02}
                var oldRankSpecifier = node.Type.RankSpecifiers.Last();
                var newRankSpecifier = oldRankSpecifier.WithTrailingTrivia(SyntaxFactory.Space);

                node = node.ReplaceNode(oldRankSpecifier, newRankSpecifier);

                // from: new something[] {0x02, 0x02}
                // to:   new something[] { 0x02, 0x02 }
                var oldInitializer = node.Initializer!;
                var newInitializer = oldInitializer
                    .WithOpenBraceToken(oldInitializer.OpenBraceToken.WithTrailingTrivia(SyntaxFactory.Space))
                    .WithCloseBraceToken(oldInitializer.CloseBraceToken.WithLeadingTrivia(SyntaxFactory.Space));

                node = node.ReplaceNode(oldInitializer, newInitializer);
            }
            else if (node.Initializer.Expressions.Any(e => e is ObjectCreationExpressionSyntax or AssignmentExpressionSyntax))
            {
                // from: new something[]
                //       {new something(), new something()}   // NormalizeWhitespace() does this for some reason
                // to:   new something[]
                //       {
                //           new something(),
                //           new something()
                //       };
                node = node.ReplaceNode(node.Initializer, IndentInitializer(node, node.Initializer));
            }

            return base.VisitArrayCreationExpression(node);
        }

        public override SyntaxNode? VisitImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax node)
        {
            if (node.Initializer == null)
                return base.VisitImplicitArrayCreationExpression(node);

            if (node.Initializer.Expressions.All(e => e is LiteralExpressionSyntax))
            {
                // from: new[]{0x02, 0x02}
                // to:   new[] {0x02, 0x02}
                var oldClose = node.CloseBracketToken;
                var newClose = oldClose.WithTrailingTrivia(SyntaxFactory.Space);

                node = node.ReplaceToken(oldClose, newClose);

                // from: new[] {0x02, 0x02}
                // to:   new[] { 0x02, 0x02 }
                var oldInitializer = node.Initializer!;
                var newInitializer = oldInitializer
                    .WithOpenBraceToken(oldInitializer.OpenBraceToken.WithTrailingTrivia(SyntaxFactory.Space))
                    .WithCloseBraceToken(oldInitializer.CloseBraceToken.WithLeadingTrivia(SyntaxFactory.Space));

                node = node.ReplaceNode(oldInitializer, newInitializer);
            }
            else if (node.Initializer.Expressions.Any(e => e is ObjectCreationExpressionSyntax or AssignmentExpressionSyntax))
            {
                // from: new[]{new something(), new something()}
                // to:   new[]
                //       {
                //           new something(),
                //           new something()
                //       }
                node = node.ReplaceNode(node.Initializer, IndentInitializer(node, node.Initializer));
            }

            return base.VisitImplicitArrayCreationExpression(node);
        }

        private static InitializerExpressionSyntax IndentInitializer(SyntaxNode node, InitializerExpressionSyntax syntax, string indentation = "    ")
        {
            var currIndentation = GetIndentation(node);
            var oldInitializer = syntax;
            var newInitializer = oldInitializer
                .WithOpenBraceToken(oldInitializer.OpenBraceToken
                    .WithLeadingTrivia(
                        SyntaxFactory.TriviaList(
                            SyntaxFactory.LineFeed,
                            SyntaxFactory.Whitespace(currIndentation))))
                .WithCloseBraceToken(oldInitializer.CloseBraceToken
                    .WithLeadingTrivia(
                        SyntaxFactory.TriviaList(
                            SyntaxFactory.LineFeed,
                            SyntaxFactory.Whitespace(currIndentation))))
                .WithExpressions(
                    SyntaxFactory.SeparatedList(
                        oldInitializer.Expressions.Select(e => e
                            .WithLeadingTrivia(
                                SyntaxFactory.TriviaList(
                                    SyntaxFactory.LineFeed,
                                    SyntaxFactory.Whitespace(currIndentation + indentation))))));

            return newInitializer;
        }

        private static bool TryGetIdentation(SyntaxNode node, out SyntaxTrivia indentation)
        {
            if (node.HasLeadingTrivia
                && node.GetLeadingTrivia().LastOrDefault() is SyntaxTrivia trivia
                && trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                indentation = trivia;
                return true;
            }

            indentation = default;
            return false;
        }

        private static string GetIndentation(SyntaxNode node)
        {
            var curNode = node;
            while (curNode != null)
            {
                if (TryGetIdentation(curNode, out var indentation))
                    return indentation.ToFullString();
                curNode = curNode.Parent;
            }

            return string.Empty;
        }
    }
}
