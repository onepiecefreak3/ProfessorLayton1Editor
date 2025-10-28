using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.Contract.Files;
using Logic.Business.Layton1ToolManagement.InternalContract.Files;
using Logic.Domain.Level5Management.Contract.DataClasses.Archives;

namespace Logic.Business.Layton1ToolManagement.Files;

class Layton1PcmFileManager(ILayton1FileTypeDetector detector, ILayton1FileParser fileParser) : ILayton1PcmFileManager
{
    public FileType Detect(PcmFile file)
    {
        return detector.Detect(file);
    }

    public object? Parse(PcmFile file, FileType type)
    {
        return fileParser.Parse(file.FileData, type, string.Empty);
    }
}