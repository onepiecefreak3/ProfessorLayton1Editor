using Logic.Domain.CodeAnalysisManagement.Contract;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Layton1;
using Logic.Domain.CodeAnalysisManagement.Contract.Exceptions.Level5;
using Logic.Domain.CodeAnalysisManagement.Contract.Level5;
using Logic.Domain.CodeAnalysisManagement.Layton1.InternalContract.DataClasses;

namespace Logic.Domain.CodeAnalysisManagement.Layton1;

internal class Layton1ScriptParser : ILayton1ScriptParser
{
    private readonly ITokenFactory<Layton1SyntaxToken> _scriptFactory;
    private readonly ILayton1SyntaxFactory _syntaxFactory;

    public Layton1ScriptParser(ITokenFactory<Layton1SyntaxToken> scriptFactory, ILayton1SyntaxFactory syntaxFactory)
    {
        _scriptFactory = scriptFactory;
        _syntaxFactory = syntaxFactory;
    }

    public CodeUnitSyntax ParseCodeUnit(string text)
    {
        IBuffer<Layton1SyntaxToken> buffer = CreateTokenBuffer(text);

        return ParseCodeUnit(buffer);
    }

    private CodeUnitSyntax ParseCodeUnit(IBuffer<Layton1SyntaxToken> buffer)
    {
        var methodDeclarations = ParseMethodDeclaration(buffer);

        return new CodeUnitSyntax(methodDeclarations);
    }

    private MethodDeclarationSyntax ParseMethodDeclaration(IBuffer<Layton1SyntaxToken> buffer)
    {
        SyntaxToken identifier = ParseIdentifierToken(buffer);
        var parameters = ParseMethodDeclarationParameters(buffer);
        var body = ParseBody(buffer);

        return new MethodDeclarationSyntax(identifier, parameters, body);
    }

    private MethodDeclarationParametersSyntax ParseMethodDeclarationParameters(IBuffer<Layton1SyntaxToken> buffer)
    {
        SyntaxToken parenOpenToken = ParseParenOpenToken(buffer);
        SyntaxToken parenCloseToken = ParseParenCloseToken(buffer);

        return new MethodDeclarationParametersSyntax(parenOpenToken, parenCloseToken);
    }

    private BodySyntax ParseBody(IBuffer<Layton1SyntaxToken> buffer)
    {
        SyntaxToken curlyOpenToken = ParseCurlyOpenToken(buffer);
        var expressions = ParseStatements(buffer);
        SyntaxToken curlyCloseToken = ParseCurlyCloseToken(buffer);

        return new BodySyntax(curlyOpenToken, expressions, curlyCloseToken);
    }

    private IReadOnlyList<StatementSyntax> ParseStatements(IBuffer<Layton1SyntaxToken> buffer)
    {
        var result = new List<StatementSyntax>();

        while (IsStatement(buffer))
            result.Add(ParseStatement(buffer));

        return result;
    }

    private bool IsStatement(IBuffer<Layton1SyntaxToken> buffer)
    {
        return HasTokenKind(buffer, SyntaxTokenKind.StringLiteral) ||
               HasTokenKind(buffer, SyntaxTokenKind.ReturnKeyword) ||
               HasTokenKind(buffer, SyntaxTokenKind.BreakKeyword) ||
               HasTokenKind(buffer, SyntaxTokenKind.WhileKeyword) ||
               HasTokenKind(buffer, SyntaxTokenKind.IfKeyword) ||
               IsMethodInvocation(buffer);
    }

    private bool IsMethodInvocation(IBuffer<Layton1SyntaxToken> buffer)
    {
        return HasTokenKind(buffer, SyntaxTokenKind.Identifier) &&
               HasTokenKind(buffer, 1, SyntaxTokenKind.ParenOpen);
    }

    private StatementSyntax ParseStatement(IBuffer<Layton1SyntaxToken> buffer)
    {
        if (HasTokenKind(buffer, SyntaxTokenKind.ReturnKeyword))
            return ParseReturnStatement(buffer);

        if (HasTokenKind(buffer, SyntaxTokenKind.BreakKeyword))
            return ParseBreakStatement(buffer);

        if (HasTokenKind(buffer, SyntaxTokenKind.WhileKeyword))
            return ParseWhileStatement(buffer);

        if (HasTokenKind(buffer, SyntaxTokenKind.IfKeyword))
            return ParseIfElseStatement(buffer);

        if (IsMethodInvocation(buffer))
            return ParseMethodInvocationStatement(buffer);

        throw CreateException(buffer, "Unknown statement.", SyntaxTokenKind.ReturnKeyword, SyntaxTokenKind.StringLiteral,
            SyntaxTokenKind.IfKeyword, SyntaxTokenKind.Identifier);
    }

