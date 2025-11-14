using ImGui.Forms.Controls.Base;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace UI.Layton1Tool.Messages;

public record SelectedFileChangedMessage(Component? Target, Layton1NdsFile File, object? Content);