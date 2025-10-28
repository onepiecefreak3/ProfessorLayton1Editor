using System.Text.Json;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.DataClasses;
using Logic.Business.Layton1ToolManagement.InternalContract.Scripts;

namespace Logic.Business.Layton1ToolManagement.Scripts;

internal class Layton1ScriptInstructionDescriptionProvider : ILayton1ScriptInstructionDescriptionProvider
{
    private readonly Dictionary<string, Dictionary<int, SerializedScriptInstruction>> _methodNameMapping;
    private readonly Dictionary<string, Dictionary<string, int>> _instructionTypeMapping;

    public Layton1ScriptInstructionDescriptionProvider(Layton1ToolManagementConfiguration config)
    {
        _methodNameMapping = InitializeMapping(config.MethodMappingPath) ?? [];
        _instructionTypeMapping = _methodNameMapping.ToDictionary(x => x.Key,
            y => y.Value.Where(x => !string.IsNullOrEmpty(x.Value.Name)).ToDictionary(x => x.Value.Name, z => z.Key));
    }

    public bool MapsInstructionType(string gameCode, int instructionType)
    {
        if (!_methodNameMapping.TryGetValue(gameCode, out Dictionary<int, SerializedScriptInstruction>? instructions))
            return false;

        if (!instructions.TryGetValue(instructionType, out SerializedScriptInstruction? instruction))
            return false;

        return !string.IsNullOrEmpty(instruction.Name);
    }

    public bool MapsMethodName(string gameCode, string methodName)
    {
        return _instructionTypeMapping.TryGetValue(gameCode, out Dictionary<string, int>? instructionTypes)
               && instructionTypes.ContainsKey(methodName);
    }

    public string GetMethodName(string gameCode, int instructionType)
    {
        if (!_methodNameMapping.TryGetValue(gameCode, out Dictionary<int, SerializedScriptInstruction>? instructions))
            throw new InvalidOperationException($"Game code {gameCode} is not mapped.");

        if (!instructions.TryGetValue(instructionType, out SerializedScriptInstruction? instruction))
            throw new InvalidOperationException($"Instruction type {instructionType} is not mapped.");

        if (string.IsNullOrEmpty(instruction.Name))
            throw new InvalidOperationException($"Instruction type {instructionType} does not have a name.");

        return instruction.Name;
    }

    public int GetInstructionType(string gameCode, string methodName)
    {
        if (!_instructionTypeMapping.TryGetValue(gameCode, out Dictionary<string, int>? instructionTypes))
            throw new InvalidOperationException($"Game code {gameCode} is not mapped.");

        if (!instructionTypes.TryGetValue(methodName, out int instructionType))
            throw new InvalidOperationException($"Method name {methodName} is not mapped.");

        return instructionType;
    }

    public Layton1ScriptInstruction GetDescription(string gameCode, int instructionType)
    {
        if (!_methodNameMapping.TryGetValue(gameCode, out Dictionary<int, SerializedScriptInstruction>? instructions))
            throw new InvalidOperationException($"Game code {gameCode} is not mapped.");

        if (!instructions.TryGetValue(instructionType, out SerializedScriptInstruction? instruction))
            throw new InvalidOperationException($"Instruction type {instructionType} is not mapped.");

        return new Layton1ScriptInstruction
        {
            Id = instructionType,
            Name = instruction.Name,
            Description = instruction.Description,
            Parameters = instruction.Parameters
        };
    }

    private static Dictionary<string, Dictionary<int, SerializedScriptInstruction>>? InitializeMapping(string mappingPath)
    {
        mappingPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, mappingPath);
        if (!File.Exists(mappingPath))
            return [];

        string mappingJson = File.ReadAllText(mappingPath);
        return JsonSerializer.Deserialize<Dictionary<string, Dictionary<int, SerializedScriptInstruction>>>(mappingJson);
    }
}