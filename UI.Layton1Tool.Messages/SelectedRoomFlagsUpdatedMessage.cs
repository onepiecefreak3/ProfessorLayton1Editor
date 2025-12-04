using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using UI.Layton1Tool.Messages.DataClasses;

namespace UI.Layton1Tool.Messages;

public record SelectedRoomFlagsUpdatedMessage(Layton1NdsRom Rom, int Room, RoomFlagStates States);