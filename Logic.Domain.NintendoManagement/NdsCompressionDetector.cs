using System.Buffers.Binary;
using Logic.Domain.NintendoManagement.Contract;
using Logic.Domain.NintendoManagement.Contract.DataClasses.Archive;
using Logic.Domain.NintendoManagement.Contract.Enums;

namespace Logic.Domain.NintendoManagement;

class NdsCompressionDetector : INdsCompressionDetector
{
    public NdsCompressionType Detect(NdsRom rom, NdsFile file)
    {
        if (file is NdsContentFile)
            return NdsCompressionType.None;

        switch (file.Path)
        {
            case "sys/banner.bin":
            case "sys/arm7.bin":
            case "sys/arm9.bin" when rom.Footer is null:
                return NdsCompressionType.None;

            case "sys/arm9.bin":
                file.Stream.Position = rom.Footer.parametersOffset + 0x14;

                var buffer = new byte[4];
                _ = file.Stream.Read(buffer);

                bool hasCompressedSize = BinaryPrimitives.ReadInt32LittleEndian(buffer) != 0;
                return hasCompressedSize ? NdsCompressionType.Overlay : NdsCompressionType.None;
        }

        if (file is not NdsOverlayFile overlay)
            return NdsCompressionType.None;

        bool isCompressed = (overlay.Entry.flags & 0x1000000) != 0;
        return isCompressed ? NdsCompressionType.Overlay : NdsCompressionType.None;
    }
}