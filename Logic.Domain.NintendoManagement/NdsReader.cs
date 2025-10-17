using Logic.Domain.NintendoManagement.Contract;
using Logic.Domain.NintendoManagement.Contract.DataClasses;
using Komponent.IO;
using Komponent.Streams;
using Logic.Domain.NintendoManagement.Contract.Enums;
using Logic.Domain.NintendoManagement.DataClasses;
using Logic.Domain.NintendoManagement.InternalContract;

namespace Logic.Domain.NintendoManagement;

class NdsReader(INdsFntReader fntReader) : INdsReader
{
    private const int OverlayEntrySize_ = 0x20;
    private const int FatEntrySize_ = 0x8;

    public NdsRom Read(Stream input)
    {
        var result = new List<NdsFile>();

        using var br = new BinaryReaderX(input, true);

        // Read unit code
        input.Position = 0x12;
        var unitCode = (UnitCode)br.ReadByte();

        // Read header
        NdsHeader? ndsHeader = null;
        DsiHeader? dsiHeader = null;

        input.Position = 0;
        if (unitCode == UnitCode.NDS)
            ndsHeader = ReadNdsHeader(br);
        else
            dsiHeader = ReadDsiHeader(br);

        // Read ARM9
        var arm9Offset = ndsHeader?.arm9Offset ?? dsiHeader!.arm9Offset;
        var arm9Size = ndsHeader?.arm9Size ?? dsiHeader!.arm9Size;
        result.Add(new NdsFile { Stream = new SubStream(input, arm9Offset, arm9Size), Path = "sys/arm9.bin" });

        // Read ARM9 Footer
        Arm9Footer? arm9Footer = null;

        input.Position = arm9Offset + arm9Size;
        var nitroCode = br.ReadUInt32();
        if (nitroCode == 0xDEC00621)
        {
            input.Position -= 4;
            arm9Footer = ReadArm9Footer(br);
        }

        // Read ARM9 Overlays
        var arm9OvlOffset = ndsHeader?.arm9OverlayOffset ?? dsiHeader!.arm9OverlayOffset;
        var arm9OvlSize = ndsHeader?.arm9OverlaySize ?? dsiHeader!.arm9OverlaySize;
        var arm9OvlEntryCount = arm9OvlSize / OverlayEntrySize_;

        input.Position = arm9OvlOffset;
        IList<OverlayEntry> arm9OverlayEntries = Array.Empty<OverlayEntry>();
        if (arm9OvlOffset != 0)
            arm9OverlayEntries = ReadOverlayEntries(br, arm9OvlEntryCount);

        // Read ARM7
        var arm7Offset = ndsHeader?.arm7Offset ?? dsiHeader!.arm7Offset;
        var arm7Size = ndsHeader?.arm7Size ?? dsiHeader!.arm7Size;
        result.Add(new NdsFile { Stream = new SubStream(input, arm7Offset, arm7Size), Path = "sys/arm7.bin" });

        // Read ARM7 Overlays
        var arm7OvlOffset = ndsHeader?.arm7OverlayOffset ?? dsiHeader!.arm7OverlayOffset;
        var arm7OvlSize = ndsHeader?.arm7OverlaySize ?? dsiHeader!.arm7OverlaySize;
        var arm7OvlEntryCount = arm7OvlSize / OverlayEntrySize_;

        input.Position = arm7OvlOffset;
        IList<OverlayEntry> arm7OverlayEntries = Array.Empty<OverlayEntry>();
        if (arm7OvlOffset != 0)
            arm7OverlayEntries = ReadOverlayEntries(br, arm7OvlEntryCount);

        // Read FAT
        var fatOffset = ndsHeader?.fatOffset ?? dsiHeader!.fatOffset;
        var fatSize = ndsHeader?.fatSize ?? dsiHeader!.fatSize;
        var fatCount = fatSize / FatEntrySize_;

        input.Position = fatOffset;
        var fileEntries = ReadFatEntries(br, fatCount);

        // Read FNT
        var fntOffset = ndsHeader?.fntOffset ?? dsiHeader!.fntOffset;
        foreach (var file in fntReader.Read(input, fntOffset, 0, fileEntries))
            result.Add(file);

        // Add banner
        var iconOffset = ndsHeader?.iconOffset ?? dsiHeader!.iconOffset;
        var iconFile = ReadIcon(br, iconOffset, dsiHeader);
        if (iconFile is not null)
            result.Add(iconFile);

        // Add overlay files
        foreach (var file in arm9OverlayEntries)
        {
            var fileEntry = fileEntries[file.fileId];
            result.Add(new NdsOverlayFile
            {
                Stream = new SubStream(input, fileEntry.offset, fileEntry.endOffset - fileEntry.offset),
                Path = $"sys/ovl/overlay9_{file.id:000}",
                Entry = file
            });
        }

        foreach (var file in arm7OverlayEntries)
        {
            var fileEntry = fileEntries[file.fileId];
            result.Add(new NdsOverlayFile
            {
                Stream = new SubStream(input, fileEntry.offset, fileEntry.endOffset - fileEntry.offset),
                Path = $"sys/ovl/overlay7_{file.id:000}",
                Entry = file
            });
        }

        return new NdsRom
        {
            GameCode = ndsHeader?.gameCode ?? dsiHeader!.gameCode,
            Files = [.. result],
            DsHeader = ndsHeader,
            DsiHeader = dsiHeader,
            Footer = arm9Footer
        };
    }

