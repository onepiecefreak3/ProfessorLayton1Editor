using Logic.Business.Layton1ToolManagement.Contract.Scripts;
using Logic.Business.Layton1ToolManagement.DataClasses.Scripts;
using Logic.Business.Layton1ToolManagement.InternalContract.Scripts;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Layton1;
using Logic.Domain.CodeAnalysisManagement.Contract.Level5;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;
using System.Globalization;
using System.Text.RegularExpressions;
using Logic.Business.Layton1ToolManagement.Enums.Scripts;

namespace Logic.Business.Layton1ToolManagement.Scripts;

internal class Layton1ScriptCodeUnitConverter(ILayton1ScriptInstructionDescriptionProvider layton1ScriptInstructionProvider,
    ILayton1CodeUnitBlockParser blockParser, ILayton1SyntaxFactory syntaxFactory) : ILayton1ScriptCodeUnitConverter
{
    private readonly Regex _subPattern = new("^sub[0-9]+$", RegexOptions.Compiled);

    public GdsScriptFile CreateScriptFile(CodeUnitSyntax tree, string gameCode)
    {
        if (tree.MethodDeclaration is null)
            return new GdsScriptFile { Instructions = [CreateReturnInstruction()] };

        if (tree.MethodDeclaration.Body.Statements[^1] is not ReturnStatementSyntax)
            tree.MethodDeclaration.Body.SetStatements([.. tree.MethodDeclaration.Body.Statements.Concat([CreateReturnStatement()])]);

        var instructions = new List<GdsScriptInstruction>();

        AddStatements(instructions, tree.MethodDeclaration.Body.Statements, gameCode);

        var result = new GdsScriptFile
        {
            Instructions = [.. instructions]
        };

        return result;
    }

    private void AddStatements(List<GdsScriptInstruction> instructions, IReadOnlyList<StatementSyntax> statements, string gameCode)
    {
        CodeUnitBlock block = blockParser.Parse(statements);

        AddStatements(instructions, block, BranchType.If, null, gameCode);
    }

    private void AddStatements(List<GdsScriptInstruction> instructions, CodeUnitBlock block, BranchType branchType, CodeUnitBlock? targetBlock, string gameCode)
    {
        GdsScriptJump? jump = null;

        if (block.Label is not null)
            jump = new GdsScriptJump { Label = block.Label };

        foreach (StatementSyntax statement in block.Statements)
        {
            switch (statement)
            {
                case MethodInvocationStatementSyntax methodInvocation:
                    instructions.Add(CreateMethodInvocationInstruction(methodInvocation.Invocation, jump, gameCode));
                    break;

                case ReturnStatementSyntax:
                    if (block.Children.Count <= 0 && block.Statements[^1] == statement)
                        instructions.Add(CreateReturnInstruction(jump));
                    else
                        instructions.Add(CreateInstruction(0, [CreateArgument(GdsScriptArgumentType.Int, 79)], jump));
                    break;

                case BreakStatementSyntax:
                    instructions.Add(CreateBreakInstruction(jump));
                    return;

                case WhileStatementSyntax whileStatement:
                    if (block.Children[1].Label is null)
                        throw new InvalidOperationException("Branch does not have a label.");

                    AddConditionalInstructions(instructions, whileStatement.Expression, BranchType.While, block.Children[1].Label!, jump, gameCode);
                    AddStatements(instructions, block.Children[0], BranchType.If, block.Children[1], gameCode);

                    if (block.Children[1] != targetBlock)
                        AddStatements(instructions, block.Children[1], BranchType.If, targetBlock, gameCode);
                    return;

                case IfStatementSyntax ifStatement:
                    if (block.Children[1].Label is null)
                        throw new InvalidOperationException("Branch does not have a label.");

                    AddConditionalInstructions(instructions, ifStatement.Expression, branchType, block.Children[1].Label!, jump, gameCode);
                    AddStatements(instructions, block.Children[0], BranchType.If, block.Children[1], gameCode);

                    if (block.Children[1] != targetBlock)
                        AddStatements(instructions, block.Children[1], BranchType.If, targetBlock, gameCode);
                    return;

                case IfElseStatementSyntax ifElseStatement:
                    if (block.Children[1].Label is null)
                        throw new InvalidOperationException("Branch does not have a label.");

                    CodeUnitBlock? ifElseTargetBlock = GetIfElseTargetBlock(block.Children[0], block.Children[1]);

                    AddConditionalInstructions(instructions, ifElseStatement.Expression, branchType, block.Children[1].Label!, jump, gameCode);
                    AddStatements(instructions, block.Children[0], BranchType.If, ifElseTargetBlock, gameCode);
                    AddStatements(instructions, block.Children[1], BranchType.Else, ifElseTargetBlock, gameCode);

                    if (ifElseTargetBlock is not null && ifElseTargetBlock != targetBlock)
                        AddStatements(instructions, ifElseTargetBlock, BranchType.If, targetBlock, gameCode);
                    return;

                default:
                    throw CreateException("Only method invocations, if, while, break, and return are allowed.", statement.Location);
            }

            jump = null;
        }

        if (block.Children.Count <= 0)
            return;

        if (block.StartIndex + block.Statements.Count != block.Children[0].StartIndex)
            instructions.Add(CreateInstruction(0, [CreateArgument(GdsScriptArgumentType.Int, 23), CreateArgument(GdsScriptArgumentType.Jump, block.Children[0].Label!)], jump));

        if (block.Children[0] != targetBlock)
            AddStatements(instructions, block.Children[0], BranchType.If, targetBlock, gameCode);
    }

    private GdsScriptInstruction CreateReturnInstruction(GdsScriptJump? jump = null)
    {
        return CreateInstruction(12, [], jump);
    }

    private GdsScriptInstruction CreateBreakInstruction(GdsScriptJump? jump = null)
    {
        return CreateInstruction(11, [], jump);
    }

    private void AddConditionalInstructions(List<GdsScriptInstruction> instructions, ExpressionSyntax conditional, BranchType type, string target, GdsScriptJump? jump, string gameCode)
    {
        instructions.Add(CreateInstruction(0, [CreateArgument(GdsScriptArgumentType.Int, type switch
        {
            BranchType.While => 21,
            BranchType.Else => 22,
            _ => 18
        })], jump));

        var isNegate = false;

        ExpressionSyntax expression = conditional;
        while (expression is LogicalExpressionSyntax logicalExpression)
        {
            AddConditionalInstruction(instructions, logicalExpression.Left, gameCode, ref isNegate);

            if (logicalExpression.Operation.RawKind is (int)SyntaxTokenKind.AndKeyword)
            {
                instructions.Add(CreateInstruction(9, []));
            }
            else if (logicalExpression.Operation.RawKind is (int)SyntaxTokenKind.OrKeyword)
            {
                instructions.Add(CreateInstruction(10, []));
            }

            expression = logicalExpression.Right;
        }

        AddConditionalInstruction(instructions, expression, gameCode, ref isNegate);

        GdsScriptArgument jumpArgument = CreateArgument(GdsScriptArgumentType.Jump, target);
        instructions[^1].Arguments = instructions[^1].Arguments.Concat([jumpArgument]).ToArray();
    }

    private void AddConditionalInstruction(List<GdsScriptInstruction> instructions, ExpressionSyntax expression, string gameCode, ref bool isNegate)
    {
        if (expression is UnaryExpressionSyntax unaryLeft)
        {
            if (unaryLeft.Operation.RawKind is not (int)SyntaxTokenKind.NotKeyword)
                throw new InvalidOperationException("Unsupported unary expression.");

            instructions.Add(CreateInstruction(8, []));
            isNegate = true;

            expression = unaryLeft.Value;
        }

        switch (expression)
        {
            case LiteralExpressionSyntax { Literal.RawKind: (int)SyntaxTokenKind.TrueKeyword }:
                instructions.Add(CreateInstruction(0, [CreateArgument(GdsScriptArgumentType.Int, 25)]));
                return;

            case LiteralExpressionSyntax { Literal.RawKind: (int)SyntaxTokenKind.FalseKeyword }:
                instructions.Add(CreateInstruction(0, [CreateArgument(GdsScriptArgumentType.Int, 26)]));
                return;

            case MethodInvocationExpressionSyntax methodInvocation:
                instructions.Add(CreateMethodInvocationInstruction(methodInvocation, null, gameCode));
                return;
        }

        throw new InvalidOperationException("Only method invocations or true/false literals are allowed as conditional values.");
    }

    private CodeUnitBlock? GetIfElseTargetBlock(CodeUnitBlock left, CodeUnitBlock right)
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

    private CodeUnitBlock? GetRightIfElseBlock(CodeUnitBlock left, CodeUnitBlock right)
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

    private GdsScriptInstruction CreateMethodInvocationInstruction(MethodInvocationExpressionSyntax methodInvocation, GdsScriptJump? jump, string gameCode)
    {
        return CreateMethodInvocationInstruction(methodInvocation.Identifier, methodInvocation.Parameters, jump, gameCode);
    }

    private GdsScriptInstruction CreateMethodInvocationInstruction(SyntaxToken identifier, MethodInvocationParametersSyntax parameters,
        GdsScriptJump? jump, string gameCode)
    {
        int invocationType = GetInvocationType(identifier, gameCode);

        var arguments = new List<GdsScriptArgument>
        {
            CreateArgument(GdsScriptArgumentType.Int, invocationType)
        };

        if (parameters.ParameterList != null)
            foreach (LiteralExpressionSyntax parameter in parameters.ParameterList.Elements)
                arguments.Add(CreateArgument(parameter));

        return CreateInstruction(0, arguments, jump);
    }

    private int GetInvocationType(SyntaxToken identifier, string gameCode)
    {
        if (_subPattern.IsMatch(identifier.Text))
            return GetNumberFromStringEnd(identifier.Text);

        if (layton1ScriptInstructionProvider.MapsMethodName(gameCode, identifier.Text))
            return layton1ScriptInstructionProvider.GetInstructionType(gameCode, identifier.Text);

        throw CreateException("Could not determine instruction type.", identifier.Location);
    }

    private GdsScriptInstruction CreateInstruction(int instructionType, List<GdsScriptArgument> arguments, GdsScriptJump? jump = null)
    {
        return new GdsScriptInstruction
        {
            Type = instructionType,
            Arguments = [.. arguments],
            Jump = jump
        };
    }

    private GdsScriptArgument CreateArgument(LiteralExpressionSyntax literal)
    {
        GdsScriptArgumentType type;
        object value;

        switch (literal.Literal.RawKind)
        {
            case (int)SyntaxTokenKind.NumericLiteral:
                type = GdsScriptArgumentType.Int;
                value = GetNumericLiteral(literal);
                break;

            case (int)SyntaxTokenKind.FloatingNumericLiteral:
                type = GdsScriptArgumentType.Float;
                value = GetFloatingNumericLiteral(literal);
                break;

            case (int)SyntaxTokenKind.StringLiteral:
                type = GdsScriptArgumentType.String;
                value = GetStringLiteral(literal);
                break;

            case (int)SyntaxTokenKind.HashStringLiteral:
                type = GdsScriptArgumentType.Jump;
                value = GetHashStringLiteral(literal);
                break;

            default:
                throw CreateException($"Invalid literal {(SyntaxTokenKind)literal.Literal.RawKind}.", literal.Location,
                    SyntaxTokenKind.NumericLiteral, SyntaxTokenKind.FloatingNumericLiteral, SyntaxTokenKind.StringLiteral,
                    SyntaxTokenKind.HashStringLiteral);
        }

        return CreateArgument(type, value);
    }

    private GdsScriptArgument CreateArgument(GdsScriptArgumentType type, object value)
    {
        return new GdsScriptArgument
        {
            Type = type,
            Value = value
        };
    }

    private ReturnStatementSyntax CreateReturnStatement()
    {
        SyntaxToken returnToken = syntaxFactory.Token(SyntaxTokenKind.ReturnKeyword);
        SyntaxToken semicolon = syntaxFactory.Token(SyntaxTokenKind.Semicolon);

        return new ReturnStatementSyntax(returnToken, semicolon);
    }

    private int GetNumericLiteral(LiteralExpressionSyntax literal)
    {
        if (literal.Literal.RawKind != (int)SyntaxTokenKind.NumericLiteral)
            throw CreateException($"Invalid literal {(SyntaxTokenKind)literal.Literal.RawKind}.", literal.Location, SyntaxTokenKind.NumericLiteral);

        return literal.Literal.Text.StartsWith("0x", StringComparison.Ordinal) ?
            int.Parse(literal.Literal.Text[2..], NumberStyles.HexNumber) :
            int.Parse(literal.Literal.Text);
    }

    private float GetFloatingNumericLiteral(LiteralExpressionSyntax literal)
    {
        if (literal.Literal.RawKind != (int)SyntaxTokenKind.FloatingNumericLiteral)
            throw CreateException($"Invalid literal {(SyntaxTokenKind)literal.Literal.RawKind}.", literal.Location, SyntaxTokenKind.FloatingNumericLiteral);

        return float.Parse(literal.Literal.Text[..^1], CultureInfo.GetCultureInfo("en-gb"));
    }

    private string GetHashStringLiteral(LiteralExpressionSyntax literal)
    {
        if (literal.Literal.RawKind != (int)SyntaxTokenKind.HashStringLiteral)
            throw CreateException($"Invalid literal {(SyntaxTokenKind)literal.Literal.RawKind}.", literal.Location, SyntaxTokenKind.HashStringLiteral);

        return literal.Literal.Text[1..^2];
    }

    private string GetStringLiteral(LiteralExpressionSyntax literal)
    {
        if (literal.Literal.RawKind != (int)SyntaxTokenKind.StringLiteral)
            throw CreateException($"Invalid literal {(SyntaxTokenKind)literal.Literal.RawKind}.", literal.Location, SyntaxTokenKind.StringLiteral);

        return literal.Literal.Text[1..^1].Replace("\\\"", "\"");
    }

    private int GetNumberFromStringEnd(string text)
    {
        int startIndex = text.Length;
        while (text[startIndex - 1] >= '0' && text[startIndex - 1] <= '9')
            startIndex--;

        return int.Parse(text[startIndex..]);
    }

    private Exception CreateException(string message, SyntaxLocation location, params SyntaxTokenKind[] expected)
    {
        message = $"{message} (Line {location.Line}, Column {location.Column})";

        if (expected.Length > 0)
        {
            message = expected.Length == 1 ?
                $"{message} (Expected {expected[0]})" :
                $"{message} (Expected any of {string.Join(", ", expected)})";
        }

        return new InvalidOperationException(message);
    }
}