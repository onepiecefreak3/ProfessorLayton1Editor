namespace UI.Layton1Tool.Messages.DataClasses;

public class RoomFlagStates
{
    public required IDictionary<int, bool> Flags1 { get; init; }
    public required IDictionary<int, bool> Flags2 { get; init; }
    public required int State { get; set; }
}