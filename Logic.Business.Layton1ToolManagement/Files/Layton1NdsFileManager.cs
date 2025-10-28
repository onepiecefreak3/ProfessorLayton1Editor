using CrossCutting.Core.Contract.EventBrokerage;
using CrossCutting.Messages;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.Contract.Files;
using Logic.Business.Layton1ToolManagement.InternalContract.Compression;
using Logic.Business.Layton1ToolManagement.InternalContract.Files;

namespace Logic.Business.Layton1ToolManagement.Files;

class Layton1NdsFileManager(
    IEventBroker eventBroker,
    ILayton1Compressor compressor,
    ILayton1FileTypeDetector detector,
    ILayton1FileParser fileParser,
    ILayton1FileComposer fileComposer) : ILayton1NdsFileManager
{
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

    public object? Parse(Layton1NdsFile file, out FileType type)
    {
        Decompress(file);

        type = Detect(file);

        return Parse(file, type);
    }

    public object? Parse(Layton1NdsFile file, FileType type)
    {
        Decompress(file);

        object? data = fileParser.Parse(GetUncompressedStream(file), type, file.Rom.GameCode);

        if (data is null)
            return null;

        eventBroker.Raise(new Layton1NdsFileParsedMessage(file, data, type));

        return data;
    }

    public void Compose(Layton1NdsFile file, object content)
    {
        Decompress(file);

        FileType type = Detect(file);

        Stream? output = fileComposer.Compose(content, type, file.Rom.GameCode);
        if (output is null)
            return;

        output.Position = 0;

        if (file.CompressionType is CompressionType.None)
            file.DataStream = output;
        else
            file.DecompressedStream = output;

        file.IsChanged = true;
    }

    public FileType Detect(Layton1NdsFile file)
    {
        return detector.Detect(file);
    }
}