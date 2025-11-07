using Komponent.IO;
using Logic.Domain.NintendoManagement.Contract.Archive;
using Logic.Domain.NintendoManagement.Contract.DataClasses.Archive;
using Logic.Domain.NintendoManagement.DataClasses.Archive;
using Logic.Domain.NintendoManagement.InternalContract;

namespace Logic.Domain.NintendoManagement.Archive;

class NdsWriter(INdsFntWriter fntWriter) : INdsWriter
{
    private const int OverlayEntrySize_ = 0x20;
    private const int FatEntrySize_ = 0x8;

    public void Write(NdsRom ndsRom, Stream output)
    {
        var arm9File = ndsRom.Files.First(x => x.Path == "sys/arm9.bin");
        var arm7File = ndsRom.Files.First(x => x.Path == "sys/arm7.bin");
        var iconFile = ndsRom.Files.FirstOrDefault(x => x.Path == "sys/banner.bin");

        var arm9Overlays = ndsRom.Files
            .Where(x => Path.GetFileName(x.Path).StartsWith("overlay9", StringComparison.Ordinal))
            .Cast<NdsOverlayFile>()
            .ToArray();
        var arm7Overlays = ndsRom.Files
            .Where(x => Path.GetFileName(x.Path).StartsWith("overlay7", StringComparison.Ordinal))
            .Cast<NdsOverlayFile>()
            .ToArray();

        var arm9OverlayEntries = new List<OverlayEntry>();
        var arm7OverlayEntries = new List<OverlayEntry>();
        var fatEntries = new List<FatEntry>();

        using var bw = new BinaryWriterX(output, true);

        // Write ARM9
        var arm9Offset = 0x4000;
        var arm9Size = (int)arm9File.Stream.Length;

        output.Position = arm9Offset;
        arm9File.Stream.Position = 0;
        arm9File.Stream.CopyTo(output);

        if (ndsRom.Footer is not null)
            WriteArm9Footer(ndsRom.Footer, bw);

        bw.WriteAlignment(0x200, 0xFF);

        // Write ARM9 Overlays
        var arm9OverlayOffset = (int)output.Position;
        var arm9OverlaySize = arm9Overlays.Length * OverlayEntrySize_;
        var arm9OverlayPosition = arm9OverlayOffset + arm9OverlaySize + 0x1FF & ~0x1FF;

        foreach (var arm9Overlay in arm9Overlays.OrderBy(x => x.Entry.id))
        {
            output.Position = arm9OverlayPosition;
            arm9Overlay.Stream.Position = 0;
            arm9Overlay.Stream.CopyTo(output);

            bw.WriteAlignment(0x200, 0xFF);

            arm9Overlay.Entry.fileId = fatEntries.Count;
            arm9OverlayEntries.Add(arm9Overlay.Entry);

            fatEntries.Add(new FatEntry
            {
                offset = arm9OverlayPosition,
                endOffset = (int)(arm9OverlayPosition + arm9Overlay.Stream.Length)
            });

            arm9OverlayPosition += (int)(arm9Overlay.Stream.Length + 0x1FF) & ~0x1FF;
        }

        output.Position = arm9OverlayOffset;
        WriteOverlayEntries(arm9OverlayEntries, bw);
        bw.WriteAlignment(0x200, 0xFF);

        // Write ARM7
        var arm7Offset = arm9OverlayPosition;
        var arm7Size = (int)arm7File.Stream.Length;

        output.Position = arm7Offset;
        arm7File.Stream.Position = 0;
        arm7File.Stream.CopyTo(output);

        // Write ARM7 Overlays
        var arm7OverlayOffset = arm7Offset + arm7Size;
        var arm7OverlaySize = arm7Overlays.Length * OverlayEntrySize_;
        var arm7OverlayPosition = arm7OverlayOffset + arm7OverlaySize + 0x1FF & ~0x1FF;

        foreach (var arm7Overlay in arm7Overlays.OrderBy(x => x.Entry.id))
        {
            output.Position = arm9OverlayPosition;
            arm7Overlay.Stream.Position = 0;
            arm7Overlay.Stream.CopyTo(output);

            bw.WriteAlignment(0x200, 0xFF);

            arm7Overlay.Entry.fileId = fatEntries.Count;
            arm7OverlayEntries.Add(arm7Overlay.Entry);

            fatEntries.Add(new FatEntry
            {
                offset = arm7OverlayPosition,
                endOffset = (int)(arm7OverlayPosition + arm7Overlay.Stream.Length)
            });

            arm7OverlayPosition += (int)arm7Overlay.Stream.Length + 0x1FF & ~0x1FF;
        }

        output.Position = arm7OverlayOffset;
        WriteOverlayEntries(arm7OverlayEntries, bw);
        bw.WriteAlignment(0x200, 0xFF);

        // Write FNT
        var romFiles = ndsRom.Files.Where(x => x is NdsContentFile).Cast<NdsContentFile>().ToArray();

        var fntOffset = arm7OverlayPosition;

        fntWriter.WriteFnt(output, fntOffset, romFiles, arm9Overlays.Length + arm7Overlays.Length);

        var fntSize = (int)output.Position - fntOffset;
        bw.WriteAlignment(0x200, 0xFF);

        //  Write icon
        var fatOffset = (int)output.Position;
        var fatSize = (ndsRom.Files.Length - 3) * FatEntrySize_;     // Not counting arm9.bin, arm7.bin, banner.bin

        var iconOffset = fatOffset + fatSize + 0x1FF & ~0x1FF;
        var iconSize = (int)(iconFile?.Stream.Length ?? 0);

        if (iconFile is not null)
        {
            output.Position = iconOffset;
            iconFile.Stream.Position = 0;
            iconFile.Stream.CopyTo(output);

            bw.WriteAlignment(0x200, 0xFF);
        }

        //  Write rom files
        var filePosition = iconOffset + iconSize + 0x1FF & ~0x1FF;

        foreach (var romFile in romFiles.OrderBy(x => x.FileId))
        {
            output.Position = filePosition;
            romFile.Stream.Position = 0;
            romFile.Stream.CopyTo(output);

            fatEntries.Add(new FatEntry
            {
                offset = filePosition,
                endOffset = (int)(filePosition + romFile.Stream.Length)
            });

            filePosition += (int)(romFile.Stream.Length + 0x1FF) & ~0x1FF;
        }

        // Write FAT
        output.Position = fatOffset;
        WriteFatEntries(fatEntries, bw);
        bw.WriteAlignment(0x200, 0xFF);

        // Write header
        output.Position = 0;

        if (ndsRom.DsHeader is not null)
        {
            ndsRom.DsHeader.arm9Offset = arm9Offset;
            ndsRom.DsHeader.arm7Offset = arm7Offset;
            ndsRom.DsHeader.arm9OverlayOffset = arm9Overlays.Length > 0 ? arm9OverlayOffset : 0;
            ndsRom.DsHeader.arm7OverlayOffset = arm7Overlays.Length > 0 ? arm7OverlayOffset : 0;
            ndsRom.DsHeader.fntOffset = fntOffset;
            ndsRom.DsHeader.fatOffset = fatOffset;
            ndsRom.DsHeader.iconOffset = iconOffset;

            ndsRom.DsHeader.arm9Size = arm9Size;
            ndsRom.DsHeader.arm7Size = arm7Size;
            ndsRom.DsHeader.arm9OverlaySize = arm9Overlays.Length > 0 ? arm9OverlaySize : 0;
            ndsRom.DsHeader.arm7OverlaySize = arm7Overlays.Length > 0 ? arm7OverlaySize : 0;
            ndsRom.DsHeader.fntSize = fntSize;
            ndsRom.DsHeader.fatSize = fatSize;

            WriteNdsHeader(ndsRom.DsHeader, bw);
        }
        else
        {
            ndsRom.DsiHeader!.arm9Offset = arm9Offset;
            ndsRom.DsiHeader.arm7Offset = arm7Offset;
            ndsRom.DsiHeader.arm9OverlayOffset = arm9Overlays.Length > 0 ? arm9OverlayOffset : 0;
            ndsRom.DsiHeader.arm7OverlayOffset = arm7Overlays.Length > 0 ? arm7OverlayOffset : 0;
            ndsRom.DsiHeader.fntOffset = fntOffset;
            ndsRom.DsiHeader.fatOffset = fatOffset;
            ndsRom.DsiHeader.iconOffset = iconOffset;

            ndsRom.DsiHeader.arm9Size = arm9Size;
            ndsRom.DsiHeader.arm7Size = arm7Size;
            ndsRom.DsiHeader.arm9OverlaySize = arm9Overlays.Length > 0 ? arm9OverlaySize : 0;
            ndsRom.DsiHeader.arm7OverlaySize = arm7Overlays.Length > 0 ? arm7OverlaySize : 0;
            ndsRom.DsiHeader.fntSize = fntSize;
            ndsRom.DsiHeader.fatSize = fatSize;
            ndsRom.DsiHeader.extendedEntries.iconSize = iconSize;

            WriteDsiHeader(ndsRom.DsiHeader, bw);
        }
    }

