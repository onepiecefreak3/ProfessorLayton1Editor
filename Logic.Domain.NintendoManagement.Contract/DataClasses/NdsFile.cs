namespace Logic.Domain.NintendoManagement.Contract.DataClasses;

public class NdsFile
{
    public required Stream Stream { get; set; }
    public required string Path { get; set; }
}