using Logic.Business.Layton1ToolManagement.InternalContract;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Domain.Level5Management.Contract;
using Logic.Domain.NintendoManagement.Contract;
using NintendoCompressionMethod = Logic.Domain.NintendoManagement.Contract.Enums.CompressionMethod;
using Level5CompressionMethod = Logic.Domain.Level5Management.Contract.Enums.CompressionMethod;

namespace Logic.Business.Layton1ToolManagement;

class Layton1Compressor(
    INintendoDecompressor nintendoDecompressor,
    ILevel5Decompressor level5Decompressor)
    : ILayton1Compressor
{
    public void Compress(Layton1NdsFile file)
    {
        file.DataStream.Position = 0;

        if (file.CompressionType is CompressionType.None)
            return;

        if (file.DecompressedStream is null)
            return;

        file.DecompressedStream.Position = 0;

        var output = new MemoryStream();
        switch (file.CompressionType)
        {
            case CompressionType.Level5Lz10:
            case CompressionType.Level5Huffman4Bit:
            case CompressionType.Level5Huffman8Bit:
            case CompressionType.Level5Rle:
                level5Decompressor.Compress(file.DecompressedStream, output, GetLevel5CompressionType(file.CompressionType));
                break;

            case CompressionType.NintendoLz10:
            case CompressionType.NintendoLz11:
            case CompressionType.NintendoHuffman4Bit:
            case CompressionType.NintendoHuffman8Bit:
            case CompressionType.NintendoRle:
                nintendoDecompressor.Compress(file.DecompressedStream, output, GetNintendoCompressionType(file.CompressionType));
                break;

            default:
                throw new InvalidOperationException($"Invalid compression type {file.CompressionType}.");
        }

        file.DataStream = output;

        file.DataStream.Position = 0;
        file.DecompressedStream.Position = 0;
    }

    public void Decompress(Layton1NdsFile file)
    {
        file.DataStream.Position = 0;

        if (file.CompressionType is CompressionType.None)
            return;

        if (file.DecompressedStream is not null)
        {
            file.DecompressedStream.Position = 0;
            return;
        }

        var output = new MemoryStream();
        switch (file.CompressionType)
        {
            case CompressionType.Level5Lz10:
            case CompressionType.Level5Huffman4Bit:
            case CompressionType.Level5Huffman8Bit:
            case CompressionType.Level5Rle:
                level5Decompressor.Decompress(file.DataStream, output);
                break;

            case CompressionType.NintendoLz10:
            case CompressionType.NintendoLz11:
            case CompressionType.NintendoHuffman4Bit:
            case CompressionType.NintendoHuffman8Bit:
            case CompressionType.NintendoRle:
                nintendoDecompressor.Decompress(file.DataStream, output);
                break;

            default:
                throw new InvalidOperationException($"Invalid compression type {file.CompressionType}.");
        }

        file.DecompressedStream = output;

        file.DataStream.Position = 0;
        file.DecompressedStream.Position = 0;
    }

    private static Level5CompressionMethod GetLevel5CompressionType(CompressionType compression)
    {
        return compression switch
        {
            CompressionType.Level5Rle => Level5CompressionMethod.Rle,
            CompressionType.Level5Lz10 => Level5CompressionMethod.Lz10,
            CompressionType.Level5Huffman4Bit => Level5CompressionMethod.Huffman4,
            CompressionType.Level5Huffman8Bit => Level5CompressionMethod.Huffman8,
            _ => throw new InvalidOperationException($"Unsupported Level5 compression {compression}.")
        };
    }

    private static NintendoCompressionMethod GetNintendoCompressionType(CompressionType compression)
    {
        return compression switch
        {
            CompressionType.NintendoLz10 => NintendoCompressionMethod.Lz10,
            CompressionType.NintendoLz11 => NintendoCompressionMethod.Lz11,
            CompressionType.NintendoHuffman4Bit => NintendoCompressionMethod.Huffman4,
            CompressionType.NintendoHuffman8Bit => NintendoCompressionMethod.Huffman8,
            CompressionType.NintendoRle => NintendoCompressionMethod.Rle,
            _ => throw new InvalidOperationException($"Unsupported Nintendo compression {compression}.")
        };
    }
}