    private void WriteNdsHeader(NdsHeader header, BinaryWriterX writer)
    {
        writer.WriteString(header.gameTitle, writeNullTerminator: false);
        writer.WriteString(header.gameCode, writeNullTerminator: false);
        writer.WriteString(header.makerCode, writeNullTerminator: false);
        writer.Write((byte)header.unitCode);
        writer.Write(header.encryptionSeed);
        writer.Write(header.deviceCapacity);
        writer.Write(header.reserved1);
        writer.Write(header.reserved2);
        writer.Write(header.consoleRegion);
        writer.Write(header.romVer);
        writer.Write(header.internalFlag);
        writer.Write(header.arm9Offset);
        writer.Write(header.arm9EntryAddress);
        writer.Write(header.arm9LoadAddress);
        writer.Write(header.arm9Size);
        writer.Write(header.arm7Offset);
        writer.Write(header.arm7EntryAddress);
        writer.Write(header.arm7LoadAddress);
        writer.Write(header.arm7Size);
        writer.Write(header.fntOffset);
        writer.Write(header.fntSize);
        writer.Write(header.fatOffset);
        writer.Write(header.fatSize);
        writer.Write(header.arm9OverlayOffset);
        writer.Write(header.arm9OverlaySize);
        writer.Write(header.arm7OverlayOffset);
        writer.Write(header.arm7OverlaySize);
        writer.Write(header.normalRegisterSettings);
        writer.Write(header.secureRegisterSettings);
        writer.Write(header.iconOffset);
        writer.Write(header.secureAreaCrc);
        writer.Write(header.secureTransferTimeout);
        writer.Write(header.arm9AutoLoad);
        writer.Write(header.arm7AutoLoad);
        writer.Write(header.secureDisable);
        writer.Write(header.ntrRegionSize);
        writer.Write(header.headerSize);
        writer.Write(header.reserved3);
        writer.Write(header.nintendoLogo);
        writer.Write(header.nintendoLogoCrc);
        writer.Write(header.headerCrc);
        writer.Write(header.dbgRomOffset);
        writer.Write(header.dbgSize);
        writer.Write(header.dbgLoadAddress);
        writer.Write(header.reserved4);
        writer.Write(header.reservedDbg);
    }

