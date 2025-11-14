using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.InternalContract.Files;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;
using Logic.Domain.Level5Management.Contract.Script.Gds;
using Logic.Domain.NintendoManagement.Contract.DataClasses.Font;
using Logic.Domain.NintendoManagement.Contract.Font;
using System.Text;
using Logic.Domain.Level5Management.Contract.Archives;
using Logic.Domain.Level5Management.Contract.DataClasses.Archives;

namespace Logic.Business.Layton1ToolManagement.Files;

class Layton1FileComposer(IGdsScriptWriter scriptWriter, INftrWriter fontWriter, IPcmWriter pcmWriter) : ILayton1FileComposer
{
    public Stream? Compose(object content, FileType type)
    {
        switch (type)
        {
            case FileType.Gds:
                if (content is not GdsScriptFile script)
                    return null;

                var scriptStream = new MemoryStream();
                scriptWriter.Write(script, scriptStream);

                return scriptStream;

            case FileType.Text:
                if (content is not string text)
                    return null;

                var writer = new StreamWriter(new MemoryStream(), Encoding.GetEncoding("Shift-JIS"));
                writer.Write(text);
                writer.Flush();

                return writer.BaseStream;

            case FileType.Pcm:
                if (content is not List<PcmFile> files)
                    return null;

                var pcmStream = new MemoryStream();
                pcmWriter.Write(files, pcmStream);

                return pcmStream;

            case FileType.Font:
                if (content is not NftrData fontData)
                    return null;

                var fontStream = new MemoryStream();
                fontWriter.Write(fontStream, fontData);

                return fontStream;

            case FileType.Binary:
                return content is not Stream binaryData ? null : binaryData;
        }

        return null;
    }
}