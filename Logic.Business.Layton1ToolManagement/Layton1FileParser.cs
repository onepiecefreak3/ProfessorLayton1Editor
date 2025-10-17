using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.InternalContract;
using Logic.Domain.Level5Management.Contract.Archives;
using Logic.Domain.Level5Management.Contract.Images;
using System.Text;
using Logic.Domain.Level5Management.Contract.Animations;

namespace Logic.Business.Layton1ToolManagement;

class Layton1FileParser(
    IBgxParser bgxParser,
    ILayton1ScriptConverter scriptConverter,
    IPcmParser pcmParser,
    IFrame1Parser anim1Parser,
    IFrame2Parser anim2Parser,
    IFrame3Parser anim3Parser) : ILayton1FileParser
{
    public object? Parse(Stream input, FileType type)
    {
        switch (type)
        {
            case FileType.Bgx:
                return bgxParser.Parse(input);

            case FileType.Gds:
                return scriptConverter.Parse(input);

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

            default:
                return null;
        }
    }
}