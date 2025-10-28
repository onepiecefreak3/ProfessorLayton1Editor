using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Files;
using Logic.Domain.NintendoManagement.Contract.Archive;
using Logic.Domain.NintendoManagement.Contract.DataClasses.Archive;

namespace Logic.Business.Layton1ToolManagement.Files;

class Layton1NdsComposer(ILayton1NdsFileManager fileManager, INdsWriter ndsWriter) : ILayton1NdsComposer
{
    public void Compose(Layton1NdsRom rom, Stream output)
    {
        NdsRom ndsRom = Compose(rom);

        ndsWriter.Write(ndsRom, output);
    }

    private NdsRom Compose(Layton1NdsRom rom)
    {
        foreach (Layton1NdsFile file in rom.Files)
        {
            if (!file.IsChanged)
                continue;

            fileManager.Compress(file);
            file.IsChanged = false;
        }

        return new NdsRom
        {
            Files = [.. rom.Files.Select(CreateNdsFile)],
            GameCode = rom.GameCode,
            DsHeader = rom.DsHeader,
            DsiHeader = rom.DsiHeader,
            Footer = rom.Footer
        };
    }

    private static NdsFile CreateNdsFile(Layton1NdsFile file)
    {
        return file switch
        {
            Layton1NdsOverlayFile overlay => new NdsOverlayFile
            {
                Path = file.Path,
                Stream = file.DataStream,
                Entry = overlay.Entry
            },
            Layton1NdsContentFile content => new NdsContentFile
            {
                Path = file.Path,
                Stream = file.DataStream,
                FileId = content.FileId
            },
            not null => new NdsFile
            {
                Path = file.Path,
                Stream = file.DataStream
            },
            _ => throw new InvalidOperationException("Unsupported type of file.")
        };
    }
}