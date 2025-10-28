namespace Logic.Domain.NintendoManagement.Contract.DataClasses.Archive;

public class OverlayEntry
{
    public int id;
    public int ramAddress;
    public int ramSize;
    public int bssSize;
    public int staticInitStartAddress;
    public int staticInitEndAddress;
    public int fileId;
    public int flags;
}