    private void WriteDsiHeader(DsiHeader header, BinaryWriterX writer)
    {
        writer.WriteString(header.gameTitle, writeNullTerminator: false);
        writer.WriteString(header.gameCode, writeNullTerminator: false);
        writer.WriteString(header.makerCode, writeNullTerminator: false);
        writer.Write((byte)header.unitCode);
        writer.Write(header.encryptionSeed);
        writer.Write(header.deviceCapacity);
        writer.Write(header.reserved1);
        writer.Write(header.systemFlags);
        writer.Write(header.permitJump);
        writer.Write(header.romVer);
        writer.Write(header.internalFlag);
        writer.Write(header.arm9Offset);
        writer.Write(header.arm9EntryAddress);
        writer.Write(header.arm9LoadAddress);
        writer.Write(header.arm9Size);
        writer.Write(header.arm7Offset);
        writer.Write(header.arm7EntryAddress);
        writer.Write(header.arm7LoadAddress);
        writer.Write(header.arm7Size);
        writer.Write(header.fntOffset);
        writer.Write(header.fntSize);
        writer.Write(header.fatOffset);
        writer.Write(header.fatSize);
        writer.Write(header.arm9OverlayOffset);
        writer.Write(header.arm9OverlaySize);
        writer.Write(header.arm7OverlayOffset);
        writer.Write(header.arm7OverlaySize);
        writer.Write(header.normalRegisterSettings);
        writer.Write(header.secureRegisterSettings);
        writer.Write(header.iconOffset);
        writer.Write(header.secureAreaCrc);
        writer.Write(header.secureTransferTimeout);
        writer.Write(header.arm9AutoLoad);
        writer.Write(header.arm7AutoLoad);
        writer.Write(header.secureDisable);
        writer.Write(header.ntrRegionSize);
        writer.Write(header.headerSize);
        writer.Write(header.arm9ParametersOffset);
        writer.Write(header.arm7ParametersOffset);
        writer.Write(header.ntrRegionEnd);
        writer.Write(header.twlRegionStart);
        writer.Write(header.reserved3);
        writer.Write(header.nintendoLogo);
        writer.Write(header.nintendoLogoCrc);
        writer.Write(header.headerCrc);
        writer.Write(header.dbgRomOffset);
        writer.Write(header.dbgSize);
        writer.Write(header.dbgLoadAddress);
        writer.Write(header.reserved4);
        writer.Write(header.reservedDbg);

        WriteExtendedEntries(header.extendedEntries, writer);
    }

