namespace Logic.Domain.NintendoManagement.Contract.DataClasses.Archive;

public class NdsRom
{
    public required string GameCode { get; set; }
    public required NdsFile[] Files { get; set; }

    public NdsHeader? DsHeader { get; init; }
    public DsiHeader? DsiHeader { get; init; }
    public Arm9Footer? Footer { get; init; }
}