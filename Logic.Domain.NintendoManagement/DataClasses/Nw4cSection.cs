namespace Logic.Domain.NintendoManagement.DataClasses;

struct Nw4cSection
{
    public long sectionOffset;

    public string magic;
    public int sectionSize;
    public object sectionData;
}