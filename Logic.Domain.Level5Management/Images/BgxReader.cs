using Komponent.IO;
using Logic.Domain.Level5Management.Contract.DataClasses.Images;
using Logic.Domain.Level5Management.Contract.Images;

namespace Logic.Domain.Level5Management.Images;

class BgxReader : IBgxReader
{
    public BgxContainer Read(Stream input)
    {
        using var reader = new BinaryReaderX(input, true);

        var colorCount = reader.ReadInt32();
        var paletteData = reader.ReadBytes(colorCount * 2);

        var tileCount = reader.ReadInt32();
        var tileData = reader.ReadBytes(tileCount * 0x40);

        var widthTileCount = reader.ReadInt16();
        var heightTileCount = reader.ReadInt16();

        var width = widthTileCount * 8;
        var height = heightTileCount * 8;

        var tileIndexes = new short[widthTileCount * heightTileCount];
        for (var i = 0; i < widthTileCount * heightTileCount; i++)
        {
            var tileIndex = reader.ReadInt16();
            tileIndexes[i] = tileIndex;
        }

        return new BgxContainer
        {
            PaletteData = paletteData,
            TileData = tileData,
            TileIndexes = tileIndexes,
            Width = width,
            Height = height
        };
    }
}