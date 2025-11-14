using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace UI.Layton1Tool.Messages;

public record PuzzleIdModifiedMessage(Layton1NdsRom Rom, Layton1PuzzleId PuzzleId);