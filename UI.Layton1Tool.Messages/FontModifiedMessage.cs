using ImGui.Forms.Controls.Base;
using Logic.Domain.NintendoManagement.Contract.DataClasses.Font;

namespace UI.Layton1Tool.Messages;

public record FontModifiedMessage(Component Source, NftrData Font);