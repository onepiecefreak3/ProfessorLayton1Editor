using Komponent.IO;
using Logic.Business.Layton1ToolManagement.Contract;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.Contract.Files;
using Logic.Business.Layton1ToolManagement.DataClasses;

namespace Logic.Business.Layton1ToolManagement;

class Layton1PuzzleIdProvider(ILayton1NdsFileManager fileManager, ILayton1PathProvider pathProvider) : ILayton1PuzzleIdProvider
{
    private static readonly Dictionary<GameVersion, int[]> OffsetLookup = new()
    {
        [GameVersion.Usa] = [0xcb634], // USA
        [GameVersion.Korea] = [0xe5554], // Korea
        [GameVersion.Europe] = [0xdaeb0], // Europe
        [GameVersion.Japan] = [0xddbcc, 0xdf464, 0xdda14], // Japan, Japan Rev1, Japan Rev2v
        [GameVersion.UsaDemo] = [0xc8cdc], // USA Demo
        [GameVersion.EuropeDemo] = [0xaddcc], // Europe Demo
        [GameVersion.JapanFriendly] = [0x9119c]  // Japan Friendly
    };

    // WiFi puzzle ID's and release dates can only be determined by reading save data after those puzzles are downloaded
    private static readonly Dictionary<GameVersion, Dictionary<int, WifiPuzzle>> WifiLookup = new()
    {
        [GameVersion.Usa] = new Dictionary<int, WifiPuzzle> // USA
        {
            [0x77] = new(1, DateTimeOffset.Parse("17.02.2008")),
            [0x7E] = new(2, DateTimeOffset.Parse("24.02.2008")),
            [0x59] = new(3, DateTimeOffset.Parse("02.03.2008")),
            [0x9A] = new(4, DateTimeOffset.Parse("09.03.2008")),
            [0x9E] = new(5, DateTimeOffset.Parse("16.03.2008")),
            [0x61] = new(6, DateTimeOffset.Parse("23.03.2008")),
            [0x3D] = new(7, DateTimeOffset.Parse("30.03.2008")),
            [0x64] = new(8, DateTimeOffset.Parse("06.04.2008")),
            [0x72] = new(9, DateTimeOffset.Parse("13.04.2008")),
            [0x80] = new(10, DateTimeOffset.Parse("20.04.2008")),
            [0x92] = new(11, DateTimeOffset.Parse("27.04.2008")),
            [0x9C] = new(12, DateTimeOffset.Parse("04.05.2008")),
            [0x84] = new(13, DateTimeOffset.Parse("11.05.2008")),
            [0xA2] = new(14, DateTimeOffset.Parse("18.05.2008")),
            [0x95] = new(15, DateTimeOffset.Parse("25.05.2008")),
            [0x4B] = new(16, DateTimeOffset.Parse("01.06.2008")),
            [0x4C] = new(17, DateTimeOffset.Parse("08.06.2008")),
            [0x8A] = new(18, DateTimeOffset.Parse("15.06.2008")),
            [0x68] = new(19, DateTimeOffset.Parse("22.06.2008")),
            [0x6E] = new(20, DateTimeOffset.Parse("29.06.2008")),
            [0x7F] = new(21, DateTimeOffset.Parse("06.07.2008")),
            [0x93] = new(22, DateTimeOffset.Parse("13.07.2008")),
            [0x99] = new(23, DateTimeOffset.Parse("20.07.2008")),
            [0x9D] = new(24, DateTimeOffset.Parse("27.07.2008")),
            [0x96] = new(25, DateTimeOffset.Parse("03.08.2008")),
            [0x8B] = new(26, DateTimeOffset.Parse("10.08.2008"))
        },
        [GameVersion.Korea] = new Dictionary<int, WifiPuzzle> // Korea
        {
            [0x77] = new(1, DateTimeOffset.Parse("11.09.2008")),
            [0x7E] = new(2, DateTimeOffset.Parse("18.09.2008")),
            [0x59] = new(3, DateTimeOffset.Parse("25.09.2008")),
            [0x9A] = new(4, DateTimeOffset.Parse("02.10.2008")),
            [0x9E] = new(5, DateTimeOffset.Parse("09.10.2008")),
            [0x61] = new(6, DateTimeOffset.Parse("16.10.2008")),
            [0x3D] = new(7, DateTimeOffset.Parse("23.10.2008")),
            [0x64] = new(8, DateTimeOffset.Parse("30.10.2008")),
            [0x72] = new(9, DateTimeOffset.Parse("06.11.2008")),
            [0x80] = new(10, DateTimeOffset.Parse("13.11.2008")),
            [0x92] = new(11, DateTimeOffset.Parse("20.11.2008")),
            [0x9C] = new(12, DateTimeOffset.Parse("27.11.2008")),
            [0x84] = new(13, DateTimeOffset.Parse("04.12.2008")),
            [0xA2] = new(14, DateTimeOffset.Parse("11.12.2008")),
            [0x95] = new(15, DateTimeOffset.Parse("18.12.2008")),
            [0x4B] = new(16, DateTimeOffset.Parse("25.12.2008")),
            [0x4C] = new(17, DateTimeOffset.Parse("01.01.2009")),
            [0x8A] = new(18, DateTimeOffset.Parse("08.01.2009")),
            [0x68] = new(19, DateTimeOffset.Parse("15.01.2009")),
            [0x6E] = new(20, DateTimeOffset.Parse("22.01.2009")),
            [0x7F] = new(21, DateTimeOffset.Parse("29.01.2009")),
            [0x93] = new(22, DateTimeOffset.Parse("05.02.2009")),
            [0x99] = new(23, DateTimeOffset.Parse("12.02.2009")),
            [0x9D] = new(24, DateTimeOffset.Parse("19.02.2009")),
            [0x94] = new(25, DateTimeOffset.Parse("26.02.2009")),
            [0x96] = new(26, DateTimeOffset.Parse("05.03.2009")),
            [0x8B] = new(27, DateTimeOffset.Parse("12.03.2009"))
        },
        [GameVersion.Europe] = new Dictionary<int, WifiPuzzle> // Europe
        {
            [0x77] = new(1, DateTimeOffset.Parse("07.11.2008")),
            [0x7E] = new(2, DateTimeOffset.Parse("14.11.2008")),
            [0x59] = new(3, DateTimeOffset.Parse("21.11.2008")),
            [0x9A] = new(4, DateTimeOffset.Parse("28.11.2008")),
            [0x9E] = new(5, DateTimeOffset.Parse("05.12.2008")),
            [0x61] = new(6, DateTimeOffset.Parse("12.12.2008")),
            [0x3D] = new(7, DateTimeOffset.Parse("19.12.2008")),
            [0x64] = new(8, DateTimeOffset.Parse("26.12.2008")),
            [0x72] = new(9, DateTimeOffset.Parse("02.01.2009")),
            [0x80] = new(10, DateTimeOffset.Parse("09.01.2009")),
            [0x92] = new(11, DateTimeOffset.Parse("16.01.2009")),
            [0x9C] = new(12, DateTimeOffset.Parse("23.01.2009")),
            [0x84] = new(13, DateTimeOffset.Parse("30.01.2009")),
            [0xA2] = new(14, DateTimeOffset.Parse("06.02.2009")),
            [0x95] = new(15, DateTimeOffset.Parse("13.02.2009")),
            [0x4B] = new(16, DateTimeOffset.Parse("20.02.2009")),
            [0x4C] = new(17, DateTimeOffset.Parse("27.02.2009")),
            [0x8A] = new(18, DateTimeOffset.Parse("06.03.2009")),
            [0x68] = new(19, DateTimeOffset.Parse("13.03.2009")),
            [0x6E] = new(20, DateTimeOffset.Parse("20.03.2009")),
            [0x7F] = new(21, DateTimeOffset.Parse("27.03.2009")),
            [0x93] = new(22, DateTimeOffset.Parse("03.04.2009")),
            [0x99] = new(23, DateTimeOffset.Parse("10.04.2009")),
            [0x9D] = new(24, DateTimeOffset.Parse("17.04.2009")),
            [0x94] = new(25, DateTimeOffset.Parse("24.04.2009")),
            [0x96] = new(26, DateTimeOffset.Parse("01.05.2009")),
            [0x8B] = new(27, DateTimeOffset.Parse("08.05.2009"))
        },
        [GameVersion.Japan] = new Dictionary<int, WifiPuzzle> // Japan, Japan Rev1, Japan Rev2v
        {
            [0x92] = new(1, DateTimeOffset.Parse("15.02.2007")),
            [0x4B] = new(2, DateTimeOffset.Parse("22.02.2007")),
            [0x61] = new(3, DateTimeOffset.Parse("01.03.2007")),
            [0x9A] = new(4, DateTimeOffset.Parse("08.03.2007")),
            [0x80] = new(5, DateTimeOffset.Parse("15.03.2007")),
            [0x7E] = new(6, DateTimeOffset.Parse("22.03.2007")),
            [0x3D] = new(7, DateTimeOffset.Parse("29.03.2007")),
            [0x4C] = new(8, DateTimeOffset.Parse("05.04.2007")),
            [0x9E] = new(10, DateTimeOffset.Parse("19.04.2007")),
            [0x64] = new(11, DateTimeOffset.Parse("26.04.2007")),
            [0x72] = new(12, DateTimeOffset.Parse("03.05.2007")),
            [0x96] = new(14, DateTimeOffset.Parse("17.05.2007")),
            [0x9D] = new(15, DateTimeOffset.Parse("24.05.2007")),
            [0x94] = new(16, DateTimeOffset.Parse("31.05.2007")),
            [0x95] = new(17, DateTimeOffset.Parse("07.06.2007")),
            [0x77] = new(18, DateTimeOffset.Parse("14.06.2007")),
            [0x68] = new(19, DateTimeOffset.Parse("21.06.2007")),
            [0x84] = new(20, DateTimeOffset.Parse("28.06.2007")),
            [0x8B] = new(21, DateTimeOffset.Parse("05.07.2007")),
            [0x99] = new(22, DateTimeOffset.Parse("12.07.2007")),
            [0xA2] = new(23, DateTimeOffset.Parse("19.07.2007")),
            [0x8A] = new(24, DateTimeOffset.Parse("26.07.2007")),
            [0x7F] = new(25, DateTimeOffset.Parse("02.08.2007")),
            [0x6E] = new(26, DateTimeOffset.Parse("09.08.2007")),
            [0x9C] = new(27, DateTimeOffset.Parse("16.08.2007")),
            [0x93] = new(28, DateTimeOffset.Parse("23.08.2007"))
        }
    };

