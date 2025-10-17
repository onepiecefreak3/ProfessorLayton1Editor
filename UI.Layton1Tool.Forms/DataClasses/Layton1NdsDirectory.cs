using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace UI.Layton1Tool.Forms.DataClasses;

class Layton1NdsDirectory
{
    public required string Name { get; set; }
    public IList<Layton1NdsDirectory> Directories { get; } = [];
    public IList<Layton1NdsFile> Files { get; } = [];
}