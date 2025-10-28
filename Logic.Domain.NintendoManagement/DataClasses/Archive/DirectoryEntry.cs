using Logic.Domain.NintendoManagement.Contract.DataClasses.Archive;

namespace Logic.Domain.NintendoManagement.DataClasses.Archive;

class DirectoryEntry
{
    public required string Name { get; set; }

    public IList<DirectoryEntry> Directories { get; } = [];

    public IList<NdsContentFile> Files { get; } = [];

    public void AddDirectory(DirectoryEntry entry)
    {
        DirectoryEntry? existingDir = Directories.FirstOrDefault(x => x.Name == entry.Name);

        if (existingDir is null)
        {
            Directories.Add(entry);
            return;
        }

        foreach (DirectoryEntry dir in entry.Directories)
            existingDir.AddDirectory(dir);

        foreach (NdsContentFile file in entry.Files)
        {
            if (!existingDir.Files.Contains(file))
                existingDir.Files.Add(file);
        }
    }
}