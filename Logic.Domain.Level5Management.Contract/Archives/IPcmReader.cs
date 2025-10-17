using Logic.Domain.Level5Management.Contract.DataClasses.Archives;

namespace Logic.Domain.Level5Management.Contract.Archives;

public interface IPcmReader
{
    PcmContainer Read(Stream input);
}