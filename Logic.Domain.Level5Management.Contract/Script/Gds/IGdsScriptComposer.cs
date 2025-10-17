using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;

namespace Logic.Domain.Level5Management.Contract.Script.Gds;

public interface IGdsScriptComposer
{
    GdsArgument[] Compose(GdsScriptFile script);
}