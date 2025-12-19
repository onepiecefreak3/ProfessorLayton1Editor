using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Layton1;
using Logic.Domain.CodeAnalysisManagement.Contract.Level5;
using Logic.Domain.CodeAnalysisManagement.Layton1.InternalContract.DataClasses;

namespace Logic.Domain.CodeAnalysisManagement.Layton1;

internal class Layton1ScriptWhitespaceNormalizer : ILayton1ScriptWhitespaceNormalizer
{
    public void NormalizeCodeUnit(CodeUnitSyntax codeUnit)
    {
        var ctx = new WhitespaceNormalizeContext();
        NormalizeCodeUnit(codeUnit, ctx);

        codeUnit.Update();
    }

    private void NormalizeCodeUnit(CodeUnitSyntax codeUnit, WhitespaceNormalizeContext ctx)
    {
        if (codeUnit.MethodDeclaration is null)
            return;

        ctx.IsFirstElement = true;
        ctx.ShouldLineBreak = false;
        NormalizeMethodDeclaration(codeUnit.MethodDeclaration, ctx);
    }

    private void NormalizeMethodDeclaration(MethodDeclarationSyntax methodDeclaration, WhitespaceNormalizeContext ctx)
    {
        bool shouldLineBreak = ctx.ShouldLineBreak;

        SyntaxToken newIdentifier = methodDeclaration.Identifier.WithNoTrivia();

        methodDeclaration.SetIdentifier(newIdentifier, false);

        ctx.ShouldLineBreak = true;
        NormalizeMethodDeclarationParameters(methodDeclaration.Parameters, ctx);

        ctx.ShouldLineBreak = shouldLineBreak;
        NormalizeBodySyntax(methodDeclaration.Body, ctx);
    }

    private void NormalizeMethodDeclarationParameters(MethodDeclarationParametersSyntax methodDeclarationParameters, WhitespaceNormalizeContext ctx)
    {
        SyntaxToken newParenOpen = methodDeclarationParameters.ParenOpen.WithLeadingTrivia(null).WithLeadingTrivia(null);
        SyntaxToken newParenClose = methodDeclarationParameters.ParenClose.WithLeadingTrivia(null).WithLeadingTrivia(null);

        if (ctx.ShouldLineBreak)
            newParenClose = newParenClose.WithTrailingTrivia("\r\n");

        methodDeclarationParameters.SetParenOpen(newParenOpen, false);
        methodDeclarationParameters.SetParenClose(newParenClose, false);
    }

    private void NormalizeBodySyntax(BodySyntax bodySyntax, WhitespaceNormalizeContext ctx)
    {
        SyntaxToken newCurlyOpen = bodySyntax.CurlyOpen.WithLeadingTrivia(null).WithTrailingTrivia("\r\n");
        SyntaxToken newCurlyClose = bodySyntax.CurlyClose.WithNoTrivia();

        if (ctx.ShouldLineBreak)
            newCurlyClose = newCurlyClose.WithTrailingTrivia("\r\n");

        var whitespace = string.Empty;
        if (ctx is { ShouldIndent: true, Indent: > 0 })
            whitespace += new string('\t', ctx.Indent);

        if (!string.IsNullOrEmpty(whitespace))
        {
            newCurlyOpen = newCurlyOpen.WithLeadingTrivia(whitespace);
            newCurlyClose = newCurlyClose.WithLeadingTrivia(whitespace);
        }

        bodySyntax.SetCurlyOpen(newCurlyOpen, false);
        bodySyntax.SetCurlyClose(newCurlyClose, false);

        ctx.Indent++;
        foreach (StatementSyntax expression in bodySyntax.Statements)
        {
            ctx.IsFirstElement = bodySyntax.Statements[0] == expression;
            ctx.ShouldLineBreak = true;
            ctx.ShouldIndent = true;

            NormalizeStatement(expression, ctx);
        }
    }

