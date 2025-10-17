using Logic.Domain.Level5Management.Contract.Enums;

namespace Logic.Domain.Level5Management.Contract;

public interface ILevel5Decompressor
{
    CompressionMethod PeekCompressionMethod(Stream input);
    void Compress(Stream input, Stream output, CompressionMethod method);
    void Decompress(Stream input, Stream output);
}