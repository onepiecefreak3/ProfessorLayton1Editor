using Logic.Domain.NintendoManagement.Contract.DataClasses;
using Logic.Domain.NintendoManagement.DataClasses;

namespace Logic.Domain.NintendoManagement.InternalContract;

interface INdsFntReader
{
    NdsFile[] Read(Stream input, int fntOffset, int contentOffset, FatEntry[] fatEntries);
}