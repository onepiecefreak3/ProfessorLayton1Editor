using System.Text;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Layton1;
using Logic.Domain.CodeAnalysisManagement.Contract.Level5;

namespace Logic.Domain.CodeAnalysisManagement.Layton1;

internal class Layton1ScriptComposer : ILayton1ScriptComposer
{
    private readonly ILayton1SyntaxFactory _syntaxFactory;

    public Layton1ScriptComposer(ILayton1SyntaxFactory syntaxFactory)
    {
        _syntaxFactory = syntaxFactory;
    }

    public string ComposeCodeUnit(CodeUnitSyntax codeUnit)
    {
        var sb = new StringBuilder();

        ComposeCodeUnit(codeUnit, sb);

        return sb.ToString();
    }

    private void ComposeCodeUnit(CodeUnitSyntax codeUnit, StringBuilder sb)
    {
        ComposeMethodDeclaration(codeUnit.MethodDeclaration, sb);
    }

    private void ComposeMethodDeclaration(MethodDeclarationSyntax? methodDeclaration, StringBuilder sb)
    {
        if (methodDeclaration is null)
            return;

        ComposeSyntaxToken(methodDeclaration.Identifier, sb);
        ComposeMethodDeclarationParameters(methodDeclaration.Parameters, sb);
        ComposeBody(methodDeclaration.Body, sb);
    }

    private void ComposeMethodDeclarationParameters(MethodDeclarationParametersSyntax methodDeclarationParameters, StringBuilder sb)
    {
        ComposeSyntaxToken(methodDeclarationParameters.ParenOpen, sb);
        ComposeSyntaxToken(methodDeclarationParameters.ParenClose, sb);
    }

    private void ComposeBody(BodySyntax body, StringBuilder sb)
    {
        ComposeSyntaxToken(body.CurlyOpen, sb);

        foreach (StatementSyntax expression in body.Statements)
            ComposeStatement(expression, sb);

        ComposeSyntaxToken(body.CurlyClose, sb);
    }

    private void ComposeStatement(StatementSyntax statement, StringBuilder sb)
    {
        switch (statement)
        {
            case ReturnStatementSyntax returnStatement:
                ComposeReturnStatement(returnStatement, sb);
                break;

            case BreakStatementSyntax breakStatement:
                ComposeBreakStatement(breakStatement, sb);
                break;

            case WhileStatementSyntax whileStatement:
                ComposeWhileStatement(whileStatement, sb);
                break;

            case IfStatementSyntax ifStatement:
                ComposeIfStatement(ifStatement, sb);
                break;

            case IfElseStatementSyntax ifElseStatement:
                ComposeIfElseStatement(ifElseStatement, sb);
                break;

            case MethodInvocationStatementSyntax methodInvocationStatement:
                ComposeMethodInvocationStatement(methodInvocationStatement, sb);
                break;
        }
    }

    private void ComposeWhileStatement(WhileStatementSyntax whileStatement, StringBuilder sb)
    {
        ComposeSyntaxToken(whileStatement.While, sb);
        ComposeExpression(whileStatement.Expression, sb);
        ComposeBody(whileStatement.Body, sb);
    }

    private void ComposeIfStatement(IfStatementSyntax ifStatement, StringBuilder sb)
    {
        ComposeSyntaxToken(ifStatement.If, sb);
        ComposeExpression(ifStatement.Expression, sb);
        ComposeBody(ifStatement.Body, sb);
    }

    private void ComposeIfElseStatement(IfElseStatementSyntax ifElseStatement, StringBuilder sb)
    {
        ComposeSyntaxToken(ifElseStatement.If, sb);
        ComposeExpression(ifElseStatement.Expression, sb);
        ComposeBody(ifElseStatement.Body, sb);
        ComposeElse(ifElseStatement.Else, sb);
    }

    private void ComposeElse(ElseSyntax elseSyntax, StringBuilder sb)
    {
        switch (elseSyntax)
        {
            case ElseBodySyntax elseBody:
                ComposeElseBody(elseBody, sb);
                break;

            case ElseIfBodySyntax elseIfBody:
                ComposeElseIfBody(elseIfBody, sb);
                break;
        }
    }

    private void ComposeElseBody(ElseBodySyntax elseBody, StringBuilder sb)
    {
        ComposeSyntaxToken(elseBody.Else, sb);
        ComposeBody(elseBody.Body, sb);
    }

