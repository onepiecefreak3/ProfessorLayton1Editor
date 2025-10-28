using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.InternalContract.Files;
using Logic.Business.Layton1ToolManagement.InternalContract.Scripts;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Level5;
using Logic.Domain.NintendoManagement.Contract.DataClasses.Font;
using Logic.Domain.NintendoManagement.Contract.Font;

namespace Logic.Business.Layton1ToolManagement.Files;

class Layton1FileComposer(ILayton1ScriptConverter scriptConverter, INftrWriter fontWriter) : ILayton1FileComposer
{
    public Stream? Compose(object content, FileType type, string gameCode)
    {
        switch (type)
        {
            case FileType.Bgx:
                return null;

            case FileType.Gds:
                if (content is not CodeUnitSyntax syntax)
                    return null;

                return scriptConverter.Compose(syntax, gameCode);

            case FileType.Text:
                return null;

            case FileType.Pcm:
                return null;

            case FileType.Anim:
                return null;

            case FileType.Anim2:
                return null;

            case FileType.Anim3:
                return null;

            case FileType.Font:
                if (content is not NftrData fontData)
                    return null;

                // TODO: Make FontComposer or let FontWriter return Stream
                var fontStream = new MemoryStream();
                fontWriter.Write(fontStream, fontData);

                return fontStream;
        }

        return null;
    }
}