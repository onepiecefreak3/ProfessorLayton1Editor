using ImGui.Forms.Controls.Base;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace UI.Layton1Tool.Messages;

public record PuzzleScriptUpdatedMessage(Component Target, Layton1NdsRom Rom, GdsScriptFile Script);