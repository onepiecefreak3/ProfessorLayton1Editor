using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace Logic.Business.Layton1ToolManagement.InternalContract.Compression;

interface ILayton1Compressor
{
    void Compress(Layton1NdsFile file);
    void Decompress(Layton1NdsFile file);
}