using Komponent.IO;
using Logic.Domain.Level5Management.Contract.Archives;
using Logic.Domain.Level5Management.Contract.DataClasses.Archives;
using Logic.Domain.Level5Management.DataClasses.Archives;

namespace Logic.Domain.Level5Management.Archives;

internal class PcmWriter : IPcmWriter
{
    public void Write(List<PcmFile> files, Stream output)
    {
        using var writer = new BinaryWriterX(output, true);

        PcmEntry[] entries = CreateEntries(files);

        output.Position = 0x10;
        WriteEntries(entries, writer);

        var header = new PcmHeader
        {
            headerSize = 0x10,
            sectionSize = (int)output.Length,
            fileCount = files.Count,
            magic = "LPCK"
        };

        output.Position = 0;
        WriteHeader(header, writer);
    }

    private PcmEntry[] CreateEntries(List<PcmFile> files)
    {
        var result = new PcmEntry[files.Count];

        for (var i = 0; i < files.Count; i++)
            result[i] = CreateEntry(files[i]);

        return result;
    }

    private PcmEntry CreateEntry(PcmFile file)
    {
        int headerSize = 0x10 + ((file.Name.Length + 16) & ~15);
        int sectionSize = (headerSize + (int)file.Data.Length + 15) & ~15;

        if (file.Data.Length % 0x10 is 0)
            sectionSize += 0x10;

        return new PcmEntry
        {
            headerSize = headerSize,
            sectionSize = sectionSize,
            zero0 = 0,
            fileSize = (int)file.Data.Length,
            fileName = file.Name,
            fileData = file.Data
        };
    }

    private void WriteEntries(PcmEntry[] entries, BinaryWriterX writer)
    {
        foreach (PcmEntry entry in entries.OrderBy(e => e.fileName))
            WriteEntry(entry, writer);
    }

    private void WriteEntry(PcmEntry entry, BinaryWriterX writer)
    {
        writer.Write(entry.headerSize);
        writer.Write(entry.sectionSize);
        writer.Write(entry.zero0);
        writer.Write(entry.fileSize);

        writer.WriteString(entry.fileName);
        writer.WriteAlignment(0x10);

        entry.fileData.Position = 0;
        entry.fileData.CopyTo(writer.BaseStream);

        if (entry.fileData.Length % 0x10 is 0)
            writer.WritePadding(0x10);
        else
            writer.WriteAlignment(0x10);
    }

    private void WriteHeader(PcmHeader header, BinaryWriterX writer)
    {
        writer.Write(header.headerSize);
        writer.Write(header.sectionSize);
        writer.Write(header.fileCount);
        writer.WriteString(header.magic, writeNullTerminator: false);
    }
}