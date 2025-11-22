using ImGui.Forms.Models;
using ImGui.Forms.Resources;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Resources;

class FontFactory : IFontFactory
{
    public void RegisterFonts()
    {
        ImGui.Forms.Factories.FontFactory.RegisterFromResource("NotoJp", "notojp.ttf", FontGlyphRange.ChineseJapanese);
        ImGui.Forms.Factories.FontFactory.RegisterFromResource("NotoKo", "notoko.ttf", FontGlyphRange.Korean);
        ImGui.Forms.Factories.FontFactory.RegisterFromResource("Roboto", "roboto.ttf", FontGlyphRange.Latin);
    }

    public FontResource GetApplicationFont(int size)
    {
        FontResource notoJpFont = ImGui.Forms.Factories.FontFactory.Get("NotoJp", size);
        FontResource notoKoFont = ImGui.Forms.Factories.FontFactory.Get("NotoKo", size, notoJpFont);
        return ImGui.Forms.Factories.FontFactory.Get("Roboto", size, notoKoFont);
    }

    public FontResource GetHexadecimalFont(int size)
    {
        return ImGui.Forms.Factories.FontFactory.GetDefault(size);
    }
}