using ImGui.Forms;

namespace UI.Layton1Tool.Resources.Contract;

public interface IColorProvider
{
    ThemedColor Default { get; }
    ThemedColor Changed { get; }
    ThemedColor Error { get; }
    ThemedColor Progress { get; }
}