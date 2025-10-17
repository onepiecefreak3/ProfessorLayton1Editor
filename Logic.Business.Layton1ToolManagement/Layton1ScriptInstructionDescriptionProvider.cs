using System.Text.Json;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.DataClasses;
using Logic.Business.Layton1ToolManagement.InternalContract;

namespace Logic.Business.Layton1ToolManagement;

internal class Layton1ScriptInstructionDescriptionProvider : ILayton1ScriptInstructionDescriptionProvider
{
    private readonly Dictionary<int, SerializedScriptInstruction> _methodNameMapping;
    private readonly Dictionary<string, int> _instructionTypeMapping;

    public Layton1ScriptInstructionDescriptionProvider(Layton1ToolManagementConfiguration config)
    {
        _methodNameMapping = InitializeMapping(config.MethodMappingPath) ?? [];
        _instructionTypeMapping = _methodNameMapping.Where(x => !string.IsNullOrEmpty(x.Value.Name)).ToDictionary(x => x.Value.Name, y => y.Key);
    }

    public bool MapsInstructionType(int instructionType)
    {
        if (!_methodNameMapping.TryGetValue(instructionType, out SerializedScriptInstruction? instruction))
            return false;

        return !string.IsNullOrEmpty(instruction.Name);
    }

    public bool MapsMethodName(string methodName)
    {
        return _instructionTypeMapping.ContainsKey(methodName);
    }

    public string GetMethodName(int instructionType)
    {
        if (!_methodNameMapping.TryGetValue(instructionType, out SerializedScriptInstruction? instruction))
            throw new InvalidOperationException($"Instruction type {instructionType} is not mapped.");

        if (string.IsNullOrEmpty(instruction.Name))
            throw new InvalidOperationException($"Instruction type {instructionType} does not have a name.");

        return instruction.Name;
    }

    public int GetInstructionType(string methodName)
    {
        if (!_instructionTypeMapping.TryGetValue(methodName, out int instructionType))
            throw new InvalidOperationException($"Method name {methodName} is not mapped.");

        return instructionType;
    }

    public Layton1ScriptInstruction GetDescription(int instructionType)
    {
        if (!_methodNameMapping.TryGetValue(instructionType, out SerializedScriptInstruction? instruction))
            throw new InvalidOperationException($"Instruction type {instructionType} is not mapped.");

        return new Layton1ScriptInstruction
        {
            Id = instructionType,
            Name = instruction.Name,
            Description = instruction.Description,
            Parameters = instruction.Parameters
        };
    }

    private static Dictionary<int, SerializedScriptInstruction>? InitializeMapping(string mappingPath)
    {
        mappingPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, mappingPath);
        if (!File.Exists(mappingPath))
            return new Dictionary<int, SerializedScriptInstruction>();

        string mappingJson = File.ReadAllText(mappingPath);
        return JsonSerializer.Deserialize<Dictionary<int, SerializedScriptInstruction>>(mappingJson);
    }
}