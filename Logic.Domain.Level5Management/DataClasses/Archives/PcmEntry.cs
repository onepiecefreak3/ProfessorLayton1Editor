namespace Logic.Domain.Level5Management.DataClasses.Archives;

public class PcmEntry
{
    public int headerSize;
    public int sectionSize;
    public int zero0;
    public int fileSize;
    public string fileName;
    public Stream fileData;
}