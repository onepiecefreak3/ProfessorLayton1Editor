using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace UI.Layton1Tool.Messages;

public record SelectedNdsFileChangedMessage(Layton1NdsRom Rom, Layton1NdsFile File);