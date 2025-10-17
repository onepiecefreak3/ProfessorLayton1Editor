using ImGui.Forms.Resources;

namespace UI.Layton1Tool.Resources.Contract;

public interface IFontFactory
{
    void RegisterFonts();

    FontResource GetDefaultFont();
}