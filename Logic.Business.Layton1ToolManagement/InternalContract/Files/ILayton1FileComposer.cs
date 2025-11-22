using Logic.Business.Layton1ToolManagement.Contract.Enums;

namespace Logic.Business.Layton1ToolManagement.InternalContract.Files;

interface ILayton1FileComposer
{
    Stream? Compose(object content, FileType type, GameVersion version);
}