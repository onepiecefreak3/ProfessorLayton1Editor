using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.InternalContract.Files;
using Logic.Domain.Level5Management.Contract.Animations;
using Logic.Domain.Level5Management.Contract.Archives;
using Logic.Domain.Level5Management.Contract.Images;
using Logic.Domain.Level5Management.Contract.Script.Gds;
using Logic.Domain.NintendoManagement.Contract.Font;
using Logic.Domain.NintendoManagement.Contract.Image;
using System.Text;

namespace Logic.Business.Layton1ToolManagement.Files;

class Layton1FileParser(
    IBgxParser bgxParser,
    IGdsScriptParser scriptParser,
    IPcmReader pcmReader,
    IFrame1Parser anim1Parser,
    IFrame2Parser anim2Parser,
    IFrame3Parser anim3Parser,
    INftrReader fontReader,
    IBannerReader bannerReader) : ILayton1FileParser
{
    public object? Parse(Stream input, FileType type, GameVersion version)
    {
        input.Position = 0;

        switch (type)
        {
            case FileType.Bgx:
                return bgxParser.Parse(input);

            case FileType.Gds:
                return scriptParser.Parse(input);

            case FileType.Text:
                switch (version)
                {
                    case GameVersion.Korea:
                        string text = new StreamReader(input, Encoding.BigEndianUnicode).ReadToEnd();
                        return text.TrimEnd('\0');

                    default:
                        return new StreamReader(input, Encoding.GetEncoding("Shift-JIS")).ReadToEnd();
                }

            case FileType.Pcm:
                return pcmReader.Read(input);

            case FileType.Anim:
                return anim1Parser.Parse(input);

            case FileType.Anim2:
                return anim2Parser.Parse(input);

            case FileType.Anim3:
                return anim3Parser.Parse(input);

            case FileType.Font:
                return fontReader.Read(input);

            case FileType.Banner:
                return bannerReader.Read(input);

            default:
                return null;
        }
    }
}