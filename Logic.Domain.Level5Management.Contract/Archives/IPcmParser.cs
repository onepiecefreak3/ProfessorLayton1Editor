using Logic.Domain.Level5Management.Contract.DataClasses.Archives;

namespace Logic.Domain.Level5Management.Contract.Archives;

public interface IPcmParser
{
    PcmFile[] Parse(Stream input);
    PcmFile[] Parse(PcmContainer container);
}