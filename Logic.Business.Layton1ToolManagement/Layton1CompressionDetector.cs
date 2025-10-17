using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.InternalContract;
using Logic.Domain.Level5Management.Contract;
using Logic.Domain.NintendoManagement.Contract;
using Logic.Domain.NintendoManagement.Contract.DataClasses;
using NintendoCompressionMethod = Logic.Domain.NintendoManagement.Contract.Enums.CompressionMethod;
using Level5CompressionMethod = Logic.Domain.Level5Management.Contract.Enums.CompressionMethod;

namespace Logic.Business.Layton1ToolManagement;

class Layton1CompressionDetector(INintendoDecompressor nintendoDecompressor, ILevel5Decompressor level5Decompressor) : ILayton1CompressionDetector
{
    public CompressionType Detect(NdsFile file)
    {
        string extension = Path.GetExtension(file.Path);
        switch (extension)
        {
            case ".arc":
            case ".arj":
                return TryGetLevel5CompressionType(file);

            case ".pcm":
                return TryGetNintendoCompressionType(file);
        }

        return CompressionType.None;
    }

    private CompressionType TryGetLevel5CompressionType(NdsFile file)
    {
        try
        {
            Level5CompressionMethod level5Comp = level5Decompressor.PeekCompressionMethod(file.Stream);
            return GetCompressionType(level5Comp);
        }
        catch (Exception)
        {
            return CompressionType.None;
        }
    }

    private static CompressionType GetCompressionType(Level5CompressionMethod compression)
    {
        return compression switch
        {
            Level5CompressionMethod.Rle => CompressionType.Level5Rle,
            Level5CompressionMethod.Lz10 => CompressionType.Level5Lz10,
            Level5CompressionMethod.Huffman4 => CompressionType.Level5Huffman4Bit,
            Level5CompressionMethod.Huffman8 => CompressionType.Level5Huffman8Bit,
            _ => throw new InvalidOperationException($"Unsupported Level5 compression {compression}.")
        };
    }

    private CompressionType TryGetNintendoCompressionType(NdsFile file)
    {
        try
        {
            NintendoCompressionMethod nintendoComp = nintendoDecompressor.PeekCompressionMethod(file.Stream);
            return GetCompressionType(nintendoComp);
        }
        catch (Exception)
        {
            return CompressionType.None;
        }
    }

    private static CompressionType GetCompressionType(NintendoCompressionMethod compression)
    {
        return compression switch
        {
            NintendoCompressionMethod.Lz10 => CompressionType.NintendoLz10,
            NintendoCompressionMethod.Lz11 => CompressionType.NintendoLz11,
            NintendoCompressionMethod.Huffman4 => CompressionType.NintendoHuffman4Bit,
            NintendoCompressionMethod.Huffman8 => CompressionType.NintendoHuffman8Bit,
            NintendoCompressionMethod.Rle => CompressionType.NintendoRle,
            _ => throw new InvalidOperationException($"Unsupported Nintendo compression {compression}.")
        };
    }
}