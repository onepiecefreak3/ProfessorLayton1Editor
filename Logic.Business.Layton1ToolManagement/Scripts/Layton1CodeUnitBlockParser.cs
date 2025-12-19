using Logic.Business.Layton1ToolManagement.DataClasses.Scripts;
using Logic.Business.Layton1ToolManagement.InternalContract.Scripts;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Layton1;

namespace Logic.Business.Layton1ToolManagement.Scripts;

internal class Layton1CodeUnitBlockParser : ILayton1CodeUnitBlockParser
{
    public CodeUnitBlock Parse(IReadOnlyList<StatementSyntax> statements)
    {
        CodeUnitBlock block = CreateBlocks(statements);

        return block;
    }

    private CodeUnitBlock CreateBlocks(IReadOnlyList<StatementSyntax> statements)
    {
        var block = new CodeUnitBlock();

        var index = 0;
        var labelIndex = 0;
        CreateBlocks(block, null, statements, ref index, ref labelIndex);

        return block;
    }

    private void CreateBlocks(CodeUnitBlock block, CodeUnitBlock? target, IReadOnlyList<StatementSyntax> statements, ref int index, ref int labelIndex)
    {
        block.StartIndex = index;

        for (var i = 0; i < statements.Count; i++)
        {
            var statement = statements[i];
            block.Statements.Add(statement);

            var shouldContinue = ProcessStatement(ref block, target, statements, i, ref index, ref labelIndex);
            if (!shouldContinue)
                return;
        }

        index += block.Statements.Count;

        if (target is not null)
        {
            block.Children.Add(target);
            target.Parents.Add(block);
        }
    }

    private bool ProcessStatement(ref CodeUnitBlock block, CodeUnitBlock? target, IReadOnlyList<StatementSyntax> statements, int statementIndex, ref int index, ref int labelIndex)
    {
        switch (statements[statementIndex])
        {
            case IfStatementSyntax ifStatement:
                if (statementIndex + 1 >= statements.Count && target is null)
                    throw new InvalidOperationException("Last statement in block needs target.");

                var ifBlock = new CodeUnitBlock();
                var elseBlock = statementIndex + 1 >= statements.Count ? target! : new CodeUnitBlock();

                index += block.Statements.Count;
                CreateBlocks(ifBlock, elseBlock, ifStatement.Body.Statements, ref index, ref labelIndex);

                block.Children.Add(ifBlock);
                block.Children.Add(elseBlock);

                ifBlock.Parents.Add(block);
                elseBlock.Parents.Add(block);

                if (statementIndex + 1 < statements.Count)
                {
                    elseBlock.Label = $"@{labelIndex++:000}@";
                    block = elseBlock;

                    block.StartIndex = index;
                    return true;
                }
                return false;

            case IfElseStatementSyntax ifElseStatement:
                if (statementIndex + 1 >= statements.Count && target is null)
                    throw new InvalidOperationException("Last statement in block needs target.");

                var nextBlock = statementIndex + 1 >= statements.Count ? target! : new CodeUnitBlock();

                index += block.Statements.Count;
                ProcessIfElseStatement(ifElseStatement, block, nextBlock, ref index, ref labelIndex);

                if (statementIndex + 1 < statements.Count)
                {
                    nextBlock.Label = $"@{labelIndex++:000}@";
                    block = nextBlock;

                    block.StartIndex = index;
                    return true;
                }
                return false;

            case WhileStatementSyntax whileStatement:
                if (statementIndex + 1 >= statements.Count && target is null)
                    throw new InvalidOperationException("Last statement in block needs target.");

                var whileBlock = new CodeUnitBlock();
                var nextBlock1 = statementIndex + 1 >= statements.Count ? target! : new CodeUnitBlock();

                index += block.Statements.Count;
                CreateBlocks(whileBlock, nextBlock1, whileStatement.Body.Statements, ref index, ref labelIndex);

                block.Children.Add(whileBlock);
                block.Children.Add(nextBlock1);

                whileBlock.Parents.Add(block);
                nextBlock1.Parents.Add(block);

                if (statementIndex + 1 < statements.Count)
                {
                    nextBlock1.Label = $"@{labelIndex++:000}@";
                    block = nextBlock1;

                    block.StartIndex = index;
                    return true;
                }
                return false;
        }

        return true;
    }

    private void ProcessIfElseStatement(IfElseStatementSyntax ifElseStatement, CodeUnitBlock block, CodeUnitBlock nextBlock, ref int index, ref int labelIndex)
    {
        var ifBlock = new CodeUnitBlock();
        var elseBlock = new CodeUnitBlock { Label = $"@{labelIndex++:000}@" };

        CreateBlocks(ifBlock, nextBlock, ifElseStatement.Body.Statements, ref index, ref labelIndex);

        switch (ifElseStatement.Else)
        {
            case ElseBodySyntax elseBody:
                CreateBlocks(elseBlock, nextBlock, elseBody.Body.Statements, ref index, ref labelIndex);
                break;

            case ElseIfBodySyntax elseIfBody:
                elseBlock.Statements.Add(elseIfBody.If);

                ProcessStatement(ref elseBlock, nextBlock, [elseIfBody.If], 0, ref index, ref labelIndex);
                break;
        }

        block.Children.Add(ifBlock);
        block.Children.Add(elseBlock);

        ifBlock.Parents.Add(block);
        elseBlock.Parents.Add(block);
    }
}