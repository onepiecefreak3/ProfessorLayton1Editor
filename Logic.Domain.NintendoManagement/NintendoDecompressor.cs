using Logic.Domain.NintendoManagement.Contract;
using Logic.Domain.NintendoManagement.Contract.Enums;
using System.Buffers.Binary;
using Kompression;
using Kompression.Contract;

namespace Logic.Domain.NintendoManagement;

class NintendoDecompressor : INintendoDecompressor
{
    public int PeekDecompressedSize(Stream input)
    {
        var sizeMethodBuffer = new byte[4];
        _ = input.Read(sizeMethodBuffer, 0, 4);
        input.Position -= 4;

        return (int)(BinaryPrimitives.ReadUInt32LittleEndian(sizeMethodBuffer) >> 8);
    }

    public CompressionMethod PeekCompressionMethod(Stream input)
    {
        int method = input.ReadByte();
        input.Position--;

        if (!Enum.IsDefined(typeof(CompressionMethod), method))
            throw new InvalidOperationException("Stream is not compressed with a Nintendo method.");

        return (CompressionMethod)method;
    }

    public void Compress(Stream input, Stream output, CompressionMethod method)
    {
        ICompression compression;
        switch (method)
        {
            case CompressionMethod.Lz10:
                compression = Compressions.Nintendo.Lz10.Build();
                break;

            case CompressionMethod.Lz11:
                compression = Compressions.Nintendo.Lz11.Build();
                break;

            case CompressionMethod.Huffman4:
                compression = Compressions.Nintendo.Huffman4Bit.Build();
                break;

            case CompressionMethod.Huffman8:
                compression = Compressions.Nintendo.Huffman8Bit.Build();
                break;

            case CompressionMethod.Rle:
                compression = Compressions.Nintendo.Rle.Build();
                break;

            default:
                throw new InvalidOperationException($"Unknown compression method {method}.");
        }

        compression.Compress(input, output);
    }

    public void Decompress(Stream input, Stream output)
    {
        CompressionMethod method = PeekCompressionMethod(input);

        ICompression compression;
        switch (method)
        {
            case CompressionMethod.Lz10:
                compression = Compressions.Nintendo.Lz10.Build();
                break;

            case CompressionMethod.Lz11:
                compression = Compressions.Nintendo.Lz11.Build();
                break;

            case CompressionMethod.Huffman4:
                compression = Compressions.Nintendo.Huffman4Bit.Build();
                break;

            case CompressionMethod.Huffman8:
                compression = Compressions.Nintendo.Huffman8Bit.Build();
                break;

            case CompressionMethod.Rle:
                compression = Compressions.Nintendo.Rle.Build();
                break;

            default:
                throw new InvalidOperationException($"Unknown compression method {method}.");
        }

        compression.Decompress(input, output);
    }
}