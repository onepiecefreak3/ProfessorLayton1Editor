using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;

namespace Logic.Business.Layton1ToolManagement.Contract.Scripts;

public interface ILayton1ScriptReducer
{
    GdsScriptInstruction[] Reduce(GdsScriptFile script, GameState state);
}