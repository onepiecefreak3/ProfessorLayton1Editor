using Logic.Domain.NintendoManagement.Contract.Enums;

namespace Logic.Domain.NintendoManagement.Contract.DataClasses;

public class DsiHeader
{
    public string gameTitle;
    public string gameCode;
    public string makerCode;
    public UnitCode unitCode;
    public byte encryptionSeed;
    public byte deviceCapacity;
    public byte[] reserved1;

    public byte systemFlags;
    public byte permitJump;
    public byte romVer;
    public byte internalFlag;   // Bit2 enable autostart

    public int arm9Offset;
    public int arm9EntryAddress;
    public int arm9LoadAddress;
    public int arm9Size;

    public int arm7Offset;
    public int arm7EntryAddress;
    public int arm7LoadAddress;
    public int arm7Size;

    public int fntOffset;
    public int fntSize;

    public int fatOffset;
    public int fatSize;

    public int arm9OverlayOffset;
    public int arm9OverlaySize;

    public int arm7OverlayOffset;
    public int arm7OverlaySize;

    public int normalRegisterSettings;
    public int secureRegisterSettings;

    public int iconOffset;

    public short secureAreaCrc;
    public short secureTransferTimeout;

    public int arm9AutoLoad;
    public int arm7AutoLoad;

    public long secureDisable;

    public int ntrRegionSize;
    public int headerSize;

    public int arm9ParametersOffset;
    public int arm7ParametersOffset;
    public short ntrRegionEnd;
    public short twlRegionStart;

    public byte[] reserved3;

    public byte[] nintendoLogo;
    public short nintendoLogoCrc;

    public short headerCrc;

    public int dbgRomOffset;
    public int dbgSize;
    public int dbgLoadAddress;  // 0x168
    public int reserved4;
    public byte[] reservedDbg;

    public DsiExtendedEntries extendedEntries;
}