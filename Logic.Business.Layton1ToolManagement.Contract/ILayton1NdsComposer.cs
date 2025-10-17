using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace Logic.Business.Layton1ToolManagement.Contract;

public interface ILayton1NdsComposer
{
    void Compose(Layton1NdsRom rom, Stream output);
}