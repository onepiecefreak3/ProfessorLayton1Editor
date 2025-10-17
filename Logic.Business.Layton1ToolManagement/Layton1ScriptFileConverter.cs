using Logic.Business.Layton1ToolManagement.InternalContract;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Level5;
using Logic.Domain.CodeAnalysisManagement.Contract.Level5;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;

namespace Logic.Business.Layton1ToolManagement;

internal class Layton1ScriptFileConverter(ILevel5SyntaxFactory syntaxFactory, ILayton1ScriptInstructionDescriptionProvider layton1ScriptInstructionProvider) : ILayton1ScriptFileConverter
{
    public CodeUnitSyntax CreateCodeUnit(GdsScriptFile script)
    {
        IReadOnlyList<MethodDeclarationSyntax> methods = CreateMethodDeclarations(script);

        return new CodeUnitSyntax(methods);
    }

    private IReadOnlyList<MethodDeclarationSyntax> CreateMethodDeclarations(GdsScriptFile script)
    {
        return [CreateMethodDeclaration(script)];
    }

    private MethodDeclarationSyntax CreateMethodDeclaration(GdsScriptFile script)
    {
        SyntaxToken identifier = syntaxFactory.Identifier("Main");
        var parameters = CreateMethodDeclarationParameters();
        var body = CreateMethodDeclarationBody(script);

        return new MethodDeclarationSyntax(identifier, null, parameters, body);
    }

    private MethodDeclarationParametersSyntax CreateMethodDeclarationParameters()
    {
        SyntaxToken parenOpen = syntaxFactory.Token(SyntaxTokenKind.ParenOpen);
        SyntaxToken parenClose = syntaxFactory.Token(SyntaxTokenKind.ParenClose);

        return new MethodDeclarationParametersSyntax(parenOpen, null, parenClose);
    }

    private MethodDeclarationBodySyntax CreateMethodDeclarationBody(GdsScriptFile script)
    {
        SyntaxToken curlyOpen = syntaxFactory.Token(SyntaxTokenKind.CurlyOpen);
        var expressions = CreateStatements(script);
        SyntaxToken curlyClose = syntaxFactory.Token(SyntaxTokenKind.CurlyClose);

        return new MethodDeclarationBodySyntax(curlyOpen, expressions, curlyClose);
    }

    private IReadOnlyList<StatementSyntax> CreateStatements(GdsScriptFile script)
    {
        var result = new List<StatementSyntax>();

        foreach (GdsScriptInstruction instruction in script.Instructions)
        {
            if (instruction.Jump is not null)
                result.Add(CreateGotoLabelStatement(instruction.Jump));

            result.Add(CreateStatement(instruction));
        }

        return result;
    }

    private GotoLabelStatementSyntax CreateGotoLabelStatement(GdsScriptJump jump)
    {
        var labelLiteral = CreateStringLiteralExpression(jump.Label);
        SyntaxToken colonToken = syntaxFactory.Token(SyntaxTokenKind.Colon);

        return new GotoLabelStatementSyntax(labelLiteral, colonToken);
    }

    private StatementSyntax CreateStatement(GdsScriptInstruction instruction)
    {
        switch (instruction.Type)
        {
            case 7:
                return CreateMethodInvocationExpression(syntaxFactory.Identifier("cmd7"), instruction);

            case 8:
                return CreateMethodInvocationExpression(syntaxFactory.Identifier("cmd8"), instruction);

            case 9:
                return CreateMethodInvocationExpression(syntaxFactory.Identifier("cmd9"), instruction);

            case 11:
                return CreateMethodInvocationExpression(syntaxFactory.Identifier("cmd11"), instruction);

            case 12:
                return CreateReturnStatement();

            default:
                return CreateMethodInvocationExpression(instruction);
        }
    }

    private ReturnStatementSyntax CreateReturnStatement()
    {
        SyntaxToken returnToken = syntaxFactory.Token(SyntaxTokenKind.ReturnKeyword);
        SyntaxToken semicolon = syntaxFactory.Token(SyntaxTokenKind.Semicolon);

        return new ReturnStatementSyntax(returnToken, null, semicolon);
    }

    private MethodInvocationStatementSyntax CreateMethodInvocationExpression(GdsScriptInstruction instruction)
    {
        SyntaxToken identifier = CreateMethodNameIdentifier(instruction);

        return CreateMethodInvocationExpression(identifier, instruction);
    }

    private MethodInvocationStatementSyntax CreateMethodInvocationExpression(SyntaxToken methodName, GdsScriptInstruction instruction)
    {
        var parameters = CreateMethodInvocationExpressionParameters(instruction);
        SyntaxToken semicolon = syntaxFactory.Token(SyntaxTokenKind.Semicolon);

        return new MethodInvocationStatementSyntax(methodName, null, parameters, semicolon);
    }

