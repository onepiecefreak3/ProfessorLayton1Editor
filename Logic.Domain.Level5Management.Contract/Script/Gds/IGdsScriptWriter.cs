using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;

namespace Logic.Domain.Level5Management.Contract.Script.Gds;

public interface IGdsScriptWriter
{
    void Write(GdsScriptFile script, Stream output);

    void Write(GdsArgument[] arguments, Stream output);
}