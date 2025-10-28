using ImGui.Forms.Controls.Base;
using ImGui.Forms.Resources;

namespace UI.Layton1Tool.Messages;

public record AnimationFrameChangedMessage(Component Source, ImageResource Frame);