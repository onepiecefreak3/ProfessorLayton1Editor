using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Logic.Domain.NintendoManagement.Contract.DataClasses.Image;

public class BannerData
{
    public required int Version { get; set; }
    public required Image<Rgba32> Image { get; set; }
    public required string[] Titles { get; set; }
}