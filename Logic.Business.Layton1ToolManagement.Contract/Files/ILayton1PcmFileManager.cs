using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Domain.Level5Management.Contract.DataClasses.Archives;
using System.Diagnostics.CodeAnalysis;

namespace Logic.Business.Layton1ToolManagement.Contract.Files;

public interface ILayton1PcmFileManager
{
    bool TryGet(List<PcmFile> files, string name, [NotNullWhen(true)] out PcmFile? file);
    PcmFile Add(List<PcmFile> files, string name, object content, FileType type);

    FileType Detect(PcmFile file);

    object? Parse(PcmFile file, FileType type);
    void Compose(PcmFile file, object content, FileType type);
}