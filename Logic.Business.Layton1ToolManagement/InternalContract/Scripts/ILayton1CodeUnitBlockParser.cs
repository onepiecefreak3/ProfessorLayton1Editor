using Logic.Business.Layton1ToolManagement.DataClasses.Scripts;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Layton1;

namespace Logic.Business.Layton1ToolManagement.InternalContract.Scripts;

internal interface ILayton1CodeUnitBlockParser
{
    CodeUnitBlock Parse(IReadOnlyList<StatementSyntax> statements);
}