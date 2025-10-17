using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;

namespace Logic.Domain.Level5Management.Contract.Script.Gds;

public interface IGdsScriptParser
{
    GdsScriptFile Parse(Stream input);
    GdsScriptFile Parse(GdsArgument[] arguments);
}