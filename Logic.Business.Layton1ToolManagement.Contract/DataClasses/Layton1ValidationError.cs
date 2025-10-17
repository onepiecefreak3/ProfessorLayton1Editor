using Logic.Business.Layton1ToolManagement.Contract.Enums;

namespace Logic.Business.Layton1ToolManagement.Contract.DataClasses;

public class Layton1ValidationError
{
    public required Layton1NdsFile File { get; init; }
    public required Layton1Error Error { get; init; }
    public FileType? FileType { get; init; }
    public required Exception Exception { get; init; }
}