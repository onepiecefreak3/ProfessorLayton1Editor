using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Level5;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;

namespace Logic.Business.Layton1ToolManagement.Contract.Scripts;

public interface ILayton1ScriptFileConverter
{
    CodeUnitSyntax CreateCodeUnit(GdsScriptFile script, string gameCode);
}