    public int MaxPuzzleSlots => 256;

    public Layton1PuzzleId[] Get(Layton1NdsRom ndsRom)
    {
        Layton1NdsFile? arm9File = ndsRom.Files.FirstOrDefault(f => f.Path is "sys/arm9.bin");

        if (arm9File is null)
            return [];

        int[] ids = ReadIds(arm9File);

        var result = new List<Layton1PuzzleId>(ids.Length);

        for (var i = 0; i < ids.Length; i++)
        {
            if (ids[i] is 0)
                continue;

            result.Add(new Layton1PuzzleId
            {
                InternalId = i,
                Number = ids[i],
                IsWifi = false
            });
        }

        return [.. result];
    }

    public Layton1PuzzleId[] GetWifi(Layton1NdsRom ndsRom)
    {
        return ndsRom.Version is GameVersion.JapanFriendly
            ? GetWifiFriendly(ndsRom)
            : GetWifiInternational(ndsRom);
    }

    private Layton1PuzzleId[] GetWifiFriendly(Layton1NdsRom ndsRom)
    {
        string wifiOrderPath = pathProvider.GetFullDirectory("weekly/wifi_order.dat", ndsRom.Version);
        Layton1NdsFile? wifiOrderFile = ndsRom.Files.FirstOrDefault(f => f.Path == wifiOrderPath);

        if (wifiOrderFile is null)
            return [];

        Stream wifiOrderStream = fileManager.GetUncompressedStream(wifiOrderFile);
        using var reader = new BinaryReaderX(wifiOrderStream, true);

        wifiOrderStream.Position = 8;
        int wifiCount = reader.ReadInt32();

        var result = new Layton1PuzzleId[wifiCount];

        for (var i = 0; i < wifiCount; i++)
        {
            int internalId = reader.ReadByte();
            int year = reader.ReadByte() + 2000;
            int month = reader.ReadByte();
            int day = reader.ReadByte();

            result[i] = new Layton1PuzzleId
            {
                InternalId = internalId,
                Number = i + 1,
                IsWifi = true,
                ReleaseDate = new DateTime(year, month, day)
            };
        }

        return result;
    }

