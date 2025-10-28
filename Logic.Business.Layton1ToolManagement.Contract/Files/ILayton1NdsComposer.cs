using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace Logic.Business.Layton1ToolManagement.Contract.Files;

public interface ILayton1NdsComposer
{
    void Compose(Layton1NdsRom rom, Stream output);
}