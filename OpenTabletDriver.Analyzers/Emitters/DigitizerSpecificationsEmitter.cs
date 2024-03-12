using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Analyzers.Emitters
{
    public sealed class DigitizerSpecificationsEmitter
    {
        public const string CLASS_NAME = "global::OpenTabletDriver.Tablet.DigitizerSpecifications";

        private readonly DigitizerSpecifications? _digitizer;

        public DigitizerSpecificationsEmitter(DigitizerSpecifications? digitizer)
        {
            _digitizer = digitizer;
        }

        public ExpressionSyntax Emit()
        {
            if (_digitizer == null)
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
                                SyntaxFactory.IdentifierName(nameof(DigitizerSpecifications.Width)),
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    SyntaxFactory.Literal(_digitizer.Width))),
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(nameof(DigitizerSpecifications.Height)),
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    SyntaxFactory.Literal(_digitizer.Height))),
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(nameof(DigitizerSpecifications.MaxX)),
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    SyntaxFactory.Literal(_digitizer.MaxX))),
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(nameof(DigitizerSpecifications.MaxY)),
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    SyntaxFactory.Literal(_digitizer.MaxY)))
                        })))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList());
        }
    }
}
