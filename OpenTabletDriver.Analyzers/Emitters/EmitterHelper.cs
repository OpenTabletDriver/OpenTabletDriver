using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenTabletDriver.Analyzers.Emitters
{
    public static class EmitterHelper
    {
        public static ObjectCreationExpressionSyntax CreateList<T>(TypeSyntax type, IEnumerable<T> expressions) where T : ExpressionSyntax
        {
            var listExpr =
                SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier("global::System.Collections.Generic.List"))
                    .WithTypeArgumentList(
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SingletonSeparatedList(type))))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList());

            if (expressions.Any())
                listExpr = listExpr.WithInitializer(
                    SyntaxFactory.InitializerExpression(
                        SyntaxKind.CollectionInitializerExpression,
                        SyntaxFactory.SeparatedList<ExpressionSyntax>(expressions)));

            return listExpr;
        }

        public static ObjectCreationExpressionSyntax CreateDictionary<T1, T2>(TypeSyntax keyType, TypeSyntax valueType, IEnumerable<(T1 key, T2 value)> expressions)
            where T1 : ExpressionSyntax
            where T2 : ExpressionSyntax
        {
            var dictExpr =
                SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier("global::System.Collections.Generic.Dictionary"))
                    .WithTypeArgumentList(
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SeparatedList(new[]
                            {
                                keyType,
                                valueType
                            }))))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList());

            if (expressions.Any())
                dictExpr = dictExpr.WithInitializer(
                    SyntaxFactory.InitializerExpression(
                        SyntaxKind.CollectionInitializerExpression,
                        SyntaxFactory.SeparatedList<ExpressionSyntax>(
                            expressions.Select(createAssignment))));

            return dictExpr;

            static AssignmentExpressionSyntax createAssignment((T1 key, T2 value) expression)
            {
                return
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.ImplicitElementAccess()
                        .WithArgumentList(
                            SyntaxFactory.BracketedArgumentList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Argument(expression.key)))),
                        expression.value);
            }
        }

        public static ArrayCreationExpressionSyntax CreateByteArray(byte[] byteArray)
        {
            return
                SyntaxFactory.ArrayCreationExpression(
                    SyntaxFactory.ArrayType(
                        SyntaxFactory.PredefinedType(
                            SyntaxFactory.Token(SyntaxKind.ByteKeyword)))
                    .WithRankSpecifiers(
                        SyntaxFactory.SingletonList(
                            SyntaxFactory.ArrayRankSpecifier(
                                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                    SyntaxFactory.OmittedArraySizeExpression())))))
                .WithInitializer(
                    SyntaxFactory.InitializerExpression(
                        SyntaxKind.ArrayInitializerExpression,
                        SyntaxFactory.SeparatedList<ExpressionSyntax>(
                            byteArray.Select(b =>
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    SyntaxFactory.Literal($"0x{b:x2}", b))))));
        }
    }
}
