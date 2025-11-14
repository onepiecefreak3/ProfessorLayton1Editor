using ImGui.Forms.Controls.Base;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;

namespace UI.Layton1Tool.Messages;

public record PuzzleScriptModifiedMessage(Component Source, GdsScriptFile Script);