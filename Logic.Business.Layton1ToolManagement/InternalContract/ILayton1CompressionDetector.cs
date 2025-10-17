using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Domain.NintendoManagement.Contract.DataClasses;

namespace Logic.Business.Layton1ToolManagement.InternalContract;

interface ILayton1CompressionDetector
{
    CompressionType Detect(NdsFile file);
}