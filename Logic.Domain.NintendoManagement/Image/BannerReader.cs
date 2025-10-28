using System.Text;
using Kanvas.Encoding;
using Kanvas;
using Kanvas.Swizzle;
using Komponent.Contract.Enums;
using Komponent.IO;
using Konnect.Contract.DataClasses.Plugin.File.Image;
using Konnect.Plugin.File.Image;
using Logic.Domain.NintendoManagement.Contract.DataClasses.Image;
using Logic.Domain.NintendoManagement.Contract.Image;
using Logic.Domain.NintendoManagement.DataClasses.Image;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Logic.Domain.NintendoManagement.Image;

class BannerReader : IBannerReader
{
    public BannerData Read(Stream input)
    {
        using var reader = new BinaryReaderX(input, Encoding.Unicode, true);

        BannerHeader header = ReadHeader(reader);
        reader.SeekAlignment(0x20);

        Image<Rgba32> image = ReadImage(reader);
        string[] titles = ReadTitles(reader, 6);

        return new BannerData
        {
            Version = header.verison,
            Image = image,
            Titles = titles
        };
    }

    private static BannerHeader ReadHeader(BinaryReaderX reader)
    {
        return new BannerHeader
        {
            verison = reader.ReadInt16(),
            crc16 = reader.ReadUInt16()
        };
    }

    private static Image<Rgba32> ReadImage(BinaryReaderX reader)
    {
        byte[] imageData = reader.ReadBytes(0x200);
        byte[] paletteData = reader.ReadBytes(0x20);

        return ParseImage(imageData, paletteData);
    }

    private static string[] ReadTitles(BinaryReaderX reader, int count)
    {
        var result = new string[count];

        for (var i = 0; i < count; i++)
            result[i] = reader.ReadString(256).TrimEnd('\0');

        return result;
    }

    private static Image<Rgba32> ParseImage(byte[] imageData, byte[] paletteData)
    {
        var definition = new EncodingDefinition();
        definition.AddPaletteEncoding(0, new Rgba(5, 5, 5, "BGR"));
        definition.AddIndexEncoding(0, ImageFormats.I4(BitOrder.LeastSignificantBitFirst), [0]);

        var image = new ImageFile(new ImageFileInfo
        {
            ImageData = imageData,
            PaletteData = paletteData,
            BitDepth = 4,
            PaletteBitDepth = 16,
            ImageFormat = 0,
            PaletteFormat = 0,
            ImageSize = new Size(32, 32),
            RemapPixels = context => new NitroSwizzle(context),
            Quantize = context => context.WithColorCount(16)
        }, definition);

        return image.GetImage();
    }
}