    private void NormalizeStatement(StatementSyntax statement, WhitespaceNormalizeContext ctx)
    {
        switch (statement)
        {
            case ReturnStatementSyntax returnStatement:
                NormalizeReturnStatement(returnStatement, ctx);
                break;

            case BreakStatementSyntax breakStatement:
                NormalizeBreakStatement(breakStatement, ctx);
                break;

            case IfStatementSyntax ifStatement:
                NormalizeIfStatement(ifStatement, ctx);
                break;

            case WhileStatementSyntax whileStatement:
                NormalizeWhileStatement(whileStatement, ctx);
                break;

            case IfElseStatementSyntax ifElseStatement:
                NormalizeIfElseStatement(ifElseStatement, ctx);
                break;

            case MethodInvocationStatementSyntax methodInvocationStatement:
                NormalizeMethodInvocationStatement(methodInvocationStatement, ctx);
                break;
        }
    }

    private void NormalizeWhileStatement(WhileStatementSyntax whileStatement, WhitespaceNormalizeContext ctx)
    {
        SyntaxToken newIf = whileStatement.While.WithTrailingTrivia(null).WithTrailingTrivia(" ");

        if (ctx is { ShouldIndent: true, Indent: > 0 })
            newIf = newIf.WithLeadingTrivia(new string('\t', ctx.Indent));

        whileStatement.SetWhile(newIf, false);

        ctx.ShouldIndent = true;
        NormalizeConditionalBody(whileStatement.Body, ctx);

        ctx.ShouldIndent = false;
        ctx.ShouldLineBreak = false;
        ctx.IsFirstElement = true;
        NormalizeExpression(whileStatement.Expression, ctx);
    }

    private void NormalizeIfStatement(IfStatementSyntax ifStatement, WhitespaceNormalizeContext ctx)
    {
        SyntaxToken newIf = ifStatement.If.WithTrailingTrivia(null).WithTrailingTrivia(" ");

        if (ctx is { ShouldIndent: true, Indent: > 0 })
            newIf = newIf.WithLeadingTrivia(new string('\t', ctx.Indent));

        ifStatement.SetIf(newIf, false);

        ctx.ShouldIndent = true;
        NormalizeConditionalBody(ifStatement.Body, ctx);

        ctx.ShouldIndent = false;
        ctx.ShouldLineBreak = false;
        ctx.IsFirstElement = true;
        NormalizeExpression(ifStatement.Expression, ctx);
    }

    private void NormalizeConditionalBody(BodySyntax body, WhitespaceNormalizeContext ctx)
    {
        SyntaxToken newOpen = body.CurlyOpen.WithNoTrivia();
        SyntaxToken newClose = body.CurlyClose.WithNoTrivia();

        var whitespace = string.Empty;
        if (ctx is { ShouldIndent: true, Indent: > 0 })
            whitespace += new string('\t', ctx.Indent);

        if (!string.IsNullOrEmpty(whitespace))
        {
            newOpen = newOpen.WithLeadingTrivia("\r\n" + whitespace).WithTrailingTrivia("\r\n");
            newClose = newClose.WithLeadingTrivia(whitespace).WithTrailingTrivia("\r\n");
        }

        body.SetCurlyOpen(newOpen, false);
        body.SetCurlyClose(newClose, false);

        ctx.Indent++;
        foreach (StatementSyntax expression in body.Statements)
        {
            ctx.IsFirstElement = body.Statements[0] == expression;
            ctx.ShouldLineBreak = true;
            ctx.ShouldIndent = true;

            NormalizeStatement(expression, ctx);
        }
    }

    private void NormalizeIfElseStatement(IfElseStatementSyntax ifElseStatement, WhitespaceNormalizeContext ctx)
    {
        SyntaxToken newIf = ifElseStatement.If.WithTrailingTrivia(null).WithTrailingTrivia(" ");

        if (ctx is { ShouldIndent: true, Indent: > 0 })
            newIf = newIf.WithLeadingTrivia(new string('\t', ctx.Indent));

        ifElseStatement.SetIf(newIf, false);

        ctx.ShouldIndent = true;
        NormalizeConditionalBody(ifElseStatement.Body, ctx);
        NormalizeElse(ifElseStatement.Else, ctx);

        ctx.ShouldIndent = false;
        ctx.ShouldLineBreak = false;
        ctx.IsFirstElement = true;
        NormalizeExpression(ifElseStatement.Expression, ctx);
    }

