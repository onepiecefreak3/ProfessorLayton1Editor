using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace UI.Layton1Tool.Messages;

public record SelectedEventFlagsModifiedMessage(Layton1NdsRom Rom, int Event);