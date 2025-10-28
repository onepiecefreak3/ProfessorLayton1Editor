using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.Contract.Files;
using Logic.Business.Layton1ToolManagement.InternalContract.Compression;
using Logic.Business.Layton1ToolManagement.InternalContract.Validation;
using Logic.Domain.NintendoManagement.Contract.Archive;
using Logic.Domain.NintendoManagement.Contract.DataClasses.Archive;

namespace Logic.Business.Layton1ToolManagement.Files;

class Layton1NdsParser(INdsReader ndsReader, ILayton1GameCodeValidator codeValidator, ILayton1CompressionDetector compressionDetector) : ILayton1NdsParser
{
    public Layton1NdsRom Parse(Stream input)
    {
        NdsRom ndsRom = ndsReader.Read(input);

        return Parse(ndsRom);
    }

    private Layton1NdsRom Parse(NdsRom rom)
    {
        codeValidator.Validate(rom.GameCode);

        var ndsRom = new Layton1NdsRom
        {
            GameCode = rom.GameCode,
            Region = GetRegion(rom.GameCode),
            DsHeader = rom.DsHeader,
            DsiHeader = rom.DsiHeader,
            Footer = rom.Footer
        };

        ndsRom.Files = CreateFiles(rom, ndsRom);

        return ndsRom;
    }

    private Layton1NdsFile[] CreateFiles(NdsRom rom, Layton1NdsRom ndsRom)
    {
        var result = new Layton1NdsFile[rom.Files.Length];

        for (var i = 0; i < rom.Files.Length; i++)
        {
            CompressionType compressionType = compressionDetector.Detect(rom, rom.Files[i]);

            result[i] = rom.Files[i] switch
            {
                NdsOverlayFile overlay => new Layton1NdsOverlayFile
                {
                    Rom = ndsRom,
                    CompressionType = compressionType,
                    DataStream = rom.Files[i].Stream,
                    Path = rom.Files[i].Path,
                    Entry = overlay.Entry
                },
                NdsContentFile content => new Layton1NdsContentFile
                {
                    Rom = ndsRom,
                    CompressionType = compressionType,
                    DataStream = rom.Files[i].Stream,
                    Path = rom.Files[i].Path,
                    FileId = content.FileId
                },
                not null => new Layton1NdsFile
                {
                    Rom = ndsRom,
                    CompressionType = compressionType,
                    DataStream = rom.Files[i].Stream,
                    Path = rom.Files[i].Path
                },
                _ => throw new InvalidOperationException("Unsupported type of file.")
            };
        }

        return result;
    }

    private static Region GetRegion(string gameCode)
    {
        if (gameCode.Length < 4)
            throw new InvalidOperationException("Game Code needs to be at least 3 characters long.");

        return gameCode[3] switch
        {
            'J' => Region.Japan,
            'K' => Region.Korea,
            'E' => Region.Usa,
            'P' => Region.Europe,
            _ => throw new InvalidOperationException($"Region Code {gameCode[3]} is not associated with Professor Layton 1.")
        };
    }
}