    private void NormalizeElse(ElseSyntax elseSyntax, WhitespaceNormalizeContext ctx)
    {
        switch (elseSyntax)
        {
            case ElseBodySyntax elseBody:
                NormalizeElseBody(elseBody, ctx);
                break;

            case ElseIfBodySyntax elseIfBody:
                NormalizeElseIfBody(elseIfBody, ctx);
                break;
        }
    }

    private void NormalizeElseBody(ElseBodySyntax elseBody, WhitespaceNormalizeContext ctx)
    {
        SyntaxToken newElse = elseBody.Else.WithNoTrivia();

        var whitespace = string.Empty;
        if (ctx is { ShouldIndent: true, Indent: > 0 })
            whitespace += new string('\t', ctx.Indent);

        if (!string.IsNullOrEmpty(whitespace))
            newElse = newElse.WithLeadingTrivia(whitespace).WithTrailingTrivia("\r\n");

        elseBody.SetElse(newElse, false);

        ctx.ShouldIndent = true;
        NormalizeBodySyntax(elseBody.Body, ctx);
    }

    private void NormalizeElseIfBody(ElseIfBodySyntax elseIfBody, WhitespaceNormalizeContext ctx)
    {
        SyntaxToken newElse = elseIfBody.Else.WithLeadingTrivia(null).WithTrailingTrivia(" ");

        var whitespace = string.Empty;
        if (ctx is { ShouldIndent: true, Indent: > 0 })
            whitespace += new string('\t', ctx.Indent);

        if (!string.IsNullOrEmpty(whitespace))
            newElse = newElse.WithLeadingTrivia(whitespace);

        elseIfBody.SetElse(newElse, false);

        switch (elseIfBody.If)
        {
            case IfStatementSyntax ifStatement:
                ctx.ShouldIndent = false;
                NormalizeIfStatement(ifStatement, ctx);
                break;

            case IfElseStatementSyntax ifElseStatement:
                ctx.ShouldIndent = false;
                NormalizeIfElseStatement(ifElseStatement, ctx);
                break;
        }
    }

    private void NormalizeReturnStatement(ReturnStatementSyntax returnStatement, WhitespaceNormalizeContext ctx)
    {
        SyntaxToken newReturnKeyword = returnStatement.Return.WithNoTrivia();
        SyntaxToken newSemicolon = returnStatement.Semicolon.WithNoTrivia();

        if (ctx is { ShouldIndent: true, Indent: > 0 })
            newReturnKeyword = newReturnKeyword.WithLeadingTrivia(new string('\t', ctx.Indent));

        if (ctx.ShouldLineBreak)
            newSemicolon = newSemicolon.WithTrailingTrivia("\r\n");

        returnStatement.SetReturn(newReturnKeyword, false);
        returnStatement.SetSemicolon(newSemicolon, false);
    }

    private void NormalizeBreakStatement(BreakStatementSyntax breakStatement, WhitespaceNormalizeContext ctx)
    {
        SyntaxToken newBreakKeyword = breakStatement.Break.WithNoTrivia();
        SyntaxToken newSemicolon = breakStatement.Semicolon.WithNoTrivia();

        if (ctx is { ShouldIndent: true, Indent: > 0 })
            newBreakKeyword = newBreakKeyword.WithLeadingTrivia(new string('\t', ctx.Indent));

        if (ctx.ShouldLineBreak)
            newSemicolon = newSemicolon.WithTrailingTrivia("\r\n");

        breakStatement.SetBreak(newBreakKeyword, false);
        breakStatement.SetSemicolon(newSemicolon, false);
    }

    private void NormalizeExpression(ExpressionSyntax expression, WhitespaceNormalizeContext ctx)
    {
        switch (expression)
        {
            case LogicalExpressionSyntax logicalExpression:
                ctx.IsFirstElement = true;
                NormalizeLogicalExpression(logicalExpression, ctx);
                break;

            case UnaryExpressionSyntax rightUnaryExpression:
                ctx.IsFirstElement = true;
                NormalizeUnaryExpression(rightUnaryExpression, ctx);
                break;

            case MethodInvocationExpressionSyntax rightMethodInvocation:
                NormalizeMethodInvocationExpression(rightMethodInvocation, ctx);
                break;

            case LiteralExpressionSyntax literalExpression:
                NormalizeLiteralExpression(literalExpression, ctx);
                break;

            default:
                throw new InvalidOperationException("Unknown expression.");
        }
    }