    private WhileStatementSyntax ParseWhileStatement(IBuffer<Layton1SyntaxToken> buffer)
    {
        SyntaxToken ifToken = ParseWhileKeywordToken(buffer);
        ExpressionSyntax conditional = ParseExpression(buffer);
        BodySyntax body = ParseBody(buffer);

        return new WhileStatementSyntax(ifToken, conditional, body);
    }

    private StatementSyntax ParseIfElseStatement(IBuffer<Layton1SyntaxToken> buffer)
    {
        SyntaxToken ifToken = ParseIfKeywordToken(buffer);
        ExpressionSyntax conditional = ParseExpression(buffer);
        BodySyntax body = ParseBody(buffer);

        if (!HasTokenKind(buffer, SyntaxTokenKind.ElseKeyword))
            return new IfStatementSyntax(ifToken, conditional, body);

        ElseSyntax elseSyntax = ParseElse(buffer);

        return new IfElseStatementSyntax(ifToken, conditional, body, elseSyntax);
    }

    private ElseSyntax ParseElse(IBuffer<Layton1SyntaxToken> buffer)
    {
        SyntaxToken elseToken = ParseElseKeywordToken(buffer);

        if (HasTokenKind(buffer, SyntaxTokenKind.CurlyOpen))
            return ParseElseBody(elseToken, buffer);

        if (HasTokenKind(buffer, SyntaxTokenKind.IfKeyword))
            return ParseElseIfBody(elseToken, buffer);

        throw CreateException(buffer, "Unknown else statement.", SyntaxTokenKind.ParenOpen, SyntaxTokenKind.IfKeyword);
    }

    private ElseBodySyntax ParseElseBody(SyntaxToken elseToken, IBuffer<Layton1SyntaxToken> buffer)
    {
        BodySyntax body = ParseBody(buffer);

        return new ElseBodySyntax(elseToken, body);
    }

    private ElseIfBodySyntax ParseElseIfBody(SyntaxToken elseToken, IBuffer<Layton1SyntaxToken> buffer)
    {
        StatementSyntax ifExpression = ParseIfElseStatement(buffer);

        return new ElseIfBodySyntax(elseToken, ifExpression);
    }

    private ReturnStatementSyntax ParseReturnStatement(IBuffer<Layton1SyntaxToken> buffer)
    {
        SyntaxToken returnToken = ParseReturnKeywordToken(buffer);
        SyntaxToken semicolon = ParseSemicolonToken(buffer);

        return new ReturnStatementSyntax(returnToken, semicolon);
    }

    private BreakStatementSyntax ParseBreakStatement(IBuffer<Layton1SyntaxToken> buffer)
    {
        SyntaxToken breakToken = ParseBreakKeywordToken(buffer);
        SyntaxToken semicolon = ParseSemicolonToken(buffer);

        return new BreakStatementSyntax(breakToken, semicolon);
    }

    private ExpressionSyntax ParseExpression(IBuffer<Layton1SyntaxToken> buffer)
    {
        ExpressionSyntax left = ParseAtomicExpression(buffer);

        if (!IsLogicalExpression(buffer))
            return left;

        return ParseLogicalExpression(buffer, left);
    }

    private ExpressionSyntax ParseAtomicExpression(IBuffer<Layton1SyntaxToken> buffer)
    {
        if (IsUnaryExpression(buffer))
            return ParseUnaryExpression(buffer);

        if (IsMethodInvocation(buffer))
            return ParseMethodInvocationExpression(buffer);

        if (IsLiteralExpression(buffer))
            return ParseLiteralExpression(buffer);

        throw CreateException(buffer, "Invalid expression.", SyntaxTokenKind.NotKeyword, SyntaxTokenKind.StringLiteral,
            SyntaxTokenKind.NumericLiteral, SyntaxTokenKind.UnsignedNumericLiteral, SyntaxTokenKind.FloatingNumericLiteral,
            SyntaxTokenKind.HashStringLiteral, SyntaxTokenKind.HashNumericLiteral);
    }

