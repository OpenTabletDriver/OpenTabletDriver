using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Analyzers.Emitters
{
    public sealed class TouchStripSpecificationsEmitter
    {
        public const string CLASS_NAME = "global::OpenTabletDriver.Tablet.TouchStripSpecifications";

        private readonly TouchStripSpecifications? _touchStripSpecifications;

        public TouchStripSpecificationsEmitter(TouchStripSpecifications? touchStripSpecifications)
        {
            _touchStripSpecifications = touchStripSpecifications;
        }

        public ExpressionSyntax Emit()
        {
            if (_touchStripSpecifications == null)
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
                                SyntaxFactory.IdentifierName(nameof(TouchStripSpecifications.Count)),
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    SyntaxFactory.Literal(_touchStripSpecifications.Count))))))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList());
        }
    }
}
