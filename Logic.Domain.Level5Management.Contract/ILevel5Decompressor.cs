using Logic.Domain.Level5Management.Contract.Enums;

namespace Logic.Domain.Level5Management.Contract;

public interface ILevel5Decompressor
{
    CompressionType PeekCompressionMethod(Stream input);
    void Compress(Stream input, Stream output, CompressionType type);
    void Decompress(Stream input, Stream output);
}