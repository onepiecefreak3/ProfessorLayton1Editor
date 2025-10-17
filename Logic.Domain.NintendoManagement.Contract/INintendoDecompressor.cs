using Logic.Domain.NintendoManagement.Contract.Enums;

namespace Logic.Domain.NintendoManagement.Contract;

public interface INintendoDecompressor
{
    CompressionMethod PeekCompressionMethod(Stream input);
    void Compress(Stream input, Stream output, CompressionMethod method);
    void Decompress(Stream input, Stream output);
}