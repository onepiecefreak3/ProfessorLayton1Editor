using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Scripts;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;

namespace Logic.Business.Layton1ToolManagement.Scripts;

internal class Layton1ScriptReducer : ILayton1ScriptReducer
{
    public GdsScriptInstruction[] Reduce(GdsScriptFile script, GameState state)
    {
        var result = new List<GdsScriptInstruction>();

        for (var i = 0; i < script.Instructions.Count; i++)
        {
            GdsScriptInstruction instruction = script.Instructions[i];

            if (instruction.Type is 12 || (instruction.Type is 0 && instruction.Arguments[0].Value is 79))
                break;

            if (instruction.Type is 0 && instruction.Arguments[0].Value is 18 or 21 or 22)
            {
                i++;
                if (!EvaluateCondition(script.Instructions, ref i, state))
                {
                    if (script.Instructions[i].Type is 12)
                        continue;

                    if (script.Instructions[i].Arguments[^1].Value is not string jumpLabel)
                        continue;

                    int index = script.Instructions.FindIndex(ins => ins.Jump?.Label == jumpLabel);
                    if (index < 0)
                        continue;

                    i = index - 1;
                }

                continue;
            }

            if (instruction.Type is 0 && instruction.Arguments[0].Value is 23)
            {
                if (instruction.Arguments[^1].Value is not string jumpLabel)
                    continue;

                int index = script.Instructions.FindIndex(ins => ins.Jump?.Label == jumpLabel);
                if (index < 0)
                    continue;

                i = index - 1;
                continue;
            }

            if (instruction.Type is 0 && instruction.Arguments[0].Value is 72)
            {
                if (instruction.Arguments[1].Value is int puzzleId)
                    state.ActivePuzzleId = puzzleId;
            }

            result.Add(instruction);
        }

        return [.. result];
    }

    private static bool EvaluateCondition(List<GdsScriptInstruction> instructions, ref int index, GameState state)
    {
        var result = false;

        var isFirst = true;
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
                switch (instructions[index].Type)
                {
                    case 8:
                        isNegated = true;
                        break;

                    case 9:
                        isAnd = true;
                        isOr = false;
                        break;

                    case 10:
                        isAnd = false;
                        isOr = true;
                        break;
                }

                index++;
                continue;
            }

            bool evaluation = EvaluateInvocation(instructions[index], state);

            if (isNegated)
                evaluation = !evaluation;

            if (isFirst)
            {
                result = evaluation;
                isFirst = false;
            }
            else if (isAnd)
            {
                result &= evaluation;
            }
            else if (isOr)
            {
                result |= evaluation;
            }

            if (instructions[index].Arguments.Length > 0 && instructions[index].Arguments[^1].Type is GdsScriptArgumentType.Jump)
                break;

            index++;
        }

        return result;
    }

    private static bool EvaluateInvocation(GdsScriptInstruction instruction, GameState state)
    {
        return instruction.Arguments[0].Value switch
        {
            25 => true,
            26 => false,
            74 => !state.IsScriptReturn,
            75 => state.IsScriptReturn,
            77 => state.IsScriptSolved,
            193 => state.ReceivedUserInput,
            73 => state.Puzzles.TryGetValue(state.ActivePuzzleId, out var flag) && flag.Seen,
            84 => state.Puzzles.TryGetValue((int)instruction.Arguments[1].Value!, out var flag) && flag.Solved,
            78 => state.Puzzles.TryGetValue(state.ActivePuzzleId, out var flag) && flag.FinalSolved,
            119 => state.SolvedCount >= (int)instruction.Arguments[1].Value!,
            99 => state.State == (int)instruction.Arguments[1].Value!,
            141 => state.BitFlags.TryGetValue((int)instruction.Arguments[1].Value!, out bool flag) && flag,
            88 => state.ByteFlags.TryGetValue((int)instruction.Arguments[1].Value!, out bool flag) && flag,
            218 => state.DialogIndex == (int)instruction.Arguments[1].Value!,
            _ => throw new InvalidOperationException("Unknown conditional instruction.")
        };
    }
}