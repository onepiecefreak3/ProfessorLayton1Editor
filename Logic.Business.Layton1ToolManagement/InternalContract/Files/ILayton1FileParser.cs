using Logic.Business.Layton1ToolManagement.Contract.Enums;

namespace Logic.Business.Layton1ToolManagement.InternalContract.Files;

interface ILayton1FileParser
{
    object? Parse(Stream input, FileType type, GameVersion version);
}