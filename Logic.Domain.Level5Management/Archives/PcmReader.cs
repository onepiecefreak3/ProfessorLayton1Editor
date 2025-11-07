using Komponent.IO;
using Logic.Domain.Level5Management.Contract.Archives;
using Logic.Domain.Level5Management.Contract.DataClasses.Archives;
using Logic.Domain.Level5Management.DataClasses.Archives;

namespace Logic.Domain.Level5Management.Archives;

class PcmReader : IPcmReader
{
    public PcmFile[] Read(Stream input)
    {
        using var reader = new BinaryReaderX(input, true);

        PcmHeader header = ReadHeader(reader);
        PcmEntry[] entries = ReadEntries(reader, header.fileCount);

        return entries.Select(e => new PcmFile
        {
            Name = e.fileName,
            Data = e.fileData
        }).ToArray();
    }

    private PcmHeader ReadHeader(BinaryReaderX reader)
    {
        return new PcmHeader
        {
            headerSize = reader.ReadInt32(),
            sectionSize = reader.ReadInt32(),
            fileCount = reader.ReadInt32(),
            magic = reader.ReadString(4)
        };
    }

    private PcmEntry[] ReadEntries(BinaryReaderX reader, int count)
    {
        var result = new PcmEntry[count];

        for (var i = 0; i < count; i++)
            result[i] = ReadEntry(reader);

        return result;
    }

    private PcmEntry ReadEntry(BinaryReaderX reader)
    {
        long position = reader.BaseStream.Position;

        var entry = new PcmEntry
        {
            headerSize = reader.ReadInt32(),
            sectionSize = reader.ReadInt32(),
            zero0 = reader.ReadInt32(),
            fileSize = reader.ReadInt32(),
            fileName = reader.ReadNullTerminatedString()
        };

        reader.SeekAlignment();

        byte[] fileData = reader.ReadBytes(entry.fileSize);
        entry.fileData = new MemoryStream(fileData);

        reader.BaseStream.Position = position + entry.sectionSize;

        return entry;
    }
}