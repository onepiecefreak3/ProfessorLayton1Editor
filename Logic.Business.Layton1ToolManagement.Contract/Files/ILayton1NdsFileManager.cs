using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums;

namespace Logic.Business.Layton1ToolManagement.Contract.Files;

public interface ILayton1NdsFileManager
{
    void Compress(Layton1NdsFile file);
    void Decompress(Layton1NdsFile file);

    Stream GetUncompressedStream(Layton1NdsFile file);

    object? Parse(Layton1NdsFile file, out FileType type);
    object? Parse(Layton1NdsFile file, FileType type);
    void Compose(Layton1NdsFile file, object content);

    FileType Detect(Layton1NdsFile file);
}