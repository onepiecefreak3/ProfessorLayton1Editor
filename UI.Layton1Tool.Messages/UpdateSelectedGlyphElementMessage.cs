using ImGui.Forms.Controls.Base;

namespace UI.Layton1Tool.Messages;

public record UpdateSelectedGlyphElementMessage(Component Target, bool IsSelected);