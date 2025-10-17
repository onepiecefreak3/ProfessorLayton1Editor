using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Domain.Level5Management.Contract.DataClasses.Archives;

namespace Logic.Business.Layton1ToolManagement.InternalContract;

interface ILayton1FileTypeDetector
{
    FileType Detect(Layton1NdsFile file);

    FileType Detect(PcmFile file);
}