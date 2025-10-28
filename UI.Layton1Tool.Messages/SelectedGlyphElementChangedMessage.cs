using ImGui.Forms.Controls.Base;

namespace UI.Layton1Tool.Messages;

public record SelectedGlyphElementChangedMessage(Component Source, bool IsSelected);