    private bool IsLogicalExpression(IBuffer<Layton1SyntaxToken> buffer)
    {
        return HasTokenKind(buffer, SyntaxTokenKind.AndKeyword) ||
               HasTokenKind(buffer, SyntaxTokenKind.OrKeyword);
    }

    private LogicalExpressionSyntax ParseLogicalExpression(IBuffer<Layton1SyntaxToken> buffer, ExpressionSyntax left)
    {
        switch (buffer.Peek().Kind)
        {
            case SyntaxTokenKind.AndKeyword:
                return new LogicalExpressionSyntax(left, ParseAndKeywordToken(buffer), ParseExpression(buffer));

            case SyntaxTokenKind.OrKeyword:
                return new LogicalExpressionSyntax(left, ParseOrKeywordToken(buffer), ParseExpression(buffer));

            default:
                throw CreateException(buffer, "Unknown logical expression.", SyntaxTokenKind.AndKeyword, SyntaxTokenKind.OrKeyword);
        }
    }

    private bool IsUnaryExpression(IBuffer<Layton1SyntaxToken> buffer)
    {
        return HasTokenKind(buffer, SyntaxTokenKind.NotKeyword);
    }

    private UnaryExpressionSyntax ParseUnaryExpression(IBuffer<Layton1SyntaxToken> buffer)
    {
        switch (buffer.Peek().Kind)
        {
            case SyntaxTokenKind.NotKeyword:
                return new UnaryExpressionSyntax(ParseNotKeywordToken(buffer), ParseExpression(buffer));

            default:
                throw CreateException(buffer, "Unknown unary expression.", SyntaxTokenKind.NotKeyword);
        }
    }

    private MethodInvocationExpressionSyntax ParseMethodInvocationExpression(IBuffer<Layton1SyntaxToken> buffer)
    {
        SyntaxToken identifier = ParseIdentifierToken(buffer);
        var methodInvocationParameters = ParseMethodInvocationParameters(buffer);

        return new MethodInvocationExpressionSyntax(identifier, methodInvocationParameters);
    }

    private MethodInvocationStatementSyntax ParseMethodInvocationStatement(IBuffer<Layton1SyntaxToken> buffer)
    {
        MethodInvocationExpressionSyntax invocation = ParseMethodInvocationExpression(buffer);
        SyntaxToken semicolon = ParseSemicolonToken(buffer);

        return new MethodInvocationStatementSyntax(invocation, semicolon);
    }

    private MethodInvocationParametersSyntax ParseMethodInvocationParameters(IBuffer<Layton1SyntaxToken> buffer)
    {
        SyntaxToken parenOpen = ParseParenOpenToken(buffer);
        var parameters = ParseValueList(buffer);
        SyntaxToken parenClose = ParseParenCloseToken(buffer);

        return new MethodInvocationParametersSyntax(parenOpen, parameters, parenClose);
    }

    private CommaSeparatedSyntaxList<LiteralExpressionSyntax>? ParseValueList(IBuffer<Layton1SyntaxToken> buffer)
    {
        if (!IsLiteralExpression(buffer))
            return null;

        var result = new List<LiteralExpressionSyntax>();

        LiteralExpressionSyntax parameter = ParseLiteralExpression(buffer);
        result.Add(parameter);

        while (HasTokenKind(buffer, SyntaxTokenKind.Comma))
        {
            SkipTokenKind(buffer, SyntaxTokenKind.Comma);

            if (!IsLiteralExpression(buffer))
                throw CreateException(buffer, "Invalid end of parameter list.", SyntaxTokenKind.StringLiteral,
                    SyntaxTokenKind.NumericLiteral, SyntaxTokenKind.UnsignedNumericLiteral,
                    SyntaxTokenKind.HashNumericLiteral, SyntaxTokenKind.HashStringLiteral,
                    SyntaxTokenKind.FloatingNumericLiteral);

            parameter = ParseLiteralExpression(buffer);
            result.Add(parameter);
        }

        return new CommaSeparatedSyntaxList<LiteralExpressionSyntax>(result);
    }

