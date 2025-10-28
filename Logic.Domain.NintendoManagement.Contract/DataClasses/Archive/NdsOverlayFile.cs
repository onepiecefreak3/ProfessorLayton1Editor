namespace Logic.Domain.NintendoManagement.Contract.DataClasses.Archive;

public class NdsOverlayFile : NdsFile
{
    public required OverlayEntry Entry { get; init; }
}