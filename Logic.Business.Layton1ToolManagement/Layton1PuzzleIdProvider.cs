using Komponent.IO;
using Logic.Business.Layton1ToolManagement.Contract;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.InternalContract.Compression;

namespace Logic.Business.Layton1ToolManagement;

class Layton1PuzzleIdProvider(ILayton1Compressor compressor) : ILayton1PuzzleIdProvider
{
    private const int PuzzleSlots_ = 256;
    private static readonly Dictionary<string, int[]> OffsetLookup = new()
    {
        ["A5FEHF"] = [0xcb634], // USA
        ["A5FK01"] = [0xe5554], // Korea,
        ["A5FP01"] = [0xdaeb0], // Europe,
        ["A5FJHF"] = [0xddbcc, 0xdf464, 0xdda14], // Japan, Japan Rev1, Japan Rev2v
        ["Y49E01"] = [0xc8cdc], // USA Demo
        ["Y49P01"] = [0xaddcc], // Europe Demo
        ["C5FJHF"] = [0x9119c]  // Japan Friendly
    };

    public int[] Get(Layton1NdsRom ndsRom)
    {
        Layton1NdsFile? arm9File = ndsRom.Files.FirstOrDefault(f => f.Path is "sys/arm9.bin");
        if (arm9File is null)
            throw new InvalidOperationException("ROM does not contain an arm9 binary.");

        compressor.Decompress(arm9File);

        string gameCode = ndsRom.GameCode;
        string makerCode = ndsRom.DsHeader?.makerCode ?? ndsRom.DsiHeader?.makerCode!;

        return ReadIds(arm9File, gameCode, makerCode);
    }

    private static int[] ReadIds(Layton1NdsFile file, string gameCode, string makerCode)
    {
        string gameId = gameCode + makerCode;

        if (!OffsetLookup.TryGetValue(gameId, out int[]? tableOffsets))
            throw new InvalidOperationException($"No offset to puzzle ids in ROM with game code {gameCode} and maker code {makerCode}.");

        using var reader = new BinaryReaderX(file.DecompressedStream!, true);

        if (tableOffsets.Length == 1)
        {
            reader.BaseStream.Position = tableOffsets[0];
            return ReadIntegers(reader, PuzzleSlots_);
        }

        foreach (int tableOffset in tableOffsets)
        {
            reader.BaseStream.Position = tableOffset - 8;

            string check = reader.ReadNullTerminatedString();
            if (check is not "Shadow")
                continue;

            reader.BaseStream.Position = tableOffset;
            return ReadIntegers(reader, PuzzleSlots_);
        }

        throw new InvalidOperationException($"Could not determine offset to puzzle ids in ROM with game code {gameCode} and maker code {makerCode}.");
    }

    private static int[] ReadIntegers(BinaryReaderX reader, int count)
    {
        var result = new int[count];

        for (var i = 0; i < count; i++)
            result[i] = reader.ReadInt32();

        return result;
    }
}