    private Layton1PuzzleId[] GetWifiInternational(Layton1NdsRom ndsRom)
    {
        var result = new List<Layton1PuzzleId>();

        if (!WifiLookup.TryGetValue(ndsRom.Version, out Dictionary<int, WifiPuzzle>? puzzleLookup))
            return [];

        for (var i = 0; i < MaxPuzzleSlots; i++)
        {
            WifiPuzzle? wifiPuzzle = puzzleLookup.GetValueOrDefault(i);

            if (wifiPuzzle is null)
                continue;

            result.Add(new Layton1PuzzleId
            {
                InternalId = i,
                Number = wifiPuzzle.Number,
                IsWifi = true,
                ReleaseDate = wifiPuzzle.ReleaseDate
            });
        }

        return [.. result];
    }

    public void Set(Layton1NdsRom ndsRom, Layton1PuzzleId puzzleId)
    {
        Layton1NdsFile? arm9File = ndsRom.Files.FirstOrDefault(f => f.Path is "sys/arm9.bin");

        if (arm9File is null)
            return;

        Stream fileStream = fileManager.GetUncompressedStream(arm9File);
        using var writer = new BinaryWriterX(fileStream, true);

        writer.BaseStream.Position = GetIdOffset(arm9File) + puzzleId.InternalId * 4;
        writer.Write(puzzleId.Number);

        fileManager.Compose(arm9File, fileStream);
    }

