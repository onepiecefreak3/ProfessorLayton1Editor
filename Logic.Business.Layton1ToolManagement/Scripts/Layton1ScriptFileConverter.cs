using Logic.Business.Layton1ToolManagement.Contract.Scripts;
using Logic.Business.Layton1ToolManagement.DataClasses.Scripts;
using Logic.Business.Layton1ToolManagement.InternalContract.Scripts;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Layton1;
using Logic.Domain.CodeAnalysisManagement.Contract.Level5;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;

namespace Logic.Business.Layton1ToolManagement.Scripts;

internal class Layton1ScriptFileConverter(ILayton1SyntaxFactory syntaxFactory, ILayton1ScriptInstructionDescriptionProvider layton1ScriptInstructionProvider,
    ILayton1GdsFileBlockParser blockParser) : ILayton1ScriptFileConverter
{
    public CodeUnitSyntax CreateCodeUnit(GdsScriptFile script, string gameCode)
    {
        MethodDeclarationSyntax method = CreateMethodDeclaration(script, gameCode);

        return new CodeUnitSyntax(method);
    }

    private MethodDeclarationSyntax CreateMethodDeclaration(GdsScriptFile script, string gameCode)
    {
        SyntaxToken identifier = syntaxFactory.Identifier("Main");
        var parameters = CreateMethodDeclarationParameters();
        var body = CreateMethodDeclarationBody(script, gameCode);

        return new MethodDeclarationSyntax(identifier, parameters, body);
    }

    private MethodDeclarationParametersSyntax CreateMethodDeclarationParameters()
    {
        SyntaxToken parenOpen = syntaxFactory.Token(SyntaxTokenKind.ParenOpen);
        SyntaxToken parenClose = syntaxFactory.Token(SyntaxTokenKind.ParenClose);

        return new MethodDeclarationParametersSyntax(parenOpen, parenClose);
    }

    private BodySyntax CreateMethodDeclarationBody(GdsScriptFile script, string gameCode)
    {
        var statements = CreateStatements(script, gameCode);

        return CreateBodySyntax(statements);
    }

    private IReadOnlyList<StatementSyntax> CreateStatements(GdsScriptFile script, string gameCode)
    {
        var block = blockParser.Parse(script);
        return CreateStatements(script, block, null, gameCode);
    }

    private IReadOnlyList<StatementSyntax> CreateStatements(GdsScriptFile script, GdsFileBlock block, GdsFileBlock? targetBlock, string gameCode)
    {
        var result = new List<StatementSyntax>();

        var instructions = script.Instructions[block.StartIndex..(block.EndIndex + 1)];

        for (var i = 0; i < instructions.Count; i++)
        {
            GdsScriptInstruction instruction = instructions[i];

            if (instruction.Type is 0 && instruction.Arguments[0].Value is 23)
                break;

            if (instruction.Type is 0 && instruction.Arguments[0].Value is 79)
            {
                result.Add(CreateReturnStatement());
                continue;
            }

            if (instruction.Type is 0 && instruction.Arguments[0].Value is 18 or 22)
            {
                if (block.Children.Count <= 1)
                {
                    // if {}
                    result.Add(CreateEmptyIfStatement(script, block, i + 1, gameCode));

                    if (block.Children[0] != targetBlock)
                        result.AddRange(CreateStatements(script, block.Children[0], targetBlock, gameCode));
                }
                else if (block.Children[1].Parents.Count >= 2)
                {
                    // if ...
                    result.Add(CreateIfStatement(script, block, block.Children[1], i + 1, gameCode));

                    if (block.Children[1] != targetBlock)
                        result.AddRange(CreateStatements(script, block.Children[1], targetBlock, gameCode));
                }
                else
                {
                    // if ... else ...
                    var ifElseTargetBlock = GetIfElseTargetBlock(block.Children[0], block.Children[1]);
                    result.Add(CreateIfElseStatement(script, block, ifElseTargetBlock, i + 1, gameCode));

                    if (ifElseTargetBlock is not null && ifElseTargetBlock != targetBlock)
                        result.AddRange(CreateStatements(script, ifElseTargetBlock, targetBlock, gameCode));
                }
                break;
            }

            if (instruction.Type is 0 && instruction.Arguments[0].Value is 21)
            {
                if (block.Children[1].Parents.Count >= 2)
                {
                    // while ...
                    result.Add(CreateWhileStatement(script, block, block.Children[1], i + 1, gameCode));

                    if (block.Children[1] != targetBlock)
                        result.AddRange(CreateStatements(script, block.Children[1], targetBlock, gameCode));
                }
                break;
            }

            result.Add(CreateStatement(instruction, gameCode));
        }

        return result;
    }

    private WhileStatementSyntax CreateWhileStatement(GdsScriptFile script, GdsFileBlock block, GdsFileBlock targetBlock, int conditionalIndex, string gameCode)
    {
        SyntaxToken whileToken = syntaxFactory.Token(SyntaxTokenKind.WhileKeyword);

        var conditionalInstructions = script.Instructions[block.StartIndex..(block.EndIndex + 1)];
        var conditional = CreateConditionalExpression(conditionalInstructions, gameCode, ref conditionalIndex);

        var statements = CreateStatements(script, block.Children[0], targetBlock, gameCode);
        var body = CreateBodySyntax(statements);

        return new WhileStatementSyntax(whileToken, conditional, body);
    }

    private IfStatementSyntax CreateIfStatement(GdsScriptFile script, GdsFileBlock block, GdsFileBlock targetBlock, int conditionalIndex, string gameCode)
    {
        SyntaxToken ifToken = syntaxFactory.Token(SyntaxTokenKind.IfKeyword);

        var conditionalInstructions = script.Instructions[block.StartIndex..(block.EndIndex + 1)];
        var conditional = CreateConditionalExpression(conditionalInstructions, gameCode, ref conditionalIndex);

        var statements = CreateStatements(script, block.Children[0], targetBlock, gameCode);
        var body = CreateBodySyntax(statements);

        return new IfStatementSyntax(ifToken, conditional, body);
    }

    private IfStatementSyntax CreateEmptyIfStatement(GdsScriptFile script, GdsFileBlock block, int conditionalIndex, string gameCode)
    {
        SyntaxToken ifToken = syntaxFactory.Token(SyntaxTokenKind.IfKeyword);

        var conditionalInstructions = script.Instructions[block.StartIndex..(block.EndIndex + 1)];
        var conditional = CreateConditionalExpression(conditionalInstructions, gameCode, ref conditionalIndex);

        var body = CreateBodySyntax([]);

        return new IfStatementSyntax(ifToken, conditional, body);
    }

    private IfElseStatementSyntax CreateIfElseStatement(GdsScriptFile script, GdsFileBlock block, GdsFileBlock? targetBlock, int conditionalIndex, string gameCode)
    {
        SyntaxToken ifToken = syntaxFactory.Token(SyntaxTokenKind.IfKeyword);

        var conditionalInstructions = script.Instructions[block.StartIndex..(block.EndIndex + 1)];
        var conditional = CreateConditionalExpression(conditionalInstructions, gameCode, ref conditionalIndex);

        var statements = CreateStatements(script, block.Children[0], targetBlock, gameCode);
        var thenBody = CreateBodySyntax(statements);

        statements = CreateStatements(script, block.Children[1], targetBlock, gameCode);

        ElseSyntax elseSyntax;
        if (statements.Count > 0)
        {
            elseSyntax = statements[0] switch
            {
                IfStatementSyntax ifStatement => CreateElseIfBody(ifStatement),
                IfElseStatementSyntax ifElseStatement => CreateElseIfBody(ifElseStatement),
                _ => CreateElseBody(statements)
            };
        }
        else
        {
            elseSyntax = CreateElseBody(statements);
        }

        return new IfElseStatementSyntax(ifToken, conditional, thenBody, elseSyntax);
    }

    private ElseBodySyntax CreateElseBody(IReadOnlyList<StatementSyntax> statements)
    {
        SyntaxToken elseToken = syntaxFactory.Token(SyntaxTokenKind.ElseKeyword);
        BodySyntax body = CreateBodySyntax(statements);

        return new ElseBodySyntax(elseToken, body);
    }

    private ElseIfBodySyntax CreateElseIfBody(StatementSyntax ifExpression)
    {
        SyntaxToken elseToken = syntaxFactory.Token(SyntaxTokenKind.ElseKeyword);

        return new ElseIfBodySyntax(elseToken, ifExpression);
    }

    private BodySyntax CreateBodySyntax(IReadOnlyList<StatementSyntax> statements)
    {
        SyntaxToken curlyOpenToken = syntaxFactory.Token(SyntaxTokenKind.CurlyOpen);
        SyntaxToken curlyCloseToken = syntaxFactory.Token(SyntaxTokenKind.CurlyClose);

        return new BodySyntax(curlyOpenToken, statements, curlyCloseToken);
    }

    private GdsFileBlock? GetIfElseTargetBlock(GdsFileBlock left, GdsFileBlock right)
    {
        var target = GetRightIfElseBlock(left, right);
        if (target is not null)
            return target;

        foreach (var child in left.Children)
        {
            target = GetIfElseTargetBlock(child, right);
            if (target is not null)
                return target;
        }

        return null;
    }

    private GdsFileBlock? GetRightIfElseBlock(GdsFileBlock left, GdsFileBlock right)
    {
        if (left == right)
            return right;

        foreach (var child in right.Children)
        {
            var target = GetRightIfElseBlock(left, child);
            if (target is not null)
                return target;
        }

        return null;
    }

    private StatementSyntax CreateStatement(GdsScriptInstruction instruction, string gameCode)
    {
        switch (instruction.Type)
        {
            case 0:
                return CreateMethodInvocationStatement(instruction, gameCode);

            case 11:
                return CreateBreakStatement();

            case 12:
                return CreateReturnStatement();

            default:
                throw new InvalidOperationException($"Unknown instruction type {instruction.Type}.");
        }
    }

    private BreakStatementSyntax CreateBreakStatement()
    {
        SyntaxToken returnToken = syntaxFactory.Token(SyntaxTokenKind.BreakKeyword);
        SyntaxToken semicolon = syntaxFactory.Token(SyntaxTokenKind.Semicolon);

        return new BreakStatementSyntax(returnToken, semicolon);
    }

    private ReturnStatementSyntax CreateReturnStatement()
    {
        SyntaxToken returnToken = syntaxFactory.Token(SyntaxTokenKind.ReturnKeyword);
        SyntaxToken semicolon = syntaxFactory.Token(SyntaxTokenKind.Semicolon);

        return new ReturnStatementSyntax(returnToken, semicolon);
    }

    private ExpressionSyntax CreateConditionalExpression(List<GdsScriptInstruction> instructions, string gameCode, ref int index)
    {
        ExpressionSyntax? finalExpression = null;

        var isNegated = false;
        var isAnd = false;
        var isOr = false;

        while (index < instructions.Count)
        {
            if (instructions[index].Type is 12)
            {
                index--;
                break;
            }

            if (instructions[index].Type >= 7)
            {
                if (instructions[index].Type is 8)
                {
                    isNegated = true;
                }
                else if (instructions[index].Type is 9)
                {
                    isAnd = true;
                    isOr = false;
                }
                else if (instructions[index].Type is 10)
                {
                    isAnd = false;
                    isOr = true;
                }

                index++;
                continue;
            }

            ExpressionSyntax expression = instructions[index].Arguments[0].Value switch
            {
                25 => CreateTrueLiteralExpression(),
                26 => CreateFalseLiteralExpression(),
                _ => CreateMethodInvocationExpression(instructions[index], gameCode)
            };

            if (isNegated)
                expression = CreateNotExpression(expression);

            if (finalExpression is null)
            {
                finalExpression = expression;
            }
            else
            {
                if (isAnd)
                    finalExpression = CreateAndExpression(finalExpression, expression);
                else if (isOr)
                    finalExpression = CreateOrExpression(finalExpression, expression);
                else
                    finalExpression = expression;
            }

            if (instructions[index].Arguments.Length > 0 && instructions[index].Arguments[^1].Type is GdsScriptArgumentType.Jump)
                break;

            index++;
        }

        if (finalExpression is null)
            throw new InvalidOperationException("There was no expression given for the conditional.");

        return finalExpression;
    }

    private UnaryExpressionSyntax CreateNotExpression(ExpressionSyntax expression)
    {
        SyntaxToken notKeyword = syntaxFactory.Token(SyntaxTokenKind.NotKeyword);

        return new UnaryExpressionSyntax(notKeyword, expression);
    }

    private LogicalExpressionSyntax CreateAndExpression(ExpressionSyntax left, ExpressionSyntax right)
    {
        SyntaxToken andKeyword = syntaxFactory.Token(SyntaxTokenKind.AndKeyword);

        return new LogicalExpressionSyntax(left, andKeyword, right);
    }

    private LogicalExpressionSyntax CreateOrExpression(ExpressionSyntax left, ExpressionSyntax right)
    {
        SyntaxToken orKeyword = syntaxFactory.Token(SyntaxTokenKind.OrKeyword);

        return new LogicalExpressionSyntax(left, orKeyword, right);
    }

    private MethodInvocationStatementSyntax CreateMethodInvocationStatement(GdsScriptInstruction instruction, string gameCode)
    {
        SyntaxToken identifier = CreateMethodNameIdentifier(instruction, gameCode);

        return CreateMethodInvocationStatement(identifier, instruction);
    }

    private MethodInvocationStatementSyntax CreateMethodInvocationStatement(SyntaxToken methodName, GdsScriptInstruction instruction)
    {
        var parameters = CreateMethodInvocationExpressionParameters(instruction, false);
        var expression = new MethodInvocationExpressionSyntax(methodName, parameters);

        SyntaxToken semicolon = syntaxFactory.Token(SyntaxTokenKind.Semicolon);
        return new MethodInvocationStatementSyntax(expression, semicolon);
    }

    private MethodInvocationExpressionSyntax CreateMethodInvocationExpression(GdsScriptInstruction instruction, string gameCode)
    {
        SyntaxToken identifier = CreateMethodNameIdentifier(instruction, gameCode);

        return CreateMethodInvocationExpression(identifier, instruction);
    }

    private MethodInvocationExpressionSyntax CreateMethodInvocationExpression(SyntaxToken methodName, GdsScriptInstruction instruction)
    {
        var parameters = CreateMethodInvocationExpressionParameters(instruction, true);

        return new MethodInvocationExpressionSyntax(methodName, parameters);
    }

    private SyntaxToken CreateMethodNameIdentifier(GdsScriptInstruction instruction, string gameCode)
    {
        if (instruction.Arguments.Length < 1)
            throw new InvalidOperationException("Missing call type for instruction.");

        if (instruction.Arguments[0].Type is not GdsScriptArgumentType.Int)
            throw new InvalidOperationException("Wrong call type for instruction.");

        var instructionType = (int)instruction.Arguments[0].Value!;

        if (!layton1ScriptInstructionProvider.MapsInstructionType(gameCode, instructionType))
            return syntaxFactory.Identifier($"sub{instructionType}");

        string methodName = layton1ScriptInstructionProvider.GetMethodName(gameCode, instructionType);
        return syntaxFactory.Identifier(methodName);
    }

    private MethodInvocationParametersSyntax CreateMethodInvocationExpressionParameters(GdsScriptInstruction instruction, bool ignoreJump)
    {
        SyntaxToken parenOpen = syntaxFactory.Token(SyntaxTokenKind.ParenOpen);
        var parameterList = CreateValueList(instruction, ignoreJump);
        SyntaxToken parenClose = syntaxFactory.Token(SyntaxTokenKind.ParenClose);

        return new MethodInvocationParametersSyntax(parenOpen, parameterList, parenClose);
    }

    private CommaSeparatedSyntaxList<LiteralExpressionSyntax>? CreateValueList(GdsScriptInstruction instruction, bool ignoreJump)
    {
        if (instruction.Arguments.Length <= 0)
            return null;

        GdsScriptArgument[] arguments = instruction.Arguments;

        if (instruction.Type is 0)
            arguments = arguments[1..];

        var result = new List<LiteralExpressionSyntax>();
        foreach (GdsScriptArgument argument in arguments)
        {
            if (ignoreJump && argument.Type is GdsScriptArgumentType.Jump)
                continue;

            result.Add(CreateLiteralExpression(argument));
        }

        return new CommaSeparatedSyntaxList<LiteralExpressionSyntax>(result);
    }

    private LiteralExpressionSyntax CreateLiteralExpression(GdsScriptArgument argument)
    {
        return CreateLiteralExpression(argument.Value, argument.Type);
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

    private LiteralExpressionSyntax CreateTrueLiteralExpression()
    {
        return new LiteralExpressionSyntax(syntaxFactory.Token(SyntaxTokenKind.TrueKeyword));
    }

    private LiteralExpressionSyntax CreateFalseLiteralExpression()
    {
        return new LiteralExpressionSyntax(syntaxFactory.Token(SyntaxTokenKind.FalseKeyword));
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