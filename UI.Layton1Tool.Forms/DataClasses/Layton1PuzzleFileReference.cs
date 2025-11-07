using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Domain.Level5Management.Contract.DataClasses.Archives;

namespace UI.Layton1Tool.Forms.DataClasses;

internal class Layton1PuzzleFileReference
{
    public required Layton1NdsFile File { get; set; }
    public required object Content { get; set; }
    public PcmFile? PcmFile { get; set; }
}