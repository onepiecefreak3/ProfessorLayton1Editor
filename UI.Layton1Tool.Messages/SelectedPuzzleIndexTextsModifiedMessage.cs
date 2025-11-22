using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace UI.Layton1Tool.Messages;

public record SelectedPuzzleIndexTextsModifiedMessage(Layton1NdsRom Rom, Layton1PuzzleId Puzzle, string Title, string Type, string Location);