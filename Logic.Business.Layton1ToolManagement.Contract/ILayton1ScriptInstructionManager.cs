using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Level5;

namespace Logic.Business.Layton1ToolManagement.Contract;

public interface ILayton1ScriptInstructionManager
{
    Layton1ScriptInstruction? GetInstruction(MethodInvocationStatementSyntax invocation);
}