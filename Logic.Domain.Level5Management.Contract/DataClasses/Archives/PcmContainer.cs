namespace Logic.Domain.Level5Management.Contract.DataClasses.Archives;

public class PcmContainer
{
    public required PcmHeader Header { get; set; }
    public required PcmEntry[] Entries { get; set; }
}