using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Domain.NintendoManagement.Contract.DataClasses.Archive;

namespace Logic.Business.Layton1ToolManagement.InternalContract.Compression;

interface ILayton1CompressionDetector
{
    CompressionType Detect(NdsRom rom, NdsFile file);
}