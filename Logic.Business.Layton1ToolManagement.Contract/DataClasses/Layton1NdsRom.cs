using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Domain.NintendoManagement.Contract.DataClasses.Archive;

namespace Logic.Business.Layton1ToolManagement.Contract.DataClasses;

public class Layton1NdsRom
{
    public required string GameCode { get; init; }
    public required Region Region { get; init; }
    public Layton1NdsFile[] Files { get; set; } = [];

    public NdsHeader? DsHeader { get; init; }
    public DsiHeader? DsiHeader { get; init; }
    public Arm9Footer? Footer { get; init; }
}