using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;
using Logic.Domain.Level5Management.Contract.Script.Gds;
using Logic.Domain.Level5Management.Layton1.DataClasses.Script.Gds;

namespace Logic.Domain.Level5Management.Layton1.Script.Gds;

internal class GdsScriptParser(IGdsScriptReader reader) : IGdsScriptParser
{
    public GdsScriptFile Parse(Stream input)
    {
        GdsArgument[] arguments = reader.Read(input);

        return Parse(arguments);
    }

    public GdsScriptFile Parse(GdsArgument[] arguments)
    {
        var jumpTargets = ParseJumpTargets(arguments);

        return new GdsScriptFile
        {
            Instructions = [.. ParseInstructions(arguments, jumpTargets)]
        };
    }

    private IList<GdsScriptInstruction> ParseInstructions(GdsArgument[] arguments, GdsJumpTarget[] jumpTargets)
    {
        var result = new List<GdsScriptInstruction>();

        for (var i = 0; i < arguments.Length;)
        {
            int startIndex = i++;

            GdsScriptInstruction? instruction;
            if (arguments[startIndex].type is 0 && arguments[startIndex].value is (short)23)
            {
                int endIndex = i;

                while (endIndex < arguments.Length && arguments[endIndex].type is not 12 and not 6)
                    endIndex++;

                // Skip jump argument if it directly follows the jump instruction
                if (endIndex == i && endIndex < arguments.Length && arguments[endIndex].type is 6)
                    i++;

                instruction = CreateInstruction(arguments[startIndex], [arguments[endIndex]], jumpTargets);
            }
            else
            {
                while (i < arguments.Length && arguments[i].type is >= 1 and <= 6)
                    i++;

                instruction = CreateInstruction(arguments[startIndex], arguments[(startIndex + 1)..i], jumpTargets);
            }

            if (instruction is null)
                continue;

            result.Add(instruction);
        }

        return result;
    }

    private GdsJumpTarget[] ParseJumpTargets(GdsArgument[] arguments)
    {
        var jumpsTargets = new List<(GdsArgument, bool)>();

        Dictionary<int, GdsArgument> lookup = arguments.ToDictionary(x => x.offset, y => y);

        var isConditional = false;
        foreach (GdsArgument argument in arguments)
        {
            if (argument.type is 0 && argument.value is (short)18 or (short)21 or (short)22)
            {
                isConditional = true;
                continue;
            }

            if (argument.type is not 6)
                continue;

            if (!lookup.TryGetValue((int)argument.value!, out GdsArgument? jumpTarget))
                throw new InvalidOperationException($"Could not determine target of jump at position {argument.offset}.");

            jumpsTargets.Add((jumpTarget, isConditional));

            isConditional = false;
        }

        var result = new List<GdsJumpTarget>();

        var jumpIndex = 0;
        foreach ((GdsArgument originalJump, bool isConditionalJump) in jumpsTargets.OrderBy(j => j.Item1.offset))
        {
            GdsArgument shiftedJump = originalJump;

            if (isConditionalJump && shiftedJump.type is 0 && shiftedJump.value is (short)23)
            {
                int index = Array.IndexOf(arguments, shiftedJump);
                if (index < 0 || index + 1 >= arguments.Length)
                    continue;

                if (arguments[index + 1].type is 6 && index + 2 < arguments.Length)
                    shiftedJump = arguments[index + 2];
                else
                    shiftedJump = arguments[index + 1];
            }

            while (shiftedJump.type is 7)
            {
                int index = Array.IndexOf(arguments, shiftedJump);
                if (index < 0 || index + 1 >= arguments.Length)
                    continue;

                shiftedJump = arguments[index + 1];
            }

            GdsJumpTarget? shiftedTarget = result.FirstOrDefault(x => x.ShiftedOffset == shiftedJump.offset);
            if (shiftedTarget is not null)
            {
                shiftedTarget.Offsets.Add(originalJump.offset);
            }
            else
            {
                result.Add(new GdsJumpTarget
                {
                    Label = $"@{jumpIndex++:000}@",
                    Offsets = [originalJump.offset],
                    ShiftedOffset = shiftedJump.offset
                });
            }
        }

        return [.. result];
    }

    private static GdsScriptInstruction? CreateInstruction(GdsArgument instruction, GdsArgument[] parameters, GdsJumpTarget[] jumpTargets)
    {
        if (instruction.type is 7)
            return null;

        List<GdsScriptArgument> arguments = parameters.Select(p => CreateArgument(p, jumpTargets)).ToList();

        switch (instruction.type)
        {
            case 0:
                arguments.Insert(0, CreateArgument(instruction.value, false));
                break;
        }

        GdsJumpTarget? jumpTarget = jumpTargets.FirstOrDefault(x => x.ShiftedOffset == instruction.offset);

        return new GdsScriptInstruction
        {
            Type = instruction.type,
            Arguments = [.. arguments],
            Jump = jumpTarget is null ? null : new GdsScriptJump
            {
                Label = jumpTarget.Label
            }
        };
    }

    private static GdsScriptArgument CreateArgument(GdsArgument argument, GdsJumpTarget[] jumpTargets)
    {
        object? value = argument.value;
        if (argument.type is 6)
        {
            GdsJumpTarget jumpTarget = GetJumpTarget(argument, jumpTargets);
            value = jumpTarget.Label;
        }

        return new GdsScriptArgument
        {
            Type = argument.type switch
            {
                1 => GdsScriptArgumentType.Int,
                2 => GdsScriptArgumentType.Float,
                3 => GdsScriptArgumentType.String,
                6 => GdsScriptArgumentType.Jump,
                _ => throw new InvalidOperationException($"Unknown argument type {argument.type}.")
            },
            Value = value
        };
    }

    private static GdsScriptArgument CreateArgument(object? value, bool isJump)
    {
        return new GdsScriptArgument
        {
            Type = value switch
            {
                int => GdsScriptArgumentType.Int,
                short => GdsScriptArgumentType.Int,
                float => GdsScriptArgumentType.Float,
                string => isJump ? GdsScriptArgumentType.Jump : GdsScriptArgumentType.String,
                _ => throw new InvalidOperationException($"Unknown argument type {value?.GetType()}.")
            },
            Value = value is short shortValue ? (int)shortValue : value
        };
    }

    private static GdsJumpTarget GetJumpTarget(GdsArgument argument, GdsJumpTarget[] jumpTargets)
    {
        if (argument.value is not int intValue)
            throw new InvalidOperationException("Invalid argument type for jump target.");

        GdsJumpTarget? jumpTarget = jumpTargets.FirstOrDefault(x => x.Offsets.Contains(intValue));

        if (jumpTarget is null)
            throw new InvalidOperationException($"Could not determine target of jump at position {argument.offset}.");

        return jumpTarget;
    }
}