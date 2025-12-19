using Logic.Business.Layton1ToolManagement.DataClasses.Scripts;
using Logic.Business.Layton1ToolManagement.InternalContract.Scripts;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;

namespace Logic.Business.Layton1ToolManagement.Scripts;

internal class Layton1GdsFileBlockParser : ILayton1GdsFileBlockParser
{
    public GdsFileBlock Parse(GdsScriptFile script)
    {
        List<GdsFileBlock> blocks = CreateBlocks(script);
        RelateBlocks(script, blocks);
        ReduceBlocks(script, blocks);

        return blocks[0];
    }

    private static List<GdsFileBlock> CreateBlocks(GdsScriptFile script)
    {
        var blocks = new List<GdsFileBlock>();

        var startIndex = 0;
        var isConditional = false;

        for (var i = 0; i < script.Instructions.Count; i++)
        {
            var instruction = script.Instructions[i];

            if (instruction.Jump is not null && i - startIndex > 0)
            {
                blocks.Add(new GdsFileBlock
                {
                    StartIndex = startIndex,
                    EndIndex = i - 1
                });
                startIndex = i;
            }

            if (instruction.Type is 12 || (instruction.Type is 0 && instruction.Arguments[0].Value is 23))
            {
                blocks.Add(new GdsFileBlock
                {
                    StartIndex = startIndex,
                    EndIndex = i
                });
                startIndex = i + 1;
            }
            else if (instruction.Type is 0 && instruction.Arguments[0].Value is 18 or 21 or 22)
            {
                isConditional = true;
            }
            else if (isConditional && instruction.Type is 0 && instruction.Arguments[^1].Type is GdsScriptArgumentType.Jump)
            {
                isConditional = false;

                blocks.Add(new GdsFileBlock
                {
                    StartIndex = startIndex,
                    EndIndex = i
                });
                startIndex = i + 1;
            }
        }

        if (startIndex < script.Instructions.Count - 1)
        {
            blocks.Add(new GdsFileBlock
            {
                StartIndex = startIndex,
                EndIndex = script.Instructions.Count - 1
            });
        }

        return blocks;
    }

    private static void RelateBlocks(GdsScriptFile script, List<GdsFileBlock> blocks)
    {
        foreach (GdsFileBlock block in blocks)
        {
            var instructions = script.Instructions[block.StartIndex..(block.EndIndex + 1)];

            if (instructions[^1].Type is 12)
                continue;

            if (instructions[^1].Type is not 0 || instructions[^1].Arguments[^1].Type is not GdsScriptArgumentType.Jump)
            {
                RelateBlockByIndex(block, block.EndIndex + 1, blocks);
                continue;
            }

            var isIfConditional = instructions.Any(x => x.Type is 0 && x.Arguments[0].Value is 18 or 21);
            var isElseIfConditional = instructions.Any(x => x.Type is 0 && x.Arguments[0].Value is 22);
            var jumpLabel = (string)instructions[^1].Arguments[^1].Value!;

            if (isIfConditional || isElseIfConditional)
                RelateBlockByIndex(block, block.EndIndex + 1, blocks);

            var jumpedInstructionIndex = script.Instructions.FindIndex(i => i.Jump?.Label == jumpLabel);
            RelateBlockByIndex(block, jumpedInstructionIndex, blocks);

            if (!isElseIfConditional)
                continue;

            foreach (GdsFileBlock parent in block.Parents.ToArray())
            {
                var parentInstructions = script.Instructions[parent.StartIndex..(parent.EndIndex + 1)];
                if (parentInstructions.Any(x => x.Type is 0 && x.Arguments[0].Value is 18 or 21))
                    continue;

                if (parentInstructions[^1].Type is not 0 || parentInstructions[^1].Arguments[^1].Type is not GdsScriptArgumentType.Jump)
                {
                    parent.Children.Remove(block);
                    block.Parents.Remove(parent);
                }
            }
        }
    }

    private static void RelateBlockByIndex(GdsFileBlock block, int index, List<GdsFileBlock> blocks)
    {
        GdsFileBlock? nextBlock = blocks.FirstOrDefault(b => b.StartIndex == index);
        if (nextBlock is null)
            throw new InvalidOperationException("Could not determine next block.");

        block.Children.Add(nextBlock);
        nextBlock.Parents.Add(block);
    }

    private static void ReduceBlocks(GdsScriptFile script, List<GdsFileBlock> blocks)
    {
        foreach (var block in blocks)
        {
            if (block.EndIndex != block.StartIndex)
                continue;

            var instructions = script.Instructions[block.StartIndex..(block.EndIndex + 1)];

            if (instructions[0].Type is not 0 || instructions[0].Arguments[0].Value is not 23)
                continue;

            if (block.Children.Count != 1)
                throw new InvalidOperationException("Invalid goto statement branching to multiple children.");

            block.Children[0].Parents.Remove(block);
            block.Children[0].Parents.AddRange(block.Parents);

            foreach (var parent in block.Parents)
            {
                var childIndex = parent.Children.IndexOf(block);
                parent.Children[childIndex] = block.Children[0];
            }
        }
    }
}