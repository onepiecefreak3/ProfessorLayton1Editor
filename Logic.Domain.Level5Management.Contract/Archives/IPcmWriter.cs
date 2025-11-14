using Logic.Domain.Level5Management.Contract.DataClasses.Archives;

namespace Logic.Domain.Level5Management.Contract.Archives;

public interface IPcmWriter
{
    void Write(List<PcmFile> files, Stream output);
}