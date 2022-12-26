using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Analyzers.Emitters
{
    public sealed class PenSpecificationsEmitter
    {
        public const string CLASS_NAME = "global::OpenTabletDriver.Tablet.PenSpecifications";

        private readonly PenSpecifications? pen;

        public PenSpecificationsEmitter(PenSpecifications? pen)
        {
            this.pen = pen;
        }

        public ExpressionSyntax Emit()
        {
            if (pen == null)
                return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);

            return
                SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.ParseTypeName(CLASS_NAME))
                .WithInitializer(
                    SyntaxFactory.InitializerExpression(
                        SyntaxKind.ObjectInitializerExpression,
                        SyntaxFactory.SeparatedList<ExpressionSyntax>(new[]
                        {
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(nameof(PenSpecifications.MaxPressure)),
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    SyntaxFactory.Literal(pen.MaxPressure))),
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(nameof(PenSpecifications.ButtonCount)),
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    SyntaxFactory.Literal(pen.ButtonCount))),
                        })))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList());
        }
    }
}
