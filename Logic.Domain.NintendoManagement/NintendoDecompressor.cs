using Kompression;
using Kompression.Contract;
using Logic.Domain.NintendoManagement.Contract;
using Logic.Domain.NintendoManagement.Contract.Enums;

namespace Logic.Domain.NintendoManagement;

class NintendoDecompressor : INintendoDecompressor
{
    public CompressionType PeekCompressionMethod(Stream input)
    {
        int method = input.ReadByte();
        input.Position--;

        if (!Enum.IsDefined(typeof(CompressionType), method))
            throw new InvalidOperationException("Stream is not compressed with a Nintendo method.");

        return (CompressionType)method;
    }

    public void Compress(Stream input, Stream output, CompressionType type)
    {
        ICompression compression;
        switch (type)
        {
            case CompressionType.Lz10:
                compression = Compressions.Nintendo.Lz10.Build();
                break;

            case CompressionType.Lz11:
                compression = Compressions.Nintendo.Lz11.Build();
                break;

            case CompressionType.Huffman4:
                compression = Compressions.Nintendo.Huffman4Bit.Build();
                break;

            case CompressionType.Huffman8:
                compression = Compressions.Nintendo.Huffman8Bit.Build();
                break;

            case CompressionType.Rle:
                compression = Compressions.Nintendo.Rle.Build();
                break;

            default:
                throw new InvalidOperationException($"Unknown compression method {type}.");
        }

        compression.Compress(input, output);
    }

    public void Decompress(Stream input, Stream output)
    {
        CompressionType type = PeekCompressionMethod(input);

        ICompression compression;
        switch (type)
        {
            case CompressionType.Lz10:
                compression = Compressions.Nintendo.Lz10.Build();
                break;

            case CompressionType.Lz11:
                compression = Compressions.Nintendo.Lz11.Build();
                break;

            case CompressionType.Huffman4:
                compression = Compressions.Nintendo.Huffman4Bit.Build();
                break;

            case CompressionType.Huffman8:
                compression = Compressions.Nintendo.Huffman8Bit.Build();
                break;

            case CompressionType.Rle:
                compression = Compressions.Nintendo.Rle.Build();
                break;

            default:
                throw new InvalidOperationException($"Unknown compression method {type}.");
        }

        compression.Decompress(input, output);
    }

    public void DecompressOverlay(Stream input, Stream output)
    {
        Compressions.Nintendo.BackwardLz77.Build().Decompress(input, output);
    }

    public void CompressOverlay(Stream input, Stream output)
    {
        Compressions.Nintendo.BackwardLz77.Build().Compress(input, output);
    }
}