using ImGui.Forms.Controls.Base;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;

namespace UI.Layton1Tool.Messages;

public record PuzzleScriptModifiedMessage(Component Source, Layton1NdsRom Rom, Layton1PuzzleId PuzzleId, GdsScriptFile Script);