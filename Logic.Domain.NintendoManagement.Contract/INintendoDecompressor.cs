using Logic.Domain.NintendoManagement.Contract.Enums;

namespace Logic.Domain.NintendoManagement.Contract;

public interface INintendoDecompressor
{
    CompressionType PeekCompressionMethod(Stream input);

    void Compress(Stream input, Stream output, CompressionType type);
    void Decompress(Stream input, Stream output);

    void DecompressOverlay(Stream input, Stream output);
    void CompressOverlay(Stream input, Stream output);
}