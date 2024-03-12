using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Analyzers.Emitters
{
    public sealed class DeviceIdentifierEmitter
    {
        public const string CLASS_NAME = "global::OpenTabletDriver.Tablet.DeviceIdentifier";

        private readonly DeviceIdentifier _deviceIdentifier;

        public DeviceIdentifierEmitter(DeviceIdentifier deviceIdentifier)
        {
            _deviceIdentifier = deviceIdentifier;
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
                                SyntaxFactory.IdentifierName(nameof(DeviceIdentifier.VendorID)),
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    SyntaxFactory.Literal(_deviceIdentifier.VendorID))),
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(nameof(DeviceIdentifier.ProductID)),
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    SyntaxFactory.Literal(_deviceIdentifier.ProductID))),
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(nameof(DeviceIdentifier.InputReportLength)),
                                _deviceIdentifier.InputReportLength is not uint inputReportLength
                                    ? SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                                    : SyntaxFactory.LiteralExpression(
                                        SyntaxKind.NumericLiteralExpression,
                                        SyntaxFactory.Literal(inputReportLength))),
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(nameof(DeviceIdentifier.OutputReportLength)),
                                _deviceIdentifier.OutputReportLength is not uint outputReportLength
                                    ? SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                                    : SyntaxFactory.LiteralExpression(
                                        SyntaxKind.NumericLiteralExpression,
                                        SyntaxFactory.Literal(outputReportLength))),
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(nameof(DeviceIdentifier.ReportParser)),
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal(_deviceIdentifier.ReportParser))),
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(nameof(DeviceIdentifier.FeatureInitReport)),
                                _deviceIdentifier.FeatureInitReport is not List<byte[]> featureInitReport
                                    ? SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                                    : EmitterHelper.CreateList(
                                        SyntaxFactory.ArrayType(
                                            SyntaxFactory.PredefinedType(
                                                SyntaxFactory.Token(SyntaxKind.ByteKeyword)))
                                        .WithRankSpecifiers(
                                            SyntaxFactory.SingletonList(
                                                SyntaxFactory.ArrayRankSpecifier(
                                                    SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                                        SyntaxFactory.OmittedArraySizeExpression())))),
                                        featureInitReport.Select(EmitterHelper.CreateByteArray)
                                    )),
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(nameof(DeviceIdentifier.OutputInitReport)),
                                _deviceIdentifier.OutputInitReport is not List<byte[]> outputInitReport
                                    ? SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                                    : EmitterHelper.CreateList(
                                        SyntaxFactory.ArrayType(
                                            SyntaxFactory.PredefinedType(
                                                SyntaxFactory.Token(SyntaxKind.ByteKeyword)))
                                        .WithRankSpecifiers(
                                            SyntaxFactory.SingletonList(
                                                SyntaxFactory.ArrayRankSpecifier(
                                                    SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                                        SyntaxFactory.OmittedArraySizeExpression())))),
                                        outputInitReport.Select(EmitterHelper.CreateByteArray)
                                    )),
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(nameof(DeviceIdentifier.DeviceStrings)),
                                EmitterHelper.CreateDictionary(
                                    SyntaxFactory.PredefinedType(
                                        SyntaxFactory.Token(SyntaxKind.ByteKeyword)),
                                    SyntaxFactory.PredefinedType(
                                        SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                                    _deviceIdentifier.DeviceStrings.Select(a =>
                                        (SyntaxFactory.LiteralExpression(
                                            SyntaxKind.NumericLiteralExpression,
                                            SyntaxFactory.Literal(a.Key)),
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            SyntaxFactory.Literal(a.Value)))))),
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(nameof(DeviceIdentifier.InitializationStrings)),
                                EmitterHelper.CreateList(
                                    SyntaxFactory.PredefinedType(
                                        SyntaxFactory.Token(SyntaxKind.ByteKeyword)),
                                    _deviceIdentifier.InitializationStrings.Select(a =>
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.NumericLiteralExpression,
                                            SyntaxFactory.Literal(a)))))
                        })))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList());
        }
    }
}
