using Logic.Business.Layton1ToolManagement.Contract.Enums;

namespace Logic.Business.Layton1ToolManagement.InternalContract;

interface ILayton1FileParser
{
    object? Parse(Stream input, FileType type);
}