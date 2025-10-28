using System.Text;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.InternalContract.Files;
using Logic.Business.Layton1ToolManagement.InternalContract.Scripts;
using Logic.Domain.Level5Management.Contract.Animations;
using Logic.Domain.Level5Management.Contract.Archives;
using Logic.Domain.Level5Management.Contract.Images;
using Logic.Domain.NintendoManagement.Contract.Font;
using Logic.Domain.NintendoManagement.Contract.Image;

namespace Logic.Business.Layton1ToolManagement.Files;

class Layton1FileParser(
    IBgxParser bgxParser,
    ILayton1ScriptConverter scriptConverter,
    IPcmParser pcmParser,
    IFrame1Parser anim1Parser,
    IFrame2Parser anim2Parser,
    IFrame3Parser anim3Parser,
    INftrReader fontReader,
    IBannerReader bannerReader) : ILayton1FileParser
{
    public object? Parse(Stream input, FileType type, string gameCode)
    {
        switch (type)
        {
            case FileType.Bgx:
                return bgxParser.Parse(input);

            case FileType.Gds:
                return scriptConverter.Parse(input, gameCode);

            case FileType.Text:
                return new StreamReader(input, Encoding.GetEncoding("Shift-JIS")).ReadToEnd();

            case FileType.Pcm:
                return pcmParser.Parse(input);

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