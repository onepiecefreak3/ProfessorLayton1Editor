namespace Logic.Domain.NintendoManagement.Contract.DataClasses;

public class OverlayEntry
{
    public int id;
    public int ramAddress;
    public int ramSize;
    public int bssSize;
    public int staticInitStartAddress;
    public int staticInitEndAddress;
    public int fileId;
    public int reserved1;
}