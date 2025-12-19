namespace Logic.Business.Layton1ToolManagement.DataClasses.Scripts;

internal class GdsFileBlock
{
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
    public List<GdsFileBlock> Parents { get; } = [];
    public List<GdsFileBlock> Children { get; } = [];
}