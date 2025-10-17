using Logic.Business.Layton1ToolManagement.Contract;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.InternalContract;

namespace Logic.Business.Layton1ToolManagement;

class Layton1NdsValidator(
    ILayton1Compressor compressor,
    ILayton1NdsFileManager fileManager,
    ILayton1FileTypeDetector typeDetector)
    : ILayton1NdsValidator
{
    public Layton1ValidationError? Validate(Layton1NdsFile file)
    {
        return DecompressFile(file)
               ?? DetectFileType(file, out FileType? fileType)
               ?? ParseFileData(file, fileType!.Value);
    }

    private Layton1ValidationError? DecompressFile(Layton1NdsFile file)
    {
        try
        {
            compressor.Decompress(file);
        }
        catch (Exception e)
        {
            return new Layton1ValidationError
            {
                File = file,
                Error = Layton1Error.Compression,
                Exception = e
            };
        }

        return null;
    }

    private Layton1ValidationError? DetectFileType(Layton1NdsFile file, out FileType? fileType)
    {
        fileType = null;

        try
        {
            fileType = typeDetector.Detect(file);
        }
        catch (Exception e)
        {
            return new Layton1ValidationError
            {
                File = file,
                Error = Layton1Error.FileType,
                Exception = e
            };
        }

        return null;
    }

    private Layton1ValidationError? ParseFileData(Layton1NdsFile file, FileType fileType)
    {
        try
        {
            _ = fileManager.Parse(file, out fileType);
        }
        catch (Exception e)
        {
            return new Layton1ValidationError
            {
                File = file,
                Error = Layton1Error.Content,
                FileType = fileType,
                Exception = e
            };
        }

        return null;
    }
}