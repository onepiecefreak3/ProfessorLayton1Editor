using CrossCutting.Core.Contract.EventBrokerage;
using CrossCutting.Messages;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.Contract.Files;
using Logic.Business.Layton1ToolManagement.InternalContract.Compression;
using Logic.Business.Layton1ToolManagement.InternalContract.Files;
using System.Diagnostics.CodeAnalysis;

namespace Logic.Business.Layton1ToolManagement.Files;

class Layton1NdsFileManager(
    IEventBroker eventBroker,
    ILayton1Compressor compressor,
    ILayton1FileTypeDetector detector,
    ILayton1FileParser fileParser,
    ILayton1FileComposer fileComposer) : ILayton1NdsFileManager
{
    public bool TryGet(Layton1NdsRom rom, string path, [NotNullWhen(true)] out Layton1NdsFile? file)
    {
        file = rom.Files.FirstOrDefault(f => f.Path == path);
        return file is not null;
    }

    public Layton1NdsFile Add(Layton1NdsRom rom, string path, object content, FileType type, CompressionType compression)
    {
        var file = new Layton1NdsContentFile
        {
            Rom = rom,
            Path = path,
            DataStream = new MemoryStream(),
            CompressionType = compression,
            IsChanged = true,
            FileId = rom.Files.Count
        };
        Compose(file, content, type);

        rom.Files.Add(file);

        return file;
    }

    public FileType Detect(Layton1NdsFile file)
    {
        return detector.Detect(file);
    }

    public void Compress(Layton1NdsFile file)
    {
        compressor.Compress(file);
    }

    public void Decompress(Layton1NdsFile file)
    {
        compressor.Decompress(file);
    }

    public Stream GetUncompressedStream(Layton1NdsFile file)
    {
        Decompress(file);

        return file.DecompressedStream ?? file.DataStream;
    }

    public void SetUncompressedStream(Layton1NdsFile file, Stream input)
    {
        if (file.DecompressedStream == input)
            return;

        if (file.CompressionType is CompressionType.None)
            file.DataStream = input;
        else
            file.DecompressedStream = input;

        file.IsChanged = true;
    }

    public object? Parse(Layton1NdsFile file, out FileType type)
    {
        type = Detect(file);

        return Parse(file, type);
    }

    public object? Parse(Layton1NdsFile file, FileType type)
    {
        Stream fileStream = GetUncompressedStream(file);
        object? data = fileParser.Parse(fileStream, type, file.Rom.Version);

        if (data is null)
            return null;

        eventBroker.Raise(new Layton1NdsFileParsedMessage(file, data, type));

        return data;
    }

    public void Compose(Layton1NdsFile file, object content)
    {
        FileType type = Detect(file);

        Compose(file, content, type);
    }

    public void Compose(Layton1NdsFile file, object content, FileType type)
    {
        Stream? output = fileComposer.Compose(content, type, file.Rom.Version);
        if (output is null)
            return;

        output.Position = 0;

        if (file.CompressionType is CompressionType.None)
            file.DataStream = output;
        else
            file.DecompressedStream = output;

        file.IsChanged = true;
    }
}