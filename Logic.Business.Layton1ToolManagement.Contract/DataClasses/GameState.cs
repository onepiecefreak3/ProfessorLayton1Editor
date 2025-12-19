namespace Logic.Business.Layton1ToolManagement.Contract.DataClasses;

public class GameState
{
    public bool IsScriptReturn { get; set; }
    public bool IsScriptSolved { get; set; }
    public bool ReceivedUserInput { get; set; } = true;
    public int State { get; set; }
    public int DialogIndex { get; set; }
    public int SolvedCount { get; set; }
    public Dictionary<int, (bool Seen, bool Solved, bool FinalSolved)> Puzzles { get; set; } = [];
    public Dictionary<int, bool> ByteFlags { get; set; } = [];
    public Dictionary<int, bool> BitFlags { get; set; } = [];

    public int ActivePuzzleId { get; set; }
}