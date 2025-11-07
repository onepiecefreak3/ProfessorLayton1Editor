using ImGui.Forms.Controls.Base;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace UI.Layton1Tool.Messages;

public record FileContentModifiedMessage(Component Source, Layton1NdsFile File, object? Content);
