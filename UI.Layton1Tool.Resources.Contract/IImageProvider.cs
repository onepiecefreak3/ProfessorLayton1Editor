using ImGui.Forms.Resources;

namespace UI.Layton1Tool.Resources.Contract;

public interface IImageProvider
{
    ThemedImageResource SearchClear { get; }
    ThemedImageResource Save { get; }
    ThemedImageResource SaveAs { get; }
}