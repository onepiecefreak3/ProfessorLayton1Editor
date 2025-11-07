using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace Logic.Business.Layton1ToolManagement.Contract;

public interface ILayton1PuzzleIdProvider
{
    int MaxPuzzleSlots { get; }

    Layton1PuzzleId[] Get(Layton1NdsRom ndsRom);
    Layton1PuzzleId[] GetWifi(Layton1NdsRom ndsRom);

    void Set(Layton1NdsRom ndsRom, Layton1PuzzleId puzzleId);
}