namespace Logic.Business.Layton1ToolManagement.Contract.DataClasses;

public class Layton1PuzzleId
{
    public required int InternalId { get; set; }
    public required int Number { get; set; }
    public required bool IsWifi { get; set; }
    public DateTimeOffset? ReleaseDate { get; set; }
}