    private NdsFile? ReadIcon(BinaryReaderX reader, int iconOffset, DsiHeader? dsiHeader)
    {
        if (iconOffset is 0)
            return null;

        reader.BaseStream.Position = iconOffset;
        var version = reader.ReadInt16();

        int iconSize;
        switch (version)
        {
            case 1:
            case 2:
                iconSize = 0xA00;
                break;

            case 3:
                iconSize = 0xC00;
                break;

            case 0x103:
                if (dsiHeader is null)
                    throw new InvalidOperationException("Icon version 0x103 is only supported on DSi cards.");

                iconSize = dsiHeader.extendedEntries.iconSize;
                break;

            default:
                throw new InvalidOperationException($"Invalid icon version '{version}'.");
        }

        return new NdsFile
        {
            Stream = new SubStream(reader.BaseStream, iconOffset, iconSize),
            Path = "sys/banner.bin"
        };
    }

    private NdsHeader ReadNdsHeader(BinaryReaderX reader)
    {
        return new NdsHeader
        {
            gameTitle = reader.ReadString(0xC),
            gameCode = reader.ReadString(4),
            makerCode = reader.ReadString(2),
            unitCode = (UnitCode)reader.ReadByte(),
            encryptionSeed = reader.ReadByte(),
            deviceCapacity = reader.ReadByte(),
            reserved1 = reader.ReadBytes(7),
            reserved2 = reader.ReadByte(),
            consoleRegion = reader.ReadByte(),
            romVer = reader.ReadByte(),
            internalFlag = reader.ReadByte(),
            arm9Offset = reader.ReadInt32(),
            arm9EntryAddress = reader.ReadInt32(),
            arm9LoadAddress = reader.ReadInt32(),
            arm9Size = reader.ReadInt32(),
            arm7Offset = reader.ReadInt32(),
            arm7EntryAddress = reader.ReadInt32(),
            arm7LoadAddress = reader.ReadInt32(),
            arm7Size = reader.ReadInt32(),
            fntOffset = reader.ReadInt32(),
            fntSize = reader.ReadInt32(),
            fatOffset = reader.ReadInt32(),
            fatSize = reader.ReadInt32(),
            arm9OverlayOffset = reader.ReadInt32(),
            arm9OverlaySize = reader.ReadInt32(),
            arm7OverlayOffset = reader.ReadInt32(),
            arm7OverlaySize = reader.ReadInt32(),
            normalRegisterSettings = reader.ReadInt32(),
            secureRegisterSettings = reader.ReadInt32(),
            iconOffset = reader.ReadInt32(),
            secureAreaCrc = reader.ReadInt16(),
            secureTransferTimeout = reader.ReadInt16(),
            arm9AutoLoad = reader.ReadInt32(),
            arm7AutoLoad = reader.ReadInt32(),
            secureDisable = reader.ReadInt64(),
            ntrRegionSize = reader.ReadInt32(),
            headerSize = reader.ReadInt32(),
            reserved3 = reader.ReadBytes(0x38),
            nintendoLogo = reader.ReadBytes(0x9C),
            nintendoLogoCrc = reader.ReadInt16(),
            headerCrc = reader.ReadInt16(),
            dbgRomOffset = reader.ReadInt32(),
            dbgSize = reader.ReadInt32(),
            dbgLoadAddress = reader.ReadInt32(),
            reserved4 = reader.ReadInt32(),
            reservedDbg = reader.ReadBytes(0x90)
        };
    }

