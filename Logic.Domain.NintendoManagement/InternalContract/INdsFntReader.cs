using Logic.Domain.NintendoManagement.Contract.DataClasses.Archive;
using Logic.Domain.NintendoManagement.DataClasses.Archive;

namespace Logic.Domain.NintendoManagement.InternalContract;

interface INdsFntReader
{
    NdsFile[] Read(Stream input, int fntOffset, int contentOffset, FatEntry[] fatEntries);
}