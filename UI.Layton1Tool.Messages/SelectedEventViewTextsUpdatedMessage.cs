using ImGui.Forms.Controls.Base;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using UI.Layton1Tool.Messages.DataClasses;

namespace UI.Layton1Tool.Messages;

public record SelectedEventViewTextsUpdatedMessage(Component Source, Layton1NdsRom Rom, TextElement[] Texts, bool KeepIndex);