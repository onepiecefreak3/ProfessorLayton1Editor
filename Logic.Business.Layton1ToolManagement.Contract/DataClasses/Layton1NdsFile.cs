using Logic.Business.Layton1ToolManagement.Contract.Enums;

namespace Logic.Business.Layton1ToolManagement.Contract.DataClasses;

public class Layton1NdsFile
{
    public required CompressionType CompressionType { get; set; }
    public required Stream DataStream { get; set; }
    public required string Path { get; set; }
    public bool IsChanged { get; set; }

    public Stream? DecompressedStream { get; set; }
}