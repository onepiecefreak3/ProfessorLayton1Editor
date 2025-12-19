using Logic.Business.Layton1ToolManagement.DataClasses.Scripts;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;

namespace Logic.Business.Layton1ToolManagement.InternalContract.Scripts;

internal interface ILayton1GdsFileBlockParser
{
    GdsFileBlock Parse(GdsScriptFile script);
}