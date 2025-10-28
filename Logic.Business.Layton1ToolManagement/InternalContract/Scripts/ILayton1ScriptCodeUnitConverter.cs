using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Level5;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;

namespace Logic.Business.Layton1ToolManagement.InternalContract.Scripts;

interface ILayton1ScriptCodeUnitConverter
{
    GdsScriptFile CreateScriptFile(CodeUnitSyntax tree, string gameCode);
}