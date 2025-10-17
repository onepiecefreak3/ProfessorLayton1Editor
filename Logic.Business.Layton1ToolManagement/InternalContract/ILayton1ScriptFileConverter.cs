using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Level5;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;

namespace Logic.Business.Layton1ToolManagement.InternalContract;

interface ILayton1ScriptFileConverter
{
    CodeUnitSyntax CreateCodeUnit(GdsScriptFile script);
}