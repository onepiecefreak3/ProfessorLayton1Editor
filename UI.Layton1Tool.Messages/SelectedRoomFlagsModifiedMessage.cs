using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace UI.Layton1Tool.Messages;

public record SelectedRoomFlagsModifiedMessage(Layton1NdsRom Rom, int Room);