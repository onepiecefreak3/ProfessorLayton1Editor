namespace Logic.Domain.Level5Management.Contract.DataClasses.Archives;

public class PcmFile
{
    public required string FileName { get; set; }
    public required Stream FileData { get; set; }
}