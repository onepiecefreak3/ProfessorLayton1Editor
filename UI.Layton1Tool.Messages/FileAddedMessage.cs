using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using ImGui.Forms.Controls.Base;

namespace UI.Layton1Tool.Messages;

public record FileAddedMessage(Component Source, Layton1NdsFile File);