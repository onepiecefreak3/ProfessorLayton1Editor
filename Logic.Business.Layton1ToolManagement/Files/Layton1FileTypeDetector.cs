using Komponent.IO;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.InternalContract.Compression;
using Logic.Business.Layton1ToolManagement.InternalContract.Files;
using Logic.Domain.Level5Management.Contract.DataClasses.Archives;

namespace Logic.Business.Layton1ToolManagement.Files;

class Layton1FileTypeDetector(ILayton1Compressor compressor) : ILayton1FileTypeDetector
{
    public FileType Detect(PcmFile file)
    {
        file.Data.Position = 0;

        return Detect(file.Name, file.Data);
    }

    public FileType Detect(Layton1NdsFile file)
    {
        compressor.Decompress(file);

        return Detect(file.Path, file.DecompressedStream ?? file.DataStream);
    }

    private static FileType Detect(string filePath, Stream fileStream)
    {
        if (filePath is "sys/banner.bin")
            return FileType.Banner;

        string extension = Path.GetExtension(filePath);
        switch (extension)
        {
            case ".bgx":
                return FileType.Bgx;

            case ".gds":
                return FileType.Gds;

            case ".pcm":
                return FileType.Pcm;

            case ".txt":
                return FileType.Text;

            case ".mods":
            case ".vx":
                return FileType.Movie;

            case ".NFTR":
                return FileType.Font;

            case ".arc":
            case ".arj":
                return DetectByContent(fileStream);

            default:
                return FileType.Binary;
        }
    }

    private static FileType DetectByContent(Stream input)
    {
        using var reader = new BinaryReaderX(input, true);

        var check = (uint)reader.ReadBytes(0x1E).Sum(x => x);
        if (check is 0)
        {
            input.Position -= 0x1E;
            return FileType.Anim3;
        }

        input.Position -= 0x1E;

        check = reader.ReadUInt32();
        if (check >> 16 is 0)
        {
            input.Position -= 4;
            return FileType.Bgx;
        }

        check = reader.ReadUInt32();
        input.Position -= 8;

        return check >> 16 > 0 ? FileType.Anim : FileType.Anim2;
    }
}