using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace UI.Layton1Tool.Messages.DataClasses;

public class PaddedGlyph
{
    public Image<Rgba32>? Glyph { get; set; }
    public Size BoundingBox { get; set; }
    public Point GlyphPosition { get; set; }
    public int Baseline { get; set; }
}