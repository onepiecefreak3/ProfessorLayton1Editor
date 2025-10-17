namespace Logic.Domain.NintendoManagement.Contract.DataClasses;

public class DsiExtendedEntries
{
    public byte[] mbkSettings;
    public byte[] arm9MbkSettings;
    public byte[] arm7MbkSettings;
    public byte[] mbk9Setting;
    public byte wramNctSettings;

    public int regionFlags;
    public int accessControl;
    public int arm7ScfgSetting;
    public byte[] reserved1;
    public byte flags;

    public int arm9iOffset;
    public int reserved2;
    public int arm9iLoadAddress;
    public int arm9iSize;

    public int arm7iOffset;
    public int reserved3;
    public int arm7iLoadAddress;
    public int arm7iSize;

    public int digestNtrOffset;
    public int digestNtrSize;

    public int digestTwlOffset;
    public int digestTwlSize;

    public int digestSectorHashtableOffset;
    public int digestSectorHashtableSize;

    public int digestBlockHashtableOffset;
    public int digestBlockHashtableSize;

    public int digestSectorSize;
    public int digestBlockSectorCount;

    public int iconSize;

    public byte sdmmcSize1;
    public byte sdmmcSize2;

    public byte eulaVersion;
    public bool useRatings;
    public int totalRomSize;

    public byte sdmmcSize3;
    public byte sdmmcSize4;
    public byte sdmmcSize5;
    public byte sdmmcSize6;

    public int arm9iParametersOffset;
    public int arm7iParametersOffset;

    public int modCryptArea1Offset;
    public int modCryptArea1Size;
    public int modCryptArea2Offset;
    public int modCryptArea2Size;

    public int gameCode;    // gamecode backwards
    public byte fileType;
    public byte titleIdZero0;
    public byte titleIdZeroThree;
    public byte titleIdZero1;

    public int sdmmcPublicSaveSize;
    public int sdmmcPrivateSaveSize;
    public byte[] reserved4;

    public DsiParentalControl parentalControl;

    public Sha1Section sha1Section;
}