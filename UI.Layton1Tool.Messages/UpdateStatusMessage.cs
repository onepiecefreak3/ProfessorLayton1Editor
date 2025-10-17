using ImGui.Forms.Localization;
using UI.Layton1Tool.Messages.Enums;

namespace UI.Layton1Tool.Messages;

public record UpdateStatusMessage(LocalizedString Text, Status Status, Exception? Exception);
