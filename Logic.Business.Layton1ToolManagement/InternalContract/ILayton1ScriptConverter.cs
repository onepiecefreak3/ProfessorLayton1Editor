using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Level5;

namespace Logic.Business.Layton1ToolManagement.InternalContract;

interface ILayton1ScriptConverter
{
    CodeUnitSyntax Parse(Stream input);
    Stream Compose(CodeUnitSyntax syntax);
}