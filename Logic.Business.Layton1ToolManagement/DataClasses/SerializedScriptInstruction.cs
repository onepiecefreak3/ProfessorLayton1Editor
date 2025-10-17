using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace Logic.Business.Layton1ToolManagement.DataClasses;

class SerializedScriptInstruction
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required Layton1ScriptInstructionParameter[] Parameters { get; init; }
}