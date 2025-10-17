using Kanvas;
using Kanvas.Encoding;
using Kanvas.Swizzle;
using Konnect.Contract.DataClasses.Plugin.File.Image;
using Konnect.Plugin.File.Image;
using Logic.Domain.Level5Management.Contract.Images;
using Logic.Domain.Level5Management.Contract.DataClasses.Images;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Logic.Domain.Level5Management.Images;

class BgxParser(IBgxReader reader) : IBgxParser
{
    public Image<Rgba32> Parse(Stream input)
    {
        BgxContainer container = reader.Read(input);

        return Parse(container);
    }

    public Image<Rgba32> Parse(BgxContainer container)
    {
        var imageData = new byte[container.Width * container.Height];
        for (var i = 0; i < container.TileIndexes.Length; i++)
            Array.Copy(container.TileData, container.TileIndexes[i] * 0x40, imageData, i * 0x40, 0x40);

        var definition = new EncodingDefinition();
        definition.AddPaletteEncoding(0, new Rgba(5, 5, 5, "BGR"));
        definition.AddIndexEncoding(0, ImageFormats.I8(), [0]);

        var imageFile = new ImageFile(new ImageFileInfo
        {
            ImageData = imageData,
            PaletteData = container.PaletteData,
            BitDepth = 8,
            PaletteBitDepth = 16,
            ImageFormat = 0,
            PaletteFormat = 0,
            ImageSize = new Size(container.Width, container.Height),
            Quantize = context => context.WithColorCount(256),
            RemapPixels = options => new NitroSwizzle(options)
        }, definition);

        return imageFile.GetImage();
    }
}