using ImGui.Forms.Models;
using ImGui.Forms.Resources;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Resources;

class FontFactory : IFontFactory
{
    public void RegisterFonts()
    {
        ImGui.Forms.Factories.FontFactory.RegisterFromResource("NotoJp", "notojp.ttf", FontGlyphRange.ChineseJapanese | FontGlyphRange.Korean);
        ImGui.Forms.Factories.FontFactory.RegisterFromResource("Roboto", "roboto.ttf", FontGlyphRange.Latin);
    }

    public FontResource GetApplicationFont(int size)
    {
        FontResource notoFont = ImGui.Forms.Factories.FontFactory.Get("NotoJp", size);
        return ImGui.Forms.Factories.FontFactory.Get("Roboto", size, notoFont);
    }

    public FontResource GetHexadecimalFont(int size)
    {
        return ImGui.Forms.Factories.FontFactory.GetDefault(size);
    }
}