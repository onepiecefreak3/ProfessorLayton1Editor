using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums.Texts;

namespace UI.Layton1Tool.Messages;

public record SelectedPuzzleChangedMessage(Layton1NdsRom Rom, Layton1PuzzleId Puzzle, TextLanguage Language);