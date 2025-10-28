using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Domain.Level5Management.Contract.DataClasses.Archives;

namespace Logic.Business.Layton1ToolManagement.Contract.Files;

public interface ILayton1PcmFileManager
{
    FileType Detect(PcmFile file);
    object? Parse(PcmFile file, FileType type);
}