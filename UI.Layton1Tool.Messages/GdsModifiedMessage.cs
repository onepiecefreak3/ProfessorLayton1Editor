using ImGui.Forms.Controls.Base;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Level5;

namespace UI.Layton1Tool.Messages;

public record GdsModifiedMessage(Component Source, CodeUnitSyntax Script);