using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace UI.Layton1Tool.Forms.Contract.DataClasses;

public class Layton1NdsInfo
{
    public required string Path { get; set; }
    public required Layton1NdsRom Rom { get; set; }
}