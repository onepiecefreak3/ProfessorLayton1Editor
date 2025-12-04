using ImGui.Forms.Controls.Base;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums.Texts;
using UI.Layton1Tool.Messages.DataClasses;

namespace UI.Layton1Tool.Messages;

public record RoomScriptUpdatedMessage(Component Target, Layton1NdsRom Rom, int Room, TextLanguage Language, GdsScriptFile Script, RoomFlagStates States);