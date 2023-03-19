using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Analyzers.Emitters;

public class WheelSpecificationsEmitter
{
    public const string CLASS_NAME = "global::OpenTabletDriver.Tablet.WheelSpecifications";

    private readonly WheelSpecifications? _wheelSpecifications;

    public WheelSpecificationsEmitter(WheelSpecifications? wheelSpecifications)
    {
        _wheelSpecifications = wheelSpecifications;
    }

    public ExpressionSyntax Emit()
    {
        if (_wheelSpecifications == null)
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
                                SyntaxFactory.IdentifierName(nameof(WheelSpecifications.StepCount)),
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    SyntaxFactory.Literal(_wheelSpecifications.StepCount))))))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList());
    }
}
