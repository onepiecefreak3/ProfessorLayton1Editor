using Logic.Domain.NintendoManagement.Contract.DataClasses;

namespace Logic.Domain.NintendoManagement.DataClasses;

struct CwdhSection
{
    public short startIndex;
    public short endIndex;
    public int nextCwdhOffset;
    public CwdhEntry[] entries;
}