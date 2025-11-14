using System.Globalization;
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
            [0x77] = new(1, DateTimeOffset.Parse("02.17.2008", CultureInfo.InvariantCulture)),
            [0x7E] = new(2, DateTimeOffset.Parse("02.24.2008", CultureInfo.InvariantCulture)),
            [0x59] = new(3, DateTimeOffset.Parse("03.02.2008", CultureInfo.InvariantCulture)),
            [0x9A] = new(4, DateTimeOffset.Parse("03.09.2008", CultureInfo.InvariantCulture)),
            [0x9E] = new(5, DateTimeOffset.Parse("03.16.2008", CultureInfo.InvariantCulture)),
            [0x61] = new(6, DateTimeOffset.Parse("03.23.2008", CultureInfo.InvariantCulture)),
            [0x3D] = new(7, DateTimeOffset.Parse("03.30.2008", CultureInfo.InvariantCulture)),
            [0x64] = new(8, DateTimeOffset.Parse("04.06.2008", CultureInfo.InvariantCulture)),
            [0x72] = new(9, DateTimeOffset.Parse("04.13.2008", CultureInfo.InvariantCulture)),
            [0x80] = new(10, DateTimeOffset.Parse("04.20.2008", CultureInfo.InvariantCulture)),
            [0x92] = new(11, DateTimeOffset.Parse("04.27.2008", CultureInfo.InvariantCulture)),
            [0x9C] = new(12, DateTimeOffset.Parse("05.04.2008", CultureInfo.InvariantCulture)),
            [0x84] = new(13, DateTimeOffset.Parse("05.11.2008", CultureInfo.InvariantCulture)),
            [0xA2] = new(14, DateTimeOffset.Parse("05.18.2008", CultureInfo.InvariantCulture)),
            [0x95] = new(15, DateTimeOffset.Parse("05.25.2008", CultureInfo.InvariantCulture)),
            [0x4B] = new(16, DateTimeOffset.Parse("06.01.2008", CultureInfo.InvariantCulture)),
            [0x4C] = new(17, DateTimeOffset.Parse("06.08.2008", CultureInfo.InvariantCulture)),
            [0x8A] = new(18, DateTimeOffset.Parse("06.15.2008", CultureInfo.InvariantCulture)),
            [0x68] = new(19, DateTimeOffset.Parse("06.22.2008", CultureInfo.InvariantCulture)),
            [0x6E] = new(20, DateTimeOffset.Parse("06.29.2008", CultureInfo.InvariantCulture)),
            [0x7F] = new(21, DateTimeOffset.Parse("07.06.2008", CultureInfo.InvariantCulture)),
            [0x93] = new(22, DateTimeOffset.Parse("07.13.2008", CultureInfo.InvariantCulture)),
            [0x99] = new(23, DateTimeOffset.Parse("07.20.2008", CultureInfo.InvariantCulture)),
            [0x9D] = new(24, DateTimeOffset.Parse("07.27.2008", CultureInfo.InvariantCulture)),
            [0x96] = new(25, DateTimeOffset.Parse("08.03.2008", CultureInfo.InvariantCulture)),
            [0x8B] = new(26, DateTimeOffset.Parse("08.01.2008", CultureInfo.InvariantCulture))
        },
        [GameVersion.Korea] = new Dictionary<int, WifiPuzzle> // Korea
        {
            [0x77] = new(1, DateTimeOffset.Parse("09.11.2008", CultureInfo.InvariantCulture)),
            [0x7E] = new(2, DateTimeOffset.Parse("09.18.2008", CultureInfo.InvariantCulture)),
            [0x59] = new(3, DateTimeOffset.Parse("09.25.2008", CultureInfo.InvariantCulture)),
            [0x9A] = new(4, DateTimeOffset.Parse("10.02.2008", CultureInfo.InvariantCulture)),
            [0x9E] = new(5, DateTimeOffset.Parse("10.09.2008", CultureInfo.InvariantCulture)),
            [0x61] = new(6, DateTimeOffset.Parse("10.16.2008", CultureInfo.InvariantCulture)),
            [0x3D] = new(7, DateTimeOffset.Parse("10.23.2008", CultureInfo.InvariantCulture)),
            [0x64] = new(8, DateTimeOffset.Parse("10.30.2008", CultureInfo.InvariantCulture)),
            [0x72] = new(9, DateTimeOffset.Parse("11.06.2008", CultureInfo.InvariantCulture)),
            [0x80] = new(10, DateTimeOffset.Parse("11.13.2008", CultureInfo.InvariantCulture)),
            [0x92] = new(11, DateTimeOffset.Parse("11.20.2008", CultureInfo.InvariantCulture)),
            [0x9C] = new(12, DateTimeOffset.Parse("11.27.2008", CultureInfo.InvariantCulture)),
            [0x84] = new(13, DateTimeOffset.Parse("12.04.2008", CultureInfo.InvariantCulture)),
            [0xA2] = new(14, DateTimeOffset.Parse("12.11.2008", CultureInfo.InvariantCulture)),
            [0x95] = new(15, DateTimeOffset.Parse("12.18.2008", CultureInfo.InvariantCulture)),
            [0x4B] = new(16, DateTimeOffset.Parse("12.25.2008", CultureInfo.InvariantCulture)),
            [0x4C] = new(17, DateTimeOffset.Parse("01.01.2009", CultureInfo.InvariantCulture)),
            [0x8A] = new(18, DateTimeOffset.Parse("01.08.2009", CultureInfo.InvariantCulture)),
            [0x68] = new(19, DateTimeOffset.Parse("01.15.2009", CultureInfo.InvariantCulture)),
            [0x6E] = new(20, DateTimeOffset.Parse("01.22.2009", CultureInfo.InvariantCulture)),
            [0x7F] = new(21, DateTimeOffset.Parse("01.29.2009", CultureInfo.InvariantCulture)),
            [0x93] = new(22, DateTimeOffset.Parse("02.05.2009", CultureInfo.InvariantCulture)),
            [0x99] = new(23, DateTimeOffset.Parse("02.12.2009", CultureInfo.InvariantCulture)),
            [0x9D] = new(24, DateTimeOffset.Parse("02.19.2009", CultureInfo.InvariantCulture)),
            [0x94] = new(25, DateTimeOffset.Parse("02.26.2009", CultureInfo.InvariantCulture)),
            [0x96] = new(26, DateTimeOffset.Parse("03.05.2009", CultureInfo.InvariantCulture)),
            [0x8B] = new(27, DateTimeOffset.Parse("03.12.2009", CultureInfo.InvariantCulture))
        },
        [GameVersion.Europe] = new Dictionary<int, WifiPuzzle> // Europe
        {
            [0x77] = new(1, DateTimeOffset.Parse("11.07.2008", CultureInfo.InvariantCulture)),
            [0x7E] = new(2, DateTimeOffset.Parse("11.14.2008", CultureInfo.InvariantCulture)),
            [0x59] = new(3, DateTimeOffset.Parse("11.21.2008", CultureInfo.InvariantCulture)),
            [0x9A] = new(4, DateTimeOffset.Parse("11.28.2008", CultureInfo.InvariantCulture)),
            [0x9E] = new(5, DateTimeOffset.Parse("12.05.2008", CultureInfo.InvariantCulture)),
            [0x61] = new(6, DateTimeOffset.Parse("12.12.2008", CultureInfo.InvariantCulture)),
            [0x3D] = new(7, DateTimeOffset.Parse("12.19.2008", CultureInfo.InvariantCulture)),
            [0x64] = new(8, DateTimeOffset.Parse("12.26.2008", CultureInfo.InvariantCulture)),
            [0x72] = new(9, DateTimeOffset.Parse("01.02.2009", CultureInfo.InvariantCulture)),
            [0x80] = new(10, DateTimeOffset.Parse("01.09.2009", CultureInfo.InvariantCulture)),
            [0x92] = new(11, DateTimeOffset.Parse("01.16.2009", CultureInfo.InvariantCulture)),
            [0x9C] = new(12, DateTimeOffset.Parse("01.23.2009", CultureInfo.InvariantCulture)),
            [0x84] = new(13, DateTimeOffset.Parse("01.30.2009", CultureInfo.InvariantCulture)),
            [0xA2] = new(14, DateTimeOffset.Parse("02.06.2009", CultureInfo.InvariantCulture)),
            [0x95] = new(15, DateTimeOffset.Parse("02.13.2009", CultureInfo.InvariantCulture)),
            [0x4B] = new(16, DateTimeOffset.Parse("02.20.2009", CultureInfo.InvariantCulture)),
            [0x4C] = new(17, DateTimeOffset.Parse("02.27.2009", CultureInfo.InvariantCulture)),
            [0x8A] = new(18, DateTimeOffset.Parse("03.06.2009", CultureInfo.InvariantCulture)),
            [0x68] = new(19, DateTimeOffset.Parse("03.13.2009", CultureInfo.InvariantCulture)),
            [0x6E] = new(20, DateTimeOffset.Parse("03.20.2009", CultureInfo.InvariantCulture)),
            [0x7F] = new(21, DateTimeOffset.Parse("03.27.2009", CultureInfo.InvariantCulture)),
            [0x93] = new(22, DateTimeOffset.Parse("04.03.2009", CultureInfo.InvariantCulture)),
            [0x99] = new(23, DateTimeOffset.Parse("04.10.2009", CultureInfo.InvariantCulture)),
            [0x9D] = new(24, DateTimeOffset.Parse("04.17.2009", CultureInfo.InvariantCulture)),
            [0x94] = new(25, DateTimeOffset.Parse("04.24.2009", CultureInfo.InvariantCulture)),
            [0x96] = new(26, DateTimeOffset.Parse("05.01.2009", CultureInfo.InvariantCulture)),
            [0x8B] = new(27, DateTimeOffset.Parse("05.08.2009", CultureInfo.InvariantCulture))
        },
        [GameVersion.Japan] = new Dictionary<int, WifiPuzzle> // Japan, Japan Rev1, Japan Rev2v
        {
            [0x92] = new(1, DateTimeOffset.Parse("02.15.2007", CultureInfo.InvariantCulture)),
            [0x4B] = new(2, DateTimeOffset.Parse("02.22.2007", CultureInfo.InvariantCulture)),
            [0x61] = new(3, DateTimeOffset.Parse("03.01.2007", CultureInfo.InvariantCulture)),
            [0x9A] = new(4, DateTimeOffset.Parse("03.08.2007", CultureInfo.InvariantCulture)),
            [0x80] = new(5, DateTimeOffset.Parse("03.15.2007", CultureInfo.InvariantCulture)),
            [0x7E] = new(6, DateTimeOffset.Parse("03.22.2007", CultureInfo.InvariantCulture)),
            [0x3D] = new(7, DateTimeOffset.Parse("03.29.2007", CultureInfo.InvariantCulture)),
            [0x4C] = new(8, DateTimeOffset.Parse("04.05.2007", CultureInfo.InvariantCulture)),
            [0x9E] = new(10, DateTimeOffset.Parse("04.19.2007", CultureInfo.InvariantCulture)),
            [0x64] = new(11, DateTimeOffset.Parse("04.26.2007", CultureInfo.InvariantCulture)),
            [0x72] = new(12, DateTimeOffset.Parse("05.03.2007", CultureInfo.InvariantCulture)),
            [0x96] = new(14, DateTimeOffset.Parse("05.17.2007", CultureInfo.InvariantCulture)),
            [0x9D] = new(15, DateTimeOffset.Parse("05.24.2007", CultureInfo.InvariantCulture)),
            [0x94] = new(16, DateTimeOffset.Parse("05.31.2007", CultureInfo.InvariantCulture)),
            [0x95] = new(17, DateTimeOffset.Parse("06.07.2007", CultureInfo.InvariantCulture)),
            [0x77] = new(18, DateTimeOffset.Parse("06.14.2007", CultureInfo.InvariantCulture)),
            [0x68] = new(19, DateTimeOffset.Parse("06.21.2007", CultureInfo.InvariantCulture)),
            [0x84] = new(20, DateTimeOffset.Parse("06.28.2007", CultureInfo.InvariantCulture)),
            [0x8B] = new(21, DateTimeOffset.Parse("07.05.2007", CultureInfo.InvariantCulture)),
            [0x99] = new(22, DateTimeOffset.Parse("07.12.2007", CultureInfo.InvariantCulture)),
            [0xA2] = new(23, DateTimeOffset.Parse("07.19.2007", CultureInfo.InvariantCulture)),
            [0x8A] = new(24, DateTimeOffset.Parse("07.26.2007", CultureInfo.InvariantCulture)),
            [0x7F] = new(25, DateTimeOffset.Parse("08.02.2007", CultureInfo.InvariantCulture)),
            [0x6E] = new(26, DateTimeOffset.Parse("08.09.2007", CultureInfo.InvariantCulture)),
            [0x9C] = new(27, DateTimeOffset.Parse("08.16.2007", CultureInfo.InvariantCulture)),
            [0x93] = new(28, DateTimeOffset.Parse("08.23.2007", CultureInfo.InvariantCulture))
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

    public void Set(Layton1NdsRom ndsRom, Layton1PuzzleId[] puzzleIds)
    {
        SetWifi(ndsRom, puzzleIds);

        Layton1NdsFile? arm9File = ndsRom.Files.FirstOrDefault(f => f.Path is "sys/arm9.bin");

        if (arm9File is null)
            return;

        var ids = new int[MaxPuzzleSlots];
        foreach (Layton1PuzzleId puzzleId in puzzleIds.OrderBy(p => p.InternalId))
        {
            if (puzzleId.IsWifi || puzzleId.Number <= 0)
                continue;

            ids[puzzleId.InternalId] = puzzleId.Number;
        }

        Stream fileStream = fileManager.GetUncompressedStream(arm9File);
        using var writer = new BinaryWriterX(fileStream, true);

        writer.BaseStream.Position = GetIdOffset(arm9File);
        foreach (int id in ids)
            writer.Write(id);

        fileManager.Compose(arm9File, fileStream);
    }

    private void SetWifi(Layton1NdsRom ndsRom, Layton1PuzzleId[] puzzleIds)
    {
        if (ndsRom.Version is GameVersion.JapanFriendly)
            SetWifiFriendly(ndsRom, puzzleIds);
    }

    private void SetWifiFriendly(Layton1NdsRom ndsRom, Layton1PuzzleId[] puzzleIds)
    {
        string wifiOrderPath = pathProvider.GetFullDirectory("weekly/wifi_order.dat", ndsRom.Version);
        Layton1NdsFile? wifiOrderFile = ndsRom.Files.FirstOrDefault(f => f.Path == wifiOrderPath);

        if (wifiOrderFile is null)
            return;

        using Stream idStream = new MemoryStream();
        using var idReader = new BinaryReaderX(idStream);
        using var idWriter = new BinaryWriterX(idStream);

        int wifiCount = puzzleIds.Count(p => p.IsWifi);

        idStream.Position = 4;
        idWriter.Write(wifiCount);
        idWriter.Write(wifiCount);

        foreach (Layton1PuzzleId puzzleId in puzzleIds.Where(x => x.IsWifi).OrderBy(x => x.Number))
        {
            idWriter.Write((byte)puzzleId.InternalId);
            idWriter.Write((byte)(puzzleId.ReleaseDate?.Year ?? 0));
            idWriter.Write((byte)(puzzleId.ReleaseDate?.Month ?? 0));
            idWriter.Write((byte)(puzzleId.ReleaseDate?.Day ?? 0));
        }

        idStream.Position = 4;
        uint hash = CalculateWifiHash(idReader, (int)idStream.Length - 4);

        idStream.Position = 0;
        idWriter.Write(hash);
    }

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