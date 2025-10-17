using CrossCutting.Core.Contract.Aspects;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Level5;
using Logic.Domain.CodeAnalysisManagement.Contract.Exceptions.Level5;

namespace Logic.Domain.CodeAnalysisManagement.Contract.Level5;

[MapException(typeof(Level5ScriptParserException))]
public interface ILevel5ScriptParser
{
    CodeUnitSyntax ParseCodeUnit(string text);
    MethodDeclarationSyntax ParseMethodDeclaration(string text);
    MethodDeclarationMetadataParametersSyntax ParseMethodDeclarationMetadataParameters(string text);
    MethodDeclarationMetadataParameterListSyntax ParseMethodDeclarationMetadataParameterList(string text);
    MethodDeclarationParametersSyntax ParseMethodDeclarationParameters(string text);
    MethodDeclarationBodySyntax ParseMethodDeclarationBody(string text);
    StatementSyntax ParseStatement(string text);
    GotoLabelStatementSyntax ParseGotoLabelStatement(string text);
    ReturnStatementSyntax ParseReturnStatement(string text);
    MethodInvocationExpressionSyntax ParseMethodInvocationExpression(string text);
    MethodInvocationStatementSyntax ParseMethodInvocationStatement(string text);
    MethodInvocationParametersSyntax ParseMethodInvocationParameters(string text);
    CommaSeparatedSyntaxList<ValueExpressionSyntax>? ParseValueList(string text);
    ValueExpressionSyntax ParseValueExpression(string text);
    ValueMetadataParametersSyntax? ParseValueMetadataParameters(string text);
}