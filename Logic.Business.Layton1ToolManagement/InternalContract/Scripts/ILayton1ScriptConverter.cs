using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Level5;

namespace Logic.Business.Layton1ToolManagement.InternalContract.Scripts;

interface ILayton1ScriptConverter
{
    CodeUnitSyntax Parse(Stream input, string gameCode);
    Stream Compose(CodeUnitSyntax syntax, string gameCode);
}