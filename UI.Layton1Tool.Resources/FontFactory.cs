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

    public FontResource GetDefaultFont()
    {
        FontResource notoFont = ImGui.Forms.Factories.FontFactory.Get("NotoJp", 15);
        return ImGui.Forms.Factories.FontFactory.Get("Roboto", 15, notoFont);
    }
}