using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.Contract.Files;
using Logic.Business.Layton1ToolManagement.InternalContract.Files;
using Logic.Domain.Level5Management.Contract.DataClasses.Archives;
using System.Diagnostics.CodeAnalysis;

namespace Logic.Business.Layton1ToolManagement.Files;

class Layton1PcmFileManager(ILayton1FileTypeDetector detector, ILayton1FileParser fileParser, ILayton1FileComposer fileComposer) : ILayton1PcmFileManager
{
    public bool TryGet(List<PcmFile> files, string name, [NotNullWhen(true)] out PcmFile? file)
    {
        file = files.FirstOrDefault(f => f.Name == name);
        return file is not null;
    }

    public PcmFile Add(List<PcmFile> files, string name, object content, FileType type, GameVersion version)
    {
        var file = new PcmFile
        {
            Name = name,
            Data = new MemoryStream()
        };
        files.Add(file);

        Compose(file, content, type, version);

        return file;
    }

    public FileType Detect(PcmFile file)
    {
        return detector.Detect(file);
    }

    public object? Parse(PcmFile file, FileType type, GameVersion version)
    {
        return fileParser.Parse(file.Data, type, version);
    }

    public void Compose(PcmFile file, object content, FileType type, GameVersion version)
    {
        Stream? fileData = fileComposer.Compose(content, type, version);

        if (fileData is null)
            return;

        file.Data = fileData;
    }
}