    private bool IsLiteralExpression(IBuffer<Layton1SyntaxToken> buffer)
    {
        return HasTokenKind(buffer, SyntaxTokenKind.TrueKeyword) ||
               HasTokenKind(buffer, SyntaxTokenKind.FalseKeyword) || 
               HasTokenKind(buffer, SyntaxTokenKind.StringLiteral) ||
               HasTokenKind(buffer, SyntaxTokenKind.NumericLiteral) ||
               HasTokenKind(buffer, SyntaxTokenKind.UnsignedNumericLiteral) ||
               HasTokenKind(buffer, SyntaxTokenKind.HashStringLiteral) ||
               HasTokenKind(buffer, SyntaxTokenKind.HashNumericLiteral) ||
               HasTokenKind(buffer, SyntaxTokenKind.FloatingNumericLiteral);
    }

    private LiteralExpressionSyntax ParseLiteralExpression(IBuffer<Layton1SyntaxToken> buffer)
    {
        if (HasTokenKind(buffer, SyntaxTokenKind.TrueKeyword))
            return ParseTrueLiteralExpression(buffer);

        if (HasTokenKind(buffer, SyntaxTokenKind.FalseKeyword))
            return ParseFalseLiteralExpression(buffer);

        if (HasTokenKind(buffer, SyntaxTokenKind.StringLiteral))
            return ParseStringLiteralExpression(buffer);

        if (HasTokenKind(buffer, SyntaxTokenKind.NumericLiteral))
            return ParseNumericLiteralExpression(buffer);

        if (HasTokenKind(buffer, SyntaxTokenKind.UnsignedNumericLiteral))
            return ParseUnsignedNumericLiteralExpression(buffer);

        if (HasTokenKind(buffer, SyntaxTokenKind.HashNumericLiteral))
            return ParseHashNumericLiteralExpression(buffer);

        if (HasTokenKind(buffer, SyntaxTokenKind.HashStringLiteral))
            return ParseHashStringLiteralExpression(buffer);

        if (HasTokenKind(buffer, SyntaxTokenKind.FloatingNumericLiteral))
            return ParseFloatingNumericLiteralExpression(buffer);

        throw CreateException(buffer, "Unknown value expression.", SyntaxTokenKind.StringLiteral, SyntaxTokenKind.NumericLiteral,
            SyntaxTokenKind.UnsignedNumericLiteral, SyntaxTokenKind.FloatingNumericLiteral,
            SyntaxTokenKind.HashNumericLiteral, SyntaxTokenKind.HashStringLiteral);
    }

    private LiteralExpressionSyntax ParseTrueLiteralExpression(IBuffer<Layton1SyntaxToken> buffer)
    {
        SyntaxToken literal = ParseTrueKeywordToken(buffer);

        return new LiteralExpressionSyntax(literal);
    }

    private LiteralExpressionSyntax ParseFalseLiteralExpression(IBuffer<Layton1SyntaxToken> buffer)
    {
        SyntaxToken literal = ParseFalseKeywordToken(buffer);

        return new LiteralExpressionSyntax(literal);
    }

    private LiteralExpressionSyntax ParseStringLiteralExpression(IBuffer<Layton1SyntaxToken> buffer)
    {
        SyntaxToken literal = ParseStringLiteralToken(buffer);

        return new LiteralExpressionSyntax(literal);
    }

    private LiteralExpressionSyntax ParseNumericLiteralExpression(IBuffer<Layton1SyntaxToken> buffer)
    {
        SyntaxToken literal = ParseNumericLiteralToken(buffer);

        return new LiteralExpressionSyntax(literal);
    }

    private LiteralExpressionSyntax ParseUnsignedNumericLiteralExpression(IBuffer<Layton1SyntaxToken> buffer)
    {
        SyntaxToken literal = ParseUnsignedNumericLiteralToken(buffer);

        return new LiteralExpressionSyntax(literal);
    }

    private LiteralExpressionSyntax ParseHashNumericLiteralExpression(IBuffer<Layton1SyntaxToken> buffer)
    {
        SyntaxToken literal = ParseHashNumericLiteralToken(buffer);

        return new LiteralExpressionSyntax(literal);
    }

    private LiteralExpressionSyntax ParseHashStringLiteralExpression(IBuffer<Layton1SyntaxToken> buffer)
    {
        SyntaxToken literal = ParseHashStringLiteralToken(buffer);

        return new LiteralExpressionSyntax(literal);
    }

