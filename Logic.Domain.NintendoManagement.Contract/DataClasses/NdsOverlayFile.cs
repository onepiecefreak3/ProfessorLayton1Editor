namespace Logic.Domain.NintendoManagement.Contract.DataClasses;

public class NdsOverlayFile : NdsFile
{
    public required OverlayEntry Entry { get; init; }
}