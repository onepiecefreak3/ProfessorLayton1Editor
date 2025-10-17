namespace Logic.Business.Layton1ToolManagement.Contract.DataClasses;

public class Layton1ScriptInstruction
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required Layton1ScriptInstructionParameter[] Parameters { get; init; }
}