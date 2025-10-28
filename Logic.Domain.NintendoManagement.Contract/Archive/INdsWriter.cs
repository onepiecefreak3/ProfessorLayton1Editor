using Logic.Domain.NintendoManagement.Contract.DataClasses.Archive;

namespace Logic.Domain.NintendoManagement.Contract.Archive;

public interface INdsWriter
{
    void Write(NdsRom ndsRom, Stream output);
}