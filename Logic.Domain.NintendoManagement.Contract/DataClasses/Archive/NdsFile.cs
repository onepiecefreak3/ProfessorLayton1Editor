namespace Logic.Domain.NintendoManagement.Contract.DataClasses.Archive;

public class NdsFile
{
    public required Stream Stream { get; set; }
    public required string Path { get; set; }
}