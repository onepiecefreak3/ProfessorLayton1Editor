using ImGui.Forms;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Resources;

class ColorProvider : IColorProvider
{
    public ThemedColor Default => new(Color.Transparent, Color.Transparent);
    public ThemedColor Changed => new(new Rgba32(0xFF, 0xA5, 0x00), new Rgba32(0xFF, 0xA5, 0x00));
    public ThemedColor Error => new(Color.DarkRed, new Rgba32(0xcf, 0x66, 0x79));
    public ThemedColor Progress => Color.ForestGreen;
}