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
    public CompressionMethod PeekCompressionMethod(Stream input)
    {
        var buffer = new byte[4];
        _ = input.Read(buffer);
        input.Position -= 4;

        int method = BinaryPrimitives.ReadInt32LittleEndian(buffer);

        if (!Enum.IsDefined(typeof(CompressionMethod), method))
            throw new InvalidOperationException("Stream is not compressed with a Level5 method.");

        return (CompressionMethod)method;
    }

    public void Compress(Stream input, Stream output, CompressionMethod method)
    {
        output.Position += 4;

        ICompression compression;
        switch (method)
        {
            case CompressionMethod.Lz10:
                compression = Compressions.Nintendo.Lz10.Build();
                break;

            case CompressionMethod.Huffman4:
                compression = new CompressionConfigurationBuilder()
                    .Decode.With(() => new HuffmanDecoder(4, NibbleOrder.LowNibbleFirst))
                    .Encode.With(() => new HuffmanEncoder(4, NibbleOrder.LowNibbleFirst))
                    .Build();
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
        input.Position += 4;

        ICompression compression;
        switch (method)
        {
            case CompressionMethod.Lz10:
                compression = Compressions.Nintendo.Lz10.Build();
                break;

            case CompressionMethod.Huffman4:
                compression = new CompressionConfigurationBuilder()
                    .Decode.With(() => new HuffmanDecoder(4, NibbleOrder.LowNibbleFirst))
                    .Encode.With(() => new HuffmanEncoder(4, NibbleOrder.LowNibbleFirst))
                    .Build();
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