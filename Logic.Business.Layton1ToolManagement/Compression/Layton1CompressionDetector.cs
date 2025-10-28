using Logic.Business.Layton1ToolManagement.InternalContract.Compression;
using Logic.Domain.Level5Management.Contract;
using Logic.Domain.NintendoManagement.Contract;
using Logic.Domain.NintendoManagement.Contract.DataClasses.Archive;
using Logic.Domain.NintendoManagement.Contract.Enums;
using CompressionType = Logic.Business.Layton1ToolManagement.Contract.Enums.CompressionType;

namespace Logic.Business.Layton1ToolManagement.Compression;

class Layton1CompressionDetector(
    INdsCompressionDetector ndsDetector,
    INintendoDecompressor nintendoDecompressor,
    ILevel5Decompressor level5Decompressor)
    : ILayton1CompressionDetector
{
    public CompressionType Detect(NdsRom rom, NdsFile file)
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

        NdsCompressionType ndsCompression = ndsDetector.Detect(rom, file);
        return ndsCompression switch
        {
            NdsCompressionType.None => CompressionType.None,
            NdsCompressionType.Overlay => CompressionType.NintendoOverlay,
            _ => throw new InvalidOperationException($"Unsupported Nintendo compression {ndsCompression}.")
        };
    }

    private CompressionType TryGetLevel5CompressionType(NdsFile file)
    {
        try
        {
            Domain.Level5Management.Contract.Enums.CompressionType level5Comp = level5Decompressor.PeekCompressionMethod(file.Stream);
            return GetCompressionType(level5Comp);
        }
        catch (Exception)
        {
            return CompressionType.None;
        }
    }

    private static CompressionType GetCompressionType(Domain.Level5Management.Contract.Enums.CompressionType compression)
    {
        return compression switch
        {
            Domain.Level5Management.Contract.Enums.CompressionType.Rle => CompressionType.Level5Rle,
            Domain.Level5Management.Contract.Enums.CompressionType.Lz10 => CompressionType.Level5Lz10,
            Domain.Level5Management.Contract.Enums.CompressionType.Huffman4 => CompressionType.Level5Huffman4Bit,
            Domain.Level5Management.Contract.Enums.CompressionType.Huffman8 => CompressionType.Level5Huffman8Bit,
            _ => throw new InvalidOperationException($"Unsupported Level5 compression {compression}.")
        };
    }

    private CompressionType TryGetNintendoCompressionType(NdsFile file)
    {
        try
        {
            Domain.NintendoManagement.Contract.Enums.CompressionType nintendoComp = nintendoDecompressor.PeekCompressionMethod(file.Stream);
            return GetCompressionType(nintendoComp);
        }
        catch (Exception)
        {
            return CompressionType.None;
        }
    }

    private static CompressionType GetCompressionType(Domain.NintendoManagement.Contract.Enums.CompressionType compression)
    {
        return compression switch
        {
            Domain.NintendoManagement.Contract.Enums.CompressionType.Lz10 => CompressionType.NintendoLz10,
            Domain.NintendoManagement.Contract.Enums.CompressionType.Lz11 => CompressionType.NintendoLz11,
            Domain.NintendoManagement.Contract.Enums.CompressionType.Huffman4 => CompressionType.NintendoHuffman4Bit,
            Domain.NintendoManagement.Contract.Enums.CompressionType.Huffman8 => CompressionType.NintendoHuffman8Bit,
            Domain.NintendoManagement.Contract.Enums.CompressionType.Rle => CompressionType.NintendoRle,
            _ => throw new InvalidOperationException($"Unsupported Nintendo compression {compression}.")
        };
    }
}