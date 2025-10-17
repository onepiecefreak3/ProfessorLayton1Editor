using Logic.Domain.NintendoManagement.Contract.DataClasses;

namespace Logic.Domain.NintendoManagement.Contract;

public interface INdsReader
{
    NdsRom Read(Stream input);
}