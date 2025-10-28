using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.InternalContract.Compression;
using Logic.Domain.Level5Management.Contract;
using Logic.Domain.NintendoManagement.Contract;
using CompressionType = Logic.Domain.Level5Management.Contract.Enums.CompressionType;

namespace Logic.Business.Layton1ToolManagement.Compression;

class Layton1Compressor(
    INintendoDecompressor nintendoDecompressor,
    ILevel5Decompressor level5Decompressor)
    : ILayton1Compressor
{
    public void Compress(Layton1NdsFile file)
    {
        file.DataStream.Position = 0;

        if (file.CompressionType is Contract.Enums.CompressionType.None)
            return;

        if (file.DecompressedStream is null)
            return;

        file.DecompressedStream.Position = 0;

        var output = new MemoryStream();
        switch (file.CompressionType)
        {
            case Contract.Enums.CompressionType.Level5Lz10:
            case Contract.Enums.CompressionType.Level5Huffman4Bit:
            case Contract.Enums.CompressionType.Level5Huffman8Bit:
            case Contract.Enums.CompressionType.Level5Rle:
                level5Decompressor.Compress(file.DecompressedStream, output, GetLevel5CompressionType(file.CompressionType));
                break;

            case Contract.Enums.CompressionType.NintendoLz10:
            case Contract.Enums.CompressionType.NintendoLz11:
            case Contract.Enums.CompressionType.NintendoHuffman4Bit:
            case Contract.Enums.CompressionType.NintendoHuffman8Bit:
            case Contract.Enums.CompressionType.NintendoRle:
                nintendoDecompressor.Compress(file.DecompressedStream, output, GetNintendoCompressionType(file.CompressionType));
                break;

            case Contract.Enums.CompressionType.NintendoOverlay:
                nintendoDecompressor.CompressOverlay(file.DecompressedStream, output);
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

        if (file.CompressionType is Contract.Enums.CompressionType.None)
            return;

        if (file.DecompressedStream is not null)
        {
            file.DecompressedStream.Position = 0;
            return;
        }

        var output = new MemoryStream();
        switch (file.CompressionType)
        {
            case Contract.Enums.CompressionType.Level5Lz10:
            case Contract.Enums.CompressionType.Level5Huffman4Bit:
            case Contract.Enums.CompressionType.Level5Huffman8Bit:
            case Contract.Enums.CompressionType.Level5Rle:
                level5Decompressor.Decompress(file.DataStream, output);
                break;

            case Contract.Enums.CompressionType.NintendoLz10:
            case Contract.Enums.CompressionType.NintendoLz11:
            case Contract.Enums.CompressionType.NintendoHuffman4Bit:
            case Contract.Enums.CompressionType.NintendoHuffman8Bit:
            case Contract.Enums.CompressionType.NintendoRle:
                nintendoDecompressor.Decompress(file.DataStream, output);
                break;

            case Contract.Enums.CompressionType.NintendoOverlay:
                nintendoDecompressor.DecompressOverlay(file.DataStream, output);
                break;

            default:
                throw new InvalidOperationException($"Invalid compression type {file.CompressionType}.");
        }

        file.DecompressedStream = output;

        file.DataStream.Position = 0;
        file.DecompressedStream.Position = 0;
    }

    private static CompressionType GetLevel5CompressionType(Contract.Enums.CompressionType compression)
    {
        return compression switch
        {
            Contract.Enums.CompressionType.Level5Rle => CompressionType.Rle,
            Contract.Enums.CompressionType.Level5Lz10 => CompressionType.Lz10,
            Contract.Enums.CompressionType.Level5Huffman4Bit => CompressionType.Huffman4,
            Contract.Enums.CompressionType.Level5Huffman8Bit => CompressionType.Huffman8,
            _ => throw new InvalidOperationException($"Unsupported Level5 compression {compression}.")
        };
    }

    private static Domain.NintendoManagement.Contract.Enums.CompressionType GetNintendoCompressionType(Contract.Enums.CompressionType compression)
    {
        return compression switch
        {
            Contract.Enums.CompressionType.NintendoLz10 => Domain.NintendoManagement.Contract.Enums.CompressionType.Lz10,
            Contract.Enums.CompressionType.NintendoLz11 => Domain.NintendoManagement.Contract.Enums.CompressionType.Lz11,
            Contract.Enums.CompressionType.NintendoHuffman4Bit => Domain.NintendoManagement.Contract.Enums.CompressionType.Huffman4,
            Contract.Enums.CompressionType.NintendoHuffman8Bit => Domain.NintendoManagement.Contract.Enums.CompressionType.Huffman8,
            Contract.Enums.CompressionType.NintendoRle => Domain.NintendoManagement.Contract.Enums.CompressionType.Rle,
            _ => throw new InvalidOperationException($"Unsupported Nintendo compression {compression}.")
        };
    }
}