    private DsiHeader ReadDsiHeader(BinaryReaderX reader)
    {
        return new DsiHeader
        {
            gameTitle = reader.ReadString(0xC),
            gameCode = reader.ReadString(4),
            makerCode = reader.ReadString(2),
            unitCode = (UnitCode)reader.ReadByte(),
            encryptionSeed = reader.ReadByte(),
            deviceCapacity = reader.ReadByte(),
            reserved1 = reader.ReadBytes(7),
            systemFlags = reader.ReadByte(),
            permitJump = reader.ReadByte(),
            romVer = reader.ReadByte(),
            internalFlag = reader.ReadByte(),
            arm9Offset = reader.ReadInt32(),
            arm9EntryAddress = reader.ReadInt32(),
            arm9LoadAddress = reader.ReadInt32(),
            arm9Size = reader.ReadInt32(),
            arm7Offset = reader.ReadInt32(),
            arm7EntryAddress = reader.ReadInt32(),
            arm7LoadAddress = reader.ReadInt32(),
            arm7Size = reader.ReadInt32(),
            fntOffset = reader.ReadInt32(),
            fntSize = reader.ReadInt32(),
            fatOffset = reader.ReadInt32(),
            fatSize = reader.ReadInt32(),
            arm9OverlayOffset = reader.ReadInt32(),
            arm9OverlaySize = reader.ReadInt32(),
            arm7OverlayOffset = reader.ReadInt32(),
            arm7OverlaySize = reader.ReadInt32(),
            normalRegisterSettings = reader.ReadInt32(),
            secureRegisterSettings = reader.ReadInt32(),
            iconOffset = reader.ReadInt32(),
            secureAreaCrc = reader.ReadInt16(),
            secureTransferTimeout = reader.ReadInt16(),
            arm9AutoLoad = reader.ReadInt32(),
            arm7AutoLoad = reader.ReadInt32(),
            secureDisable = reader.ReadInt64(),
            ntrRegionSize = reader.ReadInt32(),
            headerSize = reader.ReadInt32(),
            arm9ParametersOffset = reader.ReadInt32(),
            arm7ParametersOffset = reader.ReadInt32(),
            ntrRegionEnd = reader.ReadInt16(),
            twlRegionStart = reader.ReadInt16(),
            reserved3 = reader.ReadBytes(0x2C),
            nintendoLogo = reader.ReadBytes(0x9C),
            nintendoLogoCrc = reader.ReadInt16(),
            headerCrc = reader.ReadInt16(),
            dbgRomOffset = reader.ReadInt32(),
            dbgSize = reader.ReadInt32(),
            dbgLoadAddress = reader.ReadInt32(),
            reserved4 = reader.ReadInt32(),
            reservedDbg = reader.ReadBytes(0x90),
            extendedEntries = ReadExtendedEntries(reader)
        };
    }

    private DsiExtendedEntries ReadExtendedEntries(BinaryReaderX reader)
    {
        return new DsiExtendedEntries
        {
            mbkSettings = reader.ReadBytes(0x14),
            arm9MbkSettings = reader.ReadBytes(0xC),
            arm7MbkSettings = reader.ReadBytes(0xC),
            mbk9Setting = reader.ReadBytes(0x3),
            wramNctSettings = reader.ReadByte(),
            regionFlags = reader.ReadInt32(),
            accessControl = reader.ReadInt32(),
            arm7ScfgSetting = reader.ReadInt32(),
            reserved1 = reader.ReadBytes(0x3),
            flags = reader.ReadByte(),
            arm9iOffset = reader.ReadInt32(),
            reserved2 = reader.ReadInt32(),
            arm9iLoadAddress = reader.ReadInt32(),
            arm9iSize = reader.ReadInt32(),
            arm7iOffset = reader.ReadInt32(),
            reserved3 = reader.ReadInt32(),
            arm7iLoadAddress = reader.ReadInt32(),
            arm7iSize = reader.ReadInt32(),
            digestNtrOffset = reader.ReadInt32(),
            digestNtrSize = reader.ReadInt32(),
            digestTwlOffset = reader.ReadInt32(),
            digestTwlSize = reader.ReadInt32(),
            digestSectorHashtableOffset = reader.ReadInt32(),
            digestSectorHashtableSize = reader.ReadInt32(),
            digestBlockHashtableOffset = reader.ReadInt32(),
            digestBlockHashtableSize = reader.ReadInt32(),
            digestSectorSize = reader.ReadInt32(),
            digestBlockSectorCount = reader.ReadInt32(),
            iconSize = reader.ReadInt32(),
            sdmmcSize1 = reader.ReadByte(),
            sdmmcSize2 = reader.ReadByte(),
            eulaVersion = reader.ReadByte(),
            useRatings = reader.ReadBoolean(),
            totalRomSize = reader.ReadInt32(),
            sdmmcSize3 = reader.ReadByte(),
            sdmmcSize4 = reader.ReadByte(),
            sdmmcSize5 = reader.ReadByte(),
            sdmmcSize6 = reader.ReadByte(),
            arm9iParametersOffset = reader.ReadInt32(),
            arm7iParametersOffset = reader.ReadInt32(),
            modCryptArea1Offset = reader.ReadInt32(),
            modCryptArea1Size = reader.ReadInt32(),
            modCryptArea2Offset = reader.ReadInt32(),
            modCryptArea2Size = reader.ReadInt32(),
            gameCode = reader.ReadInt32(),
            fileType = reader.ReadByte(),
            titleIdZero0 = reader.ReadByte(),
            titleIdZeroThree = reader.ReadByte(),
            titleIdZero1 = reader.ReadByte(),
            sdmmcPublicSaveSize = reader.ReadInt32(),
            sdmmcPrivateSaveSize = reader.ReadInt32(),
            reserved4 = reader.ReadBytes(0xB0),
            parentalControl = ReadParentalControl(reader),
            sha1Section = ReadSha1Section(reader)
        };
    }

