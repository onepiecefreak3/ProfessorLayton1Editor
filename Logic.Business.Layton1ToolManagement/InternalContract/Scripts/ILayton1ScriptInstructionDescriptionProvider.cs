using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace Logic.Business.Layton1ToolManagement.InternalContract.Scripts;

internal interface ILayton1ScriptInstructionDescriptionProvider
{
    bool MapsInstructionType(string gameCode, int instructionType);
    bool MapsMethodName(string gameCode, string methodName);

    string GetMethodName(string gameCode, int instructionType);
    int GetInstructionType(string gameCode, string methodName);

    Layton1ScriptInstruction GetDescription(string gameCode, int instructionType);
}