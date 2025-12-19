namespace Logic.Domain.Level5Management.Layton1.DataClasses.Script.Gds;

class GdsJumpTarget
{
    public required string Label { get; set; }
    public List<int> Offsets { get; set; } = [];
    public int ShiftedOffset { get; set; }
}