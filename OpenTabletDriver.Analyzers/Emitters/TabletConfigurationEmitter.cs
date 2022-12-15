using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Analyzers.Emitters
{
    public sealed class TabletConfigurationEmitter
    {
        public const string CLASS_NAME = "global::OpenTabletDriver.Tablet.TabletConfiguration";

        private readonly TabletConfiguration _configuration;

        public TabletConfigurationEmitter(TabletConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ObjectCreationExpressionSyntax Emit()
        {
            try
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
                                    SyntaxFactory.IdentifierName(nameof(TabletConfiguration.Name)),
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal(_configuration.Name))),
                                SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.IdentifierName(nameof(TabletConfiguration.Specifications)),
                                    new TabletSpecificationsEmitter(_configuration.Specifications).Emit()),
                                SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.IdentifierName(nameof(TabletConfiguration.DigitizerIdentifiers)),
                                    _configuration.DigitizerIdentifiers == null
                                        ? SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                                        : EmitterHelper.CreateList(
                                            SyntaxFactory.ParseTypeName(DeviceIdentifierEmitter.CLASS_NAME),
                                            _configuration.DigitizerIdentifiers.Select(d =>
                                                new DeviceIdentifierEmitter(d).Emit()))),
                                SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.IdentifierName(nameof(TabletConfiguration.AuxiliaryDeviceIdentifiers)),
                                    _configuration.AuxiliaryDeviceIdentifiers == null
                                        ? SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                                        : EmitterHelper.CreateList(
                                            SyntaxFactory.ParseTypeName(DeviceIdentifierEmitter.CLASS_NAME),
                                            _configuration.AuxiliaryDeviceIdentifiers.Select(d =>
                                                new DeviceIdentifierEmitter(d).Emit()))),
                                SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.IdentifierName(nameof(TabletConfiguration.Attributes)),
                                    EmitterHelper.CreateDictionary(
                                        SyntaxFactory.PredefinedType(
                                            SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                                        SyntaxFactory.PredefinedType(
                                            SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                                        _configuration.Attributes.Select(a =>
                                            (SyntaxFactory.LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                SyntaxFactory.Literal(a.Key)),
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                SyntaxFactory.Literal(a.Value))))))
                            })))
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList());
            }
            catch (Exception ex)
            {
                throw new EmitterException(_configuration.Name, ex);
            }
        }

        public class EmitterException : Exception
        {
            public string ConfigurationName { get; set; }
            public override string Message => $"Failed to emit code for configuration '{ConfigurationName}'.";

            public EmitterException(string configurationName) : base()
            {
                ConfigurationName = configurationName;
            }

            public EmitterException(string configurationName, Exception innerException) : base(null, innerException)
            {
                ConfigurationName = configurationName;
            }
        }
    }
}
