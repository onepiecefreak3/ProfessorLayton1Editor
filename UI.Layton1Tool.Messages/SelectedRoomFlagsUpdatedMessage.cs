using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace UI.Layton1Tool.Messages;

public record SelectedRoomFlagsUpdatedMessage(Layton1NdsRom Rom, int Room, GameState States);