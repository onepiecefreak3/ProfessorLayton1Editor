using Logic.Domain.NintendoManagement.Contract.DataClasses.Archive;

namespace Logic.Domain.NintendoManagement.Contract.Archive;

public interface INdsReader
{
    NdsRom Read(Stream input);
}