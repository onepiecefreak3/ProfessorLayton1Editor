using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Layton1;

namespace Logic.Business.Layton1ToolManagement.DataClasses.Scripts;

internal class CodeUnitBlock
{
    public int StartIndex { get; set; }
    public string? Label { get; set; }
    public List<StatementSyntax> Statements { get; set; } = [];
    public List<CodeUnitBlock> Parents { get; } = [];
    public List<CodeUnitBlock> Children { get; } = [];
}