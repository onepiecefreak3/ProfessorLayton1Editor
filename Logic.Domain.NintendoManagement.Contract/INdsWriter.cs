using Logic.Domain.NintendoManagement.Contract.DataClasses;

namespace Logic.Domain.NintendoManagement.Contract;

public interface INdsWriter
{
    void Write(NdsRom ndsRom, Stream output);
}