    private LiteralExpressionSyntax ParseFloatingNumericLiteralExpression(IBuffer<Layton1SyntaxToken> buffer)
    {
        SyntaxToken literal = ParseFloatingNumericLiteralToken(buffer);

        return new LiteralExpressionSyntax(literal);
    }

    private SyntaxToken ParseSemicolonToken(IBuffer<Layton1SyntaxToken> buffer)
    {
        return CreateToken(buffer, SyntaxTokenKind.Semicolon);
    }

    private SyntaxToken ParseParenOpenToken(IBuffer<Layton1SyntaxToken> buffer)
    {
        return CreateToken(buffer, SyntaxTokenKind.ParenOpen);
    }

    private SyntaxToken ParseParenCloseToken(IBuffer<Layton1SyntaxToken> buffer)
    {
        return CreateToken(buffer, SyntaxTokenKind.ParenClose);
    }

    private SyntaxToken ParseCurlyOpenToken(IBuffer<Layton1SyntaxToken> buffer)
    {
        return CreateToken(buffer, SyntaxTokenKind.CurlyOpen);
    }

    private SyntaxToken ParseCurlyCloseToken(IBuffer<Layton1SyntaxToken> buffer)
    {
        return CreateToken(buffer, SyntaxTokenKind.CurlyClose);
    }

    private SyntaxToken ParseReturnKeywordToken(IBuffer<Layton1SyntaxToken> buffer)
    {
        return CreateToken(buffer, SyntaxTokenKind.ReturnKeyword);
    }

    private SyntaxToken ParseBreakKeywordToken(IBuffer<Layton1SyntaxToken> buffer)
    {
        return CreateToken(buffer, SyntaxTokenKind.BreakKeyword);
    }

    private SyntaxToken ParseNotKeywordToken(IBuffer<Layton1SyntaxToken> buffer)
    {
        return CreateToken(buffer, SyntaxTokenKind.NotKeyword);
    }

    private SyntaxToken ParseAndKeywordToken(IBuffer<Layton1SyntaxToken> buffer)
    {
        return CreateToken(buffer, SyntaxTokenKind.AndKeyword);
    }

    private SyntaxToken ParseOrKeywordToken(IBuffer<Layton1SyntaxToken> buffer)
    {
        return CreateToken(buffer, SyntaxTokenKind.OrKeyword);
    }

    private SyntaxToken ParseWhileKeywordToken(IBuffer<Layton1SyntaxToken> buffer)
    {
        return CreateToken(buffer, SyntaxTokenKind.WhileKeyword);
    }

    private SyntaxToken ParseIfKeywordToken(IBuffer<Layton1SyntaxToken> buffer)
    {
        return CreateToken(buffer, SyntaxTokenKind.IfKeyword);
    }

    private SyntaxToken ParseElseKeywordToken(IBuffer<Layton1SyntaxToken> buffer)
    {
        return CreateToken(buffer, SyntaxTokenKind.ElseKeyword);
    }

    private SyntaxToken ParseTrueKeywordToken(IBuffer<Layton1SyntaxToken> buffer)
    {
        return CreateToken(buffer, SyntaxTokenKind.TrueKeyword);
    }

    private SyntaxToken ParseFalseKeywordToken(IBuffer<Layton1SyntaxToken> buffer)
    {
        return CreateToken(buffer, SyntaxTokenKind.FalseKeyword);
    }

    private SyntaxToken ParseNumericLiteralToken(IBuffer<Layton1SyntaxToken> buffer)
    {
        return CreateToken(buffer, SyntaxTokenKind.NumericLiteral);
    }

    private SyntaxToken ParseUnsignedNumericLiteralToken(IBuffer<Layton1SyntaxToken> buffer)
    {
        return CreateToken(buffer, SyntaxTokenKind.UnsignedNumericLiteral);
    }

    private SyntaxToken ParseHashNumericLiteralToken(IBuffer<Layton1SyntaxToken> buffer)
    {
        return CreateToken(buffer, SyntaxTokenKind.HashNumericLiteral);
    }

    private SyntaxToken ParseHashStringLiteralToken(IBuffer<Layton1SyntaxToken> buffer)
    {
        return CreateToken(buffer, SyntaxTokenKind.HashStringLiteral);
    }