    private SyntaxToken CreateMethodNameIdentifier(GdsScriptInstruction instruction)
    {
        if (instruction.Arguments.Length < 1)
            throw new InvalidOperationException("Missing call type for instruction.");

        if (instruction.Arguments[0].Type is not GdsScriptArgumentType.Int)
            throw new InvalidOperationException("Wrong call type for instruction.");

        var instructionType = (int)instruction.Arguments[0].Value!;

        if (!layton1ScriptInstructionProvider.MapsInstructionType(instructionType))
            return syntaxFactory.Identifier($"sub{instructionType}");

        string methodName = layton1ScriptInstructionProvider.GetMethodName(instructionType);
        return syntaxFactory.Identifier(methodName);

    }

    private MethodInvocationParametersSyntax CreateMethodInvocationExpressionParameters(GdsScriptInstruction instruction)
    {
        SyntaxToken parenOpen = syntaxFactory.Token(SyntaxTokenKind.ParenOpen);
        var parameterList = CreateValueList(instruction);
        SyntaxToken parenClose = syntaxFactory.Token(SyntaxTokenKind.ParenClose);

        return new MethodInvocationParametersSyntax(parenOpen, parameterList, parenClose);
    }

    private CommaSeparatedSyntaxList<ValueExpressionSyntax>? CreateValueList(GdsScriptInstruction instruction)
    {
        if (instruction.Arguments.Length <= 0)
            return null;

        GdsScriptArgument[] arguments = instruction.Arguments;

        if (instruction.Type is 0)
            arguments = arguments[1..];

        var result = new List<ValueExpressionSyntax>();
        foreach (GdsScriptArgument argument in arguments)
            result.Add(CreateValueExpression(argument));

        return new CommaSeparatedSyntaxList<ValueExpressionSyntax>(result);
    }

    private ValueExpressionSyntax CreateValueExpression(GdsScriptArgument argument)
    {
        return CreateValueExpression(argument.Value, argument.Type);
    }

    private ValueExpressionSyntax CreateValueExpression(object? value, GdsScriptArgumentType argumentType, int rawArgumentType = -1)
    {
        ExpressionSyntax parameter = CreateArgumentExpression(value, argumentType);

        ValueMetadataParametersSyntax? parameters = null;
        if (rawArgumentType >= 0)
            parameters = CreateValueMetadataParameters(rawArgumentType);

        return new ValueExpressionSyntax(parameter, parameters);
    }

    private ValueMetadataParametersSyntax CreateValueMetadataParameters(int rawArgumentType)
    {
        SyntaxToken relSmaller = syntaxFactory.Token(SyntaxTokenKind.Smaller);
        var metadataParameter = CreateNumericLiteralExpression(rawArgumentType);
        SyntaxToken relBigger = syntaxFactory.Token(SyntaxTokenKind.Greater);

        return new ValueMetadataParametersSyntax(relSmaller, metadataParameter, relBigger);
    }

    private ExpressionSyntax CreateArgumentExpression(object? value, GdsScriptArgumentType argumentType)
    {
        return CreateLiteralExpression(value, argumentType);
    }

    private LiteralExpressionSyntax CreateLiteralExpression(object? value, GdsScriptArgumentType argumentType)
    {
        switch (argumentType)
        {
            case GdsScriptArgumentType.Int:
                return CreateNumericLiteralExpression((int)value!);

            case GdsScriptArgumentType.Float:
                return CreateFloatingNumericLiteralExpression((float)value!);

            case GdsScriptArgumentType.String:
                return CreateStringLiteralExpression((string)value!);

            case GdsScriptArgumentType.Jump:
                return CreateHashStringExpression((string)value!);

            default:
                throw new InvalidOperationException($"Unknown argument type {argumentType}.");
        }
    }

    private LiteralExpressionSyntax CreateNumericLiteralExpression(int value)
    {
        return new LiteralExpressionSyntax(syntaxFactory.NumericLiteral(value));
    }

    private LiteralExpressionSyntax CreateFloatingNumericLiteralExpression(float value)
    {
        return new LiteralExpressionSyntax(syntaxFactory.FloatingNumericLiteral(value));
    }

    private LiteralExpressionSyntax CreateStringLiteralExpression(string value)
    {
        return new LiteralExpressionSyntax(syntaxFactory.StringLiteral(value));
    }

    private LiteralExpressionSyntax CreateHashStringExpression(string value)
    {
        return new LiteralExpressionSyntax(syntaxFactory.HashStringLiteral(value));
    }
}