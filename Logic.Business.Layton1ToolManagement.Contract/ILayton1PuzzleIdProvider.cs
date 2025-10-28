using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace Logic.Business.Layton1ToolManagement.Contract;

public interface ILayton1PuzzleIdProvider
{
    int[] Get(Layton1NdsRom ndsRom);
}