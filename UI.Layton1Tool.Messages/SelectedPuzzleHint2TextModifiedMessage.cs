using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace UI.Layton1Tool.Messages;

public record SelectedPuzzleHint2TextModifiedMessage(Layton1NdsRom Rom, Layton1PuzzleId Puzzle, string Hint2);