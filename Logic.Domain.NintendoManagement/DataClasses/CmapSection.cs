namespace Logic.Domain.NintendoManagement.DataClasses;

struct CmapSection
{
    public ushort codeBegin;
    public ushort codeEnd;
    public short mappingMethod;
    public short reserved;
    public int nextCmapOffset;
    public object indexData;
}