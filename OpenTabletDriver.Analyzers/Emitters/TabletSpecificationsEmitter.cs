using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Analyzers.Emitters
{
    public sealed class TabletSpecificationsEmitter
    {
        public const string CLASS_NAME = "global::OpenTabletDriver.Tablet.TabletSpecifications";

        private readonly TabletSpecifications _specifications;

        public TabletSpecificationsEmitter(TabletSpecifications specifications)
        {
            _specifications = specifications;
        }

        public ObjectCreationExpressionSyntax Emit()
        {
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
                                SyntaxFactory.IdentifierName(nameof(TabletSpecifications.Digitizer)),
                                new DigitizerSpecificationsEmitter(_specifications.Digitizer).Emit()),
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(nameof(TabletSpecifications.Pen)),
                                new PenSpecificationsEmitter(_specifications.Pen).Emit()),
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(nameof(TabletSpecifications.AuxiliaryButtons)),
                                new ButtonSpecificationsEmitter(_specifications.AuxiliaryButtons).Emit()),
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(nameof(TabletSpecifications.MouseButtons)),
                                new ButtonSpecificationsEmitter(_specifications.MouseButtons).Emit()),
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(nameof(TabletSpecifications.Wheel)),
                                new WheelSpecificationsEmitter(_specifications.Wheel).Emit()),
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(nameof(TabletSpecifications.Touch)),
                                new DigitizerSpecificationsEmitter(_specifications.Touch).Emit()),
                        })))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList());
        }
    }
}
