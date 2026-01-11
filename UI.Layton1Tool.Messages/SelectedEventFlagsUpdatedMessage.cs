using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace UI.Layton1Tool.Messages;

public record SelectedEventFlagsUpdatedMessage(Layton1NdsRom Rom, int Event, GameState States);