    private void WriteExtendedEntries(DsiExtendedEntries entries, BinaryWriterX writer)
    {
        writer.Write(entries.mbkSettings);
        writer.Write(entries.arm9MbkSettings);
        writer.Write(entries.arm7MbkSettings);
        writer.Write(entries.mbk9Setting);
        writer.Write(entries.wramNctSettings);
        writer.Write(entries.regionFlags);
        writer.Write(entries.accessControl);
        writer.Write(entries.arm7ScfgSetting);
        writer.Write(entries.reserved1);
        writer.Write(entries.flags);
        writer.Write(entries.arm9iOffset);
        writer.Write(entries.reserved2);
        writer.Write(entries.arm9iLoadAddress);
        writer.Write(entries.arm9iSize);
        writer.Write(entries.arm7iOffset);
        writer.Write(entries.reserved3);
        writer.Write(entries.arm7iLoadAddress);
        writer.Write(entries.arm7iSize);
        writer.Write(entries.digestNtrOffset);
        writer.Write(entries.digestNtrSize);
        writer.Write(entries.digestTwlOffset);
        writer.Write(entries.digestTwlSize);
        writer.Write(entries.digestSectorHashtableOffset);
        writer.Write(entries.digestSectorHashtableSize);
        writer.Write(entries.digestBlockHashtableOffset);
        writer.Write(entries.digestBlockHashtableSize);
        writer.Write(entries.digestSectorSize);
        writer.Write(entries.digestBlockSectorCount);
        writer.Write(entries.iconSize);
        writer.Write(entries.sdmmcSize1);
        writer.Write(entries.sdmmcSize2);
        writer.Write(entries.eulaVersion);
        writer.Write(entries.useRatings);
        writer.Write(entries.totalRomSize);
        writer.Write(entries.sdmmcSize3);
        writer.Write(entries.sdmmcSize4);
        writer.Write(entries.sdmmcSize5);
        writer.Write(entries.sdmmcSize6);
        writer.Write(entries.arm9iParametersOffset);
        writer.Write(entries.arm7iParametersOffset);
        writer.Write(entries.modCryptArea1Offset);
        writer.Write(entries.modCryptArea1Size);
        writer.Write(entries.modCryptArea2Offset);
        writer.Write(entries.modCryptArea2Size);
        writer.Write(entries.gameCode);
        writer.Write(entries.fileType);
        writer.Write(entries.titleIdZero0);
        writer.Write(entries.titleIdZeroThree);
        writer.Write(entries.titleIdZero1);
        writer.Write(entries.sdmmcPublicSaveSize);
        writer.Write(entries.sdmmcPrivateSaveSize);
        writer.Write(entries.reserved4);

        WriteParentalControl(entries.parentalControl, writer);
        WriteSha1Section(entries.sha1Section, writer);
    }

