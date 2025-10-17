using Logic.Domain.Level5Management.Contract.DataClasses.Images;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Logic.Domain.Level5Management.Contract.Images;

public interface IBgxParser
{
    Image<Rgba32> Parse(Stream input);
    Image<Rgba32> Parse(BgxContainer container);
}