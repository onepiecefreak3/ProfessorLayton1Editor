using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Level5;

namespace Logic.Business.Layton1ToolManagement.Contract.Scripts;

public interface ILayton1ScriptInstructionManager
{
    Layton1ScriptInstruction? GetInstruction(MethodInvocationStatementSyntax invocation, string gameCode);
}