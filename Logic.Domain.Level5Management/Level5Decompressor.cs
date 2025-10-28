using System.Buffers.Binary;
using Kompression;
using Kompression.Configuration;
using Kompression.Contract;
using Kompression.Contract.Enums.Encoder.Huffman;
using Kompression.Decoder.Nintendo;
using Kompression.Encoder.Nintendo;
using Logic.Domain.Level5Management.Contract;
using Logic.Domain.Level5Management.Contract.Enums;

namespace Logic.Domain.Level5Management;

class Level5Decompressor : ILevel5Decompressor
{
    public CompressionType PeekCompressionMethod(Stream input)
    {
        var buffer = new byte[4];
        _ = input.Read(buffer);
        input.Position -= 4;

        int method = BinaryPrimitives.ReadInt32LittleEndian(buffer);

        if (!Enum.IsDefined(typeof(CompressionType), method))
            throw new InvalidOperationException("Stream is not compressed with a Level5 method.");

        return (CompressionType)method;
    }

    public void Compress(Stream input, Stream output, CompressionType type)
    {
        output.Position += 4;

        ICompression compression;
        switch (type)
        {
            case CompressionType.Lz10:
                compression = Compressions.Nintendo.Lz10.Build();
                break;

            case CompressionType.Huffman4:
                compression = new CompressionConfigurationBuilder()
                    .Decode.With(() => new HuffmanDecoder(4, NibbleOrder.LowNibbleFirst))
                    .Encode.With(() => new HuffmanEncoder(4, NibbleOrder.LowNibbleFirst))
                    .Build();
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
        input.Position += 4;

        ICompression compression;
        switch (type)
        {
            case CompressionType.Lz10:
                compression = Compressions.Nintendo.Lz10.Build();
                break;

            case CompressionType.Huffman4:
                compression = new CompressionConfigurationBuilder()
                    .Decode.With(() => new HuffmanDecoder(4, NibbleOrder.LowNibbleFirst))
                    .Encode.With(() => new HuffmanEncoder(4, NibbleOrder.LowNibbleFirst))
                    .Build();
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
}