using Logic.Domain.NintendoManagement.InternalContract;
using Logic.Domain.NintendoManagement.Contract.DataClasses;
using Komponent.IO;
using Komponent.Streams;
using Logic.Domain.NintendoManagement.DataClasses;

namespace Logic.Domain.NintendoManagement;

class NdsFntReader : INdsFntReader
{
    public NdsFile[] Read(Stream input, int fntOffset, int contentOffset, FatEntry[] fatEntries)
    {
        using var reader = new BinaryReaderX(input, true);

        reader.BaseStream.Position = fntOffset;
        var mainEntry = ReadFntEntry(reader);

        reader.BaseStream.Position = fntOffset;
        var mainEntries = ReadFntEntries(reader, mainEntry.parentDirectory);

        var result = new List<NdsFile>();

        foreach (var file in ReadSubFnt(reader, mainEntries[0], fntOffset, contentOffset, string.Empty, mainEntries, fatEntries))
            result.Add(file);

        return [.. result];
    }

    private static IEnumerable<NdsFile> ReadSubFnt(BinaryReaderX br, FntEntry dirEntry, int fntOffset, int contentOffset, string path, IList<FntEntry> directoryEntries, IList<FatEntry> fatEntries)
    {
        var tableOffset = fntOffset + dirEntry.subTableOffset;
        var firstFileId = dirEntry.firstFileId;

        br.BaseStream.Position = tableOffset;

        var typeLength = br.ReadByte();
        while (typeLength != 0)
        {
            if (typeLength == 0x80)
                throw new InvalidOperationException("TypeLength 0x80 is reserved.");

            if (typeLength <= 0x7F)
            {
                // Read file
                var name = br.ReadString(typeLength);
                tableOffset = (int)br.BaseStream.Position;

                var currentFileEntry = fatEntries[firstFileId];
                yield return new NdsContentFile
                {
                    Stream = new SubStream(br.BaseStream, contentOffset + currentFileEntry.offset, currentFileEntry.endOffset - currentFileEntry.offset),
                    Path = string.IsNullOrEmpty(path) ? name : $"{path}/{name}",
                    FileId = firstFileId++
                };
            }
            else
            {
                // Read directory
                var nameLength = typeLength & 0x7F;
                var name = br.ReadString(nameLength);
                var dirEntryId = br.ReadUInt16();
                tableOffset = (int)br.BaseStream.Position;

                var subDirEntry = directoryEntries[dirEntryId & 0x0FFF];
                foreach (var file in ReadSubFnt(br, subDirEntry, fntOffset, contentOffset, string.IsNullOrEmpty(path) ? name : $"{path}/{name}", directoryEntries, fatEntries))
                    yield return file;
            }

            br.BaseStream.Position = tableOffset;
            typeLength = br.ReadByte();
        }
    }

    private static FntEntry[] ReadFntEntries(BinaryReaderX reader, int count)
    {
        var result = new FntEntry[count];

        for (var i = 0; i < count; i++)
            result[i] = ReadFntEntry(reader);

        return result;
    }

    private static FntEntry ReadFntEntry(BinaryReaderX reader)
    {
        return new FntEntry
        {
            subTableOffset = reader.ReadInt32(),
            firstFileId = reader.ReadInt16(),
            parentDirectory = reader.ReadUInt16()
        };
    }
}