    private void NormalizeUnaryExpression(UnaryExpressionSyntax unaryExpression, WhitespaceNormalizeContext ctx)
    {
        SyntaxToken operation = unaryExpression.Operation.WithNoTrivia();

        ctx.IsFirstElement = unaryExpression.Operation.RawKind != (int)SyntaxTokenKind.NotKeyword;
        NormalizeExpression(unaryExpression.Value, ctx);

        unaryExpression.SetOperation(operation, false);
    }

    private void NormalizeLogicalExpression(LogicalExpressionSyntax logicalExpression, WhitespaceNormalizeContext ctx)
    {
        NormalizeExpression(logicalExpression.Left, ctx);

        SyntaxToken operation = logicalExpression.Operation.WithLeadingTrivia(" ").WithTrailingTrivia(" ");

        ctx.IsFirstElement = true;
        NormalizeExpression(logicalExpression.Right, ctx);

        logicalExpression.SetOperation(operation, false);
    }

    private void NormalizeMethodInvocationExpression(MethodInvocationExpressionSyntax invocation, WhitespaceNormalizeContext ctx)
    {
        SyntaxToken newIdentifier = invocation.Identifier.WithNoTrivia();

        string? leadingTrivia = null;
        if (ctx is { ShouldIndent: true, Indent: > 0 })
            leadingTrivia = new string('\t', ctx.Indent);
        else if (!ctx.IsFirstElement)
            leadingTrivia = " ";

        newIdentifier.WithLeadingTrivia(leadingTrivia);
        invocation.SetIdentifier(newIdentifier, false);

        ctx.ShouldIndent = false;
        ctx.ShouldLineBreak = false;
        NormalizeMethodInvocationParameters(invocation.Parameters, ctx);
    }

    private void NormalizeMethodInvocationStatement(MethodInvocationStatementSyntax invocation, WhitespaceNormalizeContext ctx)
    {
        NormalizeMethodInvocationExpression(invocation.Invocation, ctx);

        SyntaxToken newSemicolon = invocation.Semicolon.WithNoTrivia();

        if (ctx.ShouldLineBreak)
            newSemicolon = newSemicolon.WithTrailingTrivia("\r\n");

        invocation.SetSemicolon(newSemicolon, false);
    }

    private void NormalizeMethodInvocationParameters(MethodInvocationParametersSyntax invocationParameters, WhitespaceNormalizeContext ctx)
    {
        SyntaxToken parenOpen = invocationParameters.ParenOpen.WithNoTrivia();
        SyntaxToken parenClose = invocationParameters.ParenClose.WithNoTrivia();

        invocationParameters.SetParenOpen(parenOpen, false);
        invocationParameters.SetParenClose(parenClose, false);

        NormalizeValueExpressions(invocationParameters.ParameterList, ctx);
    }

    private void NormalizeValueExpressions(CommaSeparatedSyntaxList<LiteralExpressionSyntax>? valueList, WhitespaceNormalizeContext ctx)
    {
        if (valueList == null)
            return;

        foreach (LiteralExpressionSyntax value in valueList.Elements)
        {
            ctx.IsFirstElement = valueList.Elements[0] == value;
            NormalizeLiteralExpression(value, ctx);
        }
    }

    private void NormalizeLiteralExpression(LiteralExpressionSyntax literal, WhitespaceNormalizeContext ctx)
    {
        SyntaxToken literalToken = literal.Literal.WithNoTrivia();

        string? leadingTrivia = null;
        if (ctx is { ShouldIndent: true, Indent: > 0 })
            leadingTrivia = new string('\t', ctx.Indent);
        if (!ctx.IsFirstElement)
            leadingTrivia += " ";

        literalToken = literalToken.WithLeadingTrivia(leadingTrivia);
        if (ctx.ShouldLineBreak)
            literalToken = literalToken.WithTrailingTrivia("\r\n");

        literal.SetLiteral(literalToken, false);
    }
}