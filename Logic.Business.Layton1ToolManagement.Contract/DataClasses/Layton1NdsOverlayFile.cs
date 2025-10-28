using Logic.Domain.NintendoManagement.Contract.DataClasses.Archive;

namespace Logic.Business.Layton1ToolManagement.Contract.DataClasses;

public class Layton1NdsOverlayFile : Layton1NdsFile
{
    public required OverlayEntry Entry { get; init; }
}