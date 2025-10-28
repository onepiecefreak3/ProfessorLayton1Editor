using ImGui.Forms.Controls.Base;
using UI.Layton1Tool.Messages.DataClasses;

namespace UI.Layton1Tool.Messages;

public record SelectedPaddedGlyphChangedMessage(Component Target, PaddedGlyph? PaddedGlyph);