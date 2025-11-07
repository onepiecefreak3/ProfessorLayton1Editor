namespace Logic.Domain.Level5Management.Contract.DataClasses.Archives;

public class PcmFile
{
    public required string Name { get; set; }
    public required Stream Data { get; set; }
}