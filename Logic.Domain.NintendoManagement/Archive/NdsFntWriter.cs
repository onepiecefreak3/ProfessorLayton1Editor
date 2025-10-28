using System.Text;
using Komponent.IO;
using Logic.Domain.NintendoManagement.Contract.DataClasses.Archive;
using Logic.Domain.NintendoManagement.DataClasses.Archive;
using Logic.Domain.NintendoManagement.InternalContract;

namespace Logic.Domain.NintendoManagement.Archive;

class NdsFntWriter : INdsFntWriter
{
    public void WriteFnt(Stream input, int fntOffset, NdsContentFile[] files, int startFileId)
    {
        using var writer = new BinaryWriterX(input, true);

        var fileTree = CreateDirectoryTree(files);
        var totalDirectories = CountTotalDirectories(fileTree);
        var contentOffset = fntOffset + totalDirectories * 0x8;

        var baseOffset = fntOffset;
        var fileId = startFileId;
        var dirId = 0;
        WriteFnt(writer, baseOffset, ref fntOffset, ref contentOffset, ref fileId, ref dirId, 0, fileTree);

        // Write total directories
        input.Position = baseOffset + 6;
        writer.Write((short)totalDirectories);

        input.Position = contentOffset;
    }

    private static void WriteFnt(BinaryWriterX bw, int baseOffset, ref int fntOffset, ref int contentOffset, ref int fileId, ref int dirId, int parentDirId, DirectoryEntry entry)
    {
        // Write dir entry
        bw.BaseStream.Position = fntOffset;
        fntOffset += 8;

        var fntEntry = new FntEntry
        {
            subTableOffset = contentOffset - baseOffset,
            firstFileId = (short)fileId,
            parentDirectory = (ushort)(0xF000 + parentDirId)
        };
        WriteFntEntry(fntEntry, bw);

        // Write file names
        bw.BaseStream.Position = contentOffset;

        foreach (NdsContentFile file in entry.Files)
        {
            bw.WriteString(Path.GetFileName(file.Path), Encoding.ASCII, true, false);
            file.FileId = fileId++;
        }
        contentOffset = (int)bw.BaseStream.Position;

        // Write directory entries
        var nextContentOffset = (int)(bw.BaseStream.Position + entry.Directories.Sum(x => x.Name.Length + 3) + 1);
        int currentDirId = dirId;

        foreach (DirectoryEntry dir in entry.Directories)
        {
            bw.BaseStream.Position = contentOffset;

            bw.Write((byte)(dir.Name.Length + 0x80));
            bw.WriteString(dir.Name, Encoding.ASCII, false, false);
            bw.Write((ushort)(0xF000 + ++dirId));

            contentOffset = (int)bw.BaseStream.Position;

            WriteFnt(bw, baseOffset, ref fntOffset, ref nextContentOffset, ref fileId, ref dirId, currentDirId, dir);
        }

        contentOffset = nextContentOffset;
    }

    private static void WriteFntEntry(FntEntry entry, BinaryWriterX writer)
    {
        writer.Write(entry.subTableOffset);
        writer.Write(entry.firstFileId);
        writer.Write(entry.parentDirectory);
    }

    private static int CountTotalDirectories(DirectoryEntry dirEntry)
    {
        var result = 1;

        foreach (DirectoryEntry entry in dirEntry.Directories)
            result += CountTotalDirectories(entry);

        return result;
    }

    private static DirectoryEntry CreateDirectoryTree(NdsContentFile[] files)
    {
        var root = new DirectoryEntry { Name = string.Empty };

        foreach (NdsContentFile file in files)
        {
            DirectoryEntry parent = root;

            string? directory = Path.GetDirectoryName(file.Path);
            if (!string.IsNullOrEmpty(directory))
            {
                foreach (string part in directory.Split('\\'))
                {
                    DirectoryEntry? entry = parent.Directories.FirstOrDefault(x => x.Name == part);
                    if (entry == null)
                    {
                        entry = new DirectoryEntry { Name = part };
                        parent.AddDirectory(entry);
                    }

                    parent = entry;
                }
            }

            parent.Files.Add(file);
        }

        return root;
    }
}