    private void ComposeElseIfBody(ElseIfBodySyntax elseBody, StringBuilder sb)
    {
        ComposeSyntaxToken(elseBody.Else, sb);

        switch (elseBody.If)
        {
            case IfStatementSyntax ifStatement:
                ComposeIfStatement(ifStatement, sb);
                break;

            case IfElseStatementSyntax ifElseStatement:
                ComposeIfElseStatement(ifElseStatement, sb);
                break;
        }
    }

    private void ComposeReturnStatement(ReturnStatementSyntax returnStatement, StringBuilder sb)
    {
        ComposeSyntaxToken(returnStatement.Return, sb);
        ComposeSyntaxToken(returnStatement.Semicolon, sb);
    }

    private void ComposeBreakStatement(BreakStatementSyntax breakStatement, StringBuilder sb)
    {
        ComposeSyntaxToken(breakStatement.Break, sb);
        ComposeSyntaxToken(breakStatement.Semicolon, sb);
    }

    private void ComposeMethodInvocationStatement(MethodInvocationStatementSyntax invocation, StringBuilder sb)
    {
        ComposeMethodInvocationExpression(invocation.Invocation, sb);
        ComposeSyntaxToken(invocation.Semicolon, sb);
    }

    private void ComposeExpression(ExpressionSyntax expression, StringBuilder sb)
    {
        switch (expression)
        {
            case LogicalExpressionSyntax logicalExpression:
                ComposeLogicalExpression(logicalExpression, sb);
                break;

            case UnaryExpressionSyntax unaryExpression:
                ComposeUnaryExpression(unaryExpression, sb);
                break;

            case MethodInvocationExpressionSyntax methodInvocation:
                ComposeMethodInvocationExpression(methodInvocation, sb);
                break;

            case LiteralExpressionSyntax literalExpression:
                ComposeLiteralExpression(literalExpression, sb);
                break;
        }
    }

    private void ComposeLogicalExpression(LogicalExpressionSyntax logicalExpression, StringBuilder sb)
    {
        ComposeExpression(logicalExpression.Left, sb);
        ComposeSyntaxToken(logicalExpression.Operation, sb);
        ComposeExpression(logicalExpression.Right, sb);
    }

    private void ComposeUnaryExpression(UnaryExpressionSyntax unaryExpression, StringBuilder sb)
    {
        ComposeSyntaxToken(unaryExpression.Operation, sb);
        ComposeExpression(unaryExpression.Value, sb);
    }

    private void ComposeMethodInvocationExpression(MethodInvocationExpressionSyntax invocation, StringBuilder sb)
    {
        ComposeSyntaxToken(invocation.Identifier, sb);
        ComposeMethodInvocationParameters(invocation.Parameters, sb);
    }

    private void ComposeMethodInvocationParameters(MethodInvocationParametersSyntax invocationParameters, StringBuilder sb)
    {
        ComposeSyntaxToken(invocationParameters.ParenOpen, sb);
        ComposeLiteralExpressions(invocationParameters.ParameterList, sb);
        ComposeSyntaxToken(invocationParameters.ParenClose, sb);
    }

    private void ComposeLiteralExpressions(CommaSeparatedSyntaxList<LiteralExpressionSyntax>? valueList, StringBuilder sb)
    {
        if (valueList == null || valueList.Elements.Count <= 0)
            return;

        for (var i = 0; i < valueList.Elements.Count - 1; i++)
        {
            ComposeLiteralExpression(valueList.Elements[i], sb);
            ComposeSyntaxToken(_syntaxFactory.Token(SyntaxTokenKind.Comma), sb);
        }

        ComposeLiteralExpression(valueList.Elements[^1], sb);
    }

    private void ComposeLiteralExpression(LiteralExpressionSyntax literal, StringBuilder sb)
    {
        ComposeSyntaxToken(literal.Literal, sb);
    }

    private void ComposeSyntaxToken(SyntaxToken token, StringBuilder sb)
    {
        if (token.LeadingTrivia.HasValue)
            sb.Append(token.LeadingTrivia.Value.Text);

        sb.Append(token.Text);

        if (token.TrailingTrivia.HasValue)
            sb.Append(token.TrailingTrivia.Value.Text);
    }
}