    private void WriteParentalControl(DsiParentalControl parentalControl, BinaryWriterX writer)
    {
        writer.Write(parentalControl.ageRatings);
        writer.Write(parentalControl.cero);
        writer.Write(parentalControl.esrb);
        writer.Write(parentalControl.reserved1);
        writer.Write(parentalControl.usk);
        writer.Write(parentalControl.pegiEur);
        writer.Write(parentalControl.reserved2);
        writer.Write(parentalControl.pegiPrt);
        writer.Write(parentalControl.bbfc);
        writer.Write(parentalControl.agcb);
        writer.Write(parentalControl.grb);
        writer.Write(parentalControl.reserved3);
    }

    private void WriteSha1Section(Sha1Section sha1, BinaryWriterX writer)
    {
        writer.Write(sha1.arm9HmacHash);
        writer.Write(sha1.arm7HmacHash);
        writer.Write(sha1.digestMasterHmacHash);
        writer.Write(sha1.iconHmacHash);
        writer.Write(sha1.arm9iHmacHash);
        writer.Write(sha1.arm7iHmacHash);
        writer.Write(sha1.reserved1);
        writer.Write(sha1.reserved2);
        writer.Write(sha1.arm9HmacHashWithoutSecureArea);
        writer.Write(sha1.reserved3);
        writer.Write(sha1.dbgVariableStorage);
        writer.Write(sha1.headerSectionRsa);
    }

    private void WriteArm9Footer(Arm9Footer footer, BinaryWriterX writer)
    {
        writer.Write(footer.nitroCode);
        writer.Write(footer.parametersOffset);
        writer.Write(footer.hmacOverlayOffset);
    }

    private void WriteOverlayEntries(IList<OverlayEntry> entries, BinaryWriterX writer)
    {
        foreach (OverlayEntry entry in entries)
            WriteOverlayEntry(entry, writer);
    }

    private void WriteOverlayEntry(OverlayEntry entry, BinaryWriterX writer)
    {
        writer.Write(entry.id);
        writer.Write(entry.ramAddress);
        writer.Write(entry.ramSize);
        writer.Write(entry.bssSize);
        writer.Write(entry.staticInitStartAddress);
        writer.Write(entry.staticInitEndAddress);
        writer.Write(entry.fileId);
        writer.Write(entry.flags);
    }

    private void WriteFatEntries(IList<FatEntry> entries, BinaryWriterX writer)
    {
        foreach (FatEntry entry in entries)
            WriteFatEntry(entry, writer);
    }

    private void WriteFatEntry(FatEntry entry, BinaryWriterX writer)
    {
        writer.Write(entry.offset);
        writer.Write(entry.endOffset);
    }
}