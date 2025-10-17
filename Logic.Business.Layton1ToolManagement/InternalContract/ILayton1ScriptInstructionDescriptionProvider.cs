using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace Logic.Business.Layton1ToolManagement.InternalContract;

internal interface ILayton1ScriptInstructionDescriptionProvider
{
    bool MapsInstructionType(int instructionType);
    bool MapsMethodName(string methodName);

    string GetMethodName(int instructionType);
    int GetInstructionType(string methodName);

    Layton1ScriptInstruction GetDescription(int instructionType);
}