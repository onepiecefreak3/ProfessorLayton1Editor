using ImGui.Forms.Controls.Base;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using UI.Layton1Tool.Messages.DataClasses;

namespace UI.Layton1Tool.Messages;

public record SelectedEventViewTextChangedMessage(Component Target, Layton1NdsRom Rom, TextElement? Text, int TextIndex);