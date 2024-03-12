using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Analyzers.Emitters
{
    public sealed class ButtonSpecificationsEmitter
    {
        public const string CLASS_NAME = "global::OpenTabletDriver.Tablet.ButtonSpecifications";

        private readonly ButtonSpecifications? _buttonSpecifications;

        public ButtonSpecificationsEmitter(ButtonSpecifications? buttonSpecifications)
        {
            _buttonSpecifications = buttonSpecifications;
        }

        public ExpressionSyntax Emit()
        {
            if (_buttonSpecifications == null)
                return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);

            return
                SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.ParseTypeName(CLASS_NAME))
                .WithInitializer(
                    SyntaxFactory.InitializerExpression(
                        SyntaxKind.ObjectInitializerExpression,
                        SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(nameof(ButtonSpecifications.ButtonCount)),
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    SyntaxFactory.Literal(_buttonSpecifications.ButtonCount))))))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList());
        }
    }
}