    //public void SetWifi(Layton1NdsRom ndsRom, Layton1PuzzleId puzzleId)
    //{
    //    if (ndsRom.Version is GameVersion.JapanFriendly)
    //        SetWifiFriendly(ndsRom, puzzleId);
    //}

    //private void SetWifiFriendly(Layton1NdsRom ndsRom, Layton1PuzzleId puzzleId)
    //{
    //    string wifiOrderPath = pathProvider.GetFullDirectory("weekly/wifi_order.dat", ndsRom.Version);
    //    Layton1NdsFile? wifiOrderFile = ndsRom.Files.FirstOrDefault(f => f.Path == wifiOrderPath);

    //    if (wifiOrderFile is null)
    //        return;

    //    using Stream idStream = fileManager.GetUncompressedStream(wifiOrderFile);
    //    using var idWriter = new BinaryWriterX(idStream);
    //    using var idReader = new BinaryReaderX(idStream);

    //    idStream.Position = 8;

    //    int wifiCount = idReader.ReadInt32();
    //    var hasChanges = false;

    //    for (var i = 0; i < wifiCount; i++)
    //    {
    //        int internalId = idReader.ReadByte();
    //        idStream.Position += 3;

    //        if (puzzleId.InternalId != internalId)
    //            continue;

    //        // TODO: Write all wifi puzzles in correct order according to changes. How?

    //        hasChanges = true;
    //    }

    //    if (!hasChanges)
    //        return;

    //    idStream.Position = 4;
    //    uint hash = CalculateWifiHash(idReader, (int)idStream.Length - 4);

    //    idStream.Position = 0;
    //    idWriter.Write(hash);
    //}

    private int[] ReadIds(Layton1NdsFile file)
    {
        Stream fileStream = fileManager.GetUncompressedStream(file);
        using var reader = new BinaryReaderX(fileStream, true);

        reader.BaseStream.Position = GetIdOffset(file);

        var result = new int[MaxPuzzleSlots];

        for (var i = 0; i < result.Length; i++)
            result[i] = reader.ReadInt32();

        return result;
    }

    private long GetIdOffset(Layton1NdsFile file)
    {
        if (!OffsetLookup.TryGetValue(file.Rom.Version, out int[]? tableOffsets))
            throw new InvalidOperationException($"No offset to puzzle ids in ROM for version {file.Rom.Version}.");

        Stream fileStream = fileManager.GetUncompressedStream(file);
        using var reader = new BinaryReaderX(fileStream, true);

        if (tableOffsets.Length == 1)
            return tableOffsets[0];

        foreach (int tableOffset in tableOffsets)
        {
            reader.BaseStream.Position = tableOffset - 8;

            string check = reader.ReadNullTerminatedString();
            if (check is not "Shadow")
                continue;

            return tableOffset;
        }

        throw new InvalidOperationException($"Could not determine offset to puzzle ids in ROM for version {file.Rom.Version}.");
    }

    private static uint CalculateWifiHash(BinaryReaderX reader, int count)
    {
        count >>= 1;

        uint local1 = ushort.MaxValue;
        uint local2 = ushort.MaxValue;

        while (count != 0)
        {
            int blockCount = Math.Min(0x168, count);
            count -= blockCount;

            for (var i = 0; i < blockCount; i++)
            {
                local1 += reader.ReadUInt16();
                local2 += local1;
            }

            local1 = (local1 >> 16) + (local1 & 0xFFFF);
            local2 = (local2 >> 16) + (local2 & 0xFFFF);
        }

        return local1 | (local2 << 16);
    }
}