    private SyntaxToken ParseFloatingNumericLiteralToken(IBuffer<Layton1SyntaxToken> buffer)
    {
        return CreateToken(buffer, SyntaxTokenKind.FloatingNumericLiteral);
    }

    private SyntaxToken ParseStringLiteralToken(IBuffer<Layton1SyntaxToken> buffer)
    {
        return CreateToken(buffer, SyntaxTokenKind.StringLiteral);
    }

    private SyntaxToken ParseIdentifierToken(IBuffer<Layton1SyntaxToken> buffer)
    {
        return CreateToken(buffer, SyntaxTokenKind.Identifier);
    }

    private SyntaxToken CreateToken(IBuffer<Layton1SyntaxToken> buffer, SyntaxTokenKind expectedKind)
    {
        SyntaxTokenTrivia? leadingTrivia = ReadTrivia(buffer);

        if (buffer.Peek().Kind != expectedKind)
            throw CreateException(buffer, $"Unexpected token {buffer.Peek().Kind}.", expectedKind);
        Layton1SyntaxToken content = buffer.Read();

        SyntaxTokenTrivia? trailingTrivia = ReadTrivia(buffer);

        return _syntaxFactory.Create(content.Text, (int)expectedKind, leadingTrivia, trailingTrivia);
    }

    private SyntaxTokenTrivia? ReadTrivia(IBuffer<Layton1SyntaxToken> buffer)
    {
        if (buffer.Peek().Kind == SyntaxTokenKind.Trivia)
        {
            Layton1SyntaxToken token = buffer.Read();
            return new SyntaxTokenTrivia(token.Text);
        }

        return null;
    }

    private void SkipTokenKind(IBuffer<Layton1SyntaxToken> buffer, SyntaxTokenKind expectedKind)
    {
        int toSkip = 1;

        Layton1SyntaxToken peekedToken = buffer.Peek();
        if (peekedToken.Kind == SyntaxTokenKind.Trivia)
        {
            peekedToken = buffer.Peek(1);
            toSkip++;
        }

        if (peekedToken.Kind != expectedKind)
            throw CreateException(buffer, $"Unexpected token {peekedToken.Kind}.", expectedKind);

        for (var i = 0; i < toSkip; i++)
            buffer.Read();
    }

    protected bool HasTokenKind(IBuffer<Layton1SyntaxToken> buffer, SyntaxTokenKind expectedKind)
    {
        return HasTokenKind(buffer, 0, expectedKind);
    }

    protected bool HasTokenKind(IBuffer<Layton1SyntaxToken> buffer, int position, SyntaxTokenKind expectedKind)
    {
        var toPeek = 0;
        Layton1SyntaxToken peekedToken = buffer.Peek(toPeek);

        position = Math.Max(0, position);
        for (var i = 0; i < position + 1; i++)
        {
            peekedToken = buffer.Peek(toPeek++);
            if (peekedToken.Kind == SyntaxTokenKind.Trivia)
                peekedToken = buffer.Peek(toPeek++);
        }

        return peekedToken.Kind == expectedKind;
    }

    private (int, int) GetCurrentLineAndColumn(IBuffer<Layton1SyntaxToken> buffer)
    {
        var toPeek = 0;

        if (buffer.Peek().Kind == SyntaxTokenKind.Trivia)
            toPeek++;

        Layton1SyntaxToken token = buffer.Peek(toPeek);
        return (token.Line, token.Column);
    }

    private IBuffer<Layton1SyntaxToken> CreateTokenBuffer(string text)
    {
        ILexer<Layton1SyntaxToken> lexer = _scriptFactory.CreateLexer(text);
        return _scriptFactory.CreateTokenBuffer(lexer);
    }

    private Exception CreateException(IBuffer<Layton1SyntaxToken> buffer, string message, params SyntaxTokenKind[] expected)
    {
        (int line, int column) = GetCurrentLineAndColumn(buffer);
        return CreateException(message, line, column, expected);
    }

    private Exception CreateException(string message, int line, int column, params SyntaxTokenKind[] expected)
    {
        message = $"{message} (Line {line}, Column {column})";

        if (expected.Length > 0)
        {
            message = expected.Length == 1 ?
                $"{message} (Expected {expected[0]})" :
                $"{message} (Expected any of {string.Join(", ", expected)})";
        }

        throw new Level5ScriptParserException(message, line, column);
    }
}