    private DsiParentalControl ReadParentalControl(BinaryReaderX reader)
    {
        return new DsiParentalControl
        {
            ageRatings = reader.ReadBytes(0x10),
            cero = reader.ReadByte(),
            esrb = reader.ReadByte(),
            reserved1 = reader.ReadByte(),
            usk = reader.ReadByte(),
            pegiEur = reader.ReadByte(),
            reserved2 = reader.ReadByte(),
            pegiPrt = reader.ReadByte(),
            bbfc = reader.ReadByte(),
            agcb = reader.ReadByte(),
            grb = reader.ReadByte(),
            reserved3 = reader.ReadBytes(0x6)
        };
    }

    private Sha1Section ReadSha1Section(BinaryReaderX reader)
    {
        return new Sha1Section
        {
            arm9HmacHash = reader.ReadBytes(0x14),
            arm7HmacHash = reader.ReadBytes(0x14),
            digestMasterHmacHash = reader.ReadBytes(0x14),
            iconHmacHash = reader.ReadBytes(0x14),
            arm9iHmacHash = reader.ReadBytes(0x14),
            arm7iHmacHash = reader.ReadBytes(0x14),
            reserved1 = reader.ReadBytes(0x14),
            reserved2 = reader.ReadBytes(0x14),
            arm9HmacHashWithoutSecureArea = reader.ReadBytes(0x14),
            reserved3 = reader.ReadBytes(0xA4C),
            dbgVariableStorage = reader.ReadBytes(0x180),
            headerSectionRsa = reader.ReadBytes(0x80)
        };
    }

    private Arm9Footer ReadArm9Footer(BinaryReaderX reader)
    {
        return new Arm9Footer
        {
            nitroCode = reader.ReadUInt32(),
            unk1 = reader.ReadInt32(),
            unk2 = reader.ReadInt32()
        };
    }

    private OverlayEntry[] ReadOverlayEntries(BinaryReaderX reader, int count)
    {
        var result = new OverlayEntry[count];

        for (var i = 0; i < count; i++)
            result[i] = ReadOverlayEntry(reader);

        return result;
    }

    private OverlayEntry ReadOverlayEntry(BinaryReaderX reader)
    {
        return new OverlayEntry
        {
            id = reader.ReadInt32(),
            ramAddress = reader.ReadInt32(),
            ramSize = reader.ReadInt32(),
            bssSize = reader.ReadInt32(),
            staticInitStartAddress = reader.ReadInt32(),
            staticInitEndAddress = reader.ReadInt32(),
            fileId = reader.ReadInt32(),
            reserved1 = reader.ReadInt32(),
        };
    }

    private FatEntry[] ReadFatEntries(BinaryReaderX reader, int count)
    {
        var result = new FatEntry[count];

        for (var i = 0; i < count; i++)
            result[i] = ReadFatEntry(reader);

        return result;
    }

    private FatEntry ReadFatEntry(BinaryReaderX reader)
    {
        return new FatEntry
        {
            offset = reader.ReadInt32(),
            endOffset = reader.ReadInt32()
        };
    }
}