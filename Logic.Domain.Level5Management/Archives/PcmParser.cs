using Logic.Domain.Level5Management.Contract.Archives;
using Logic.Domain.Level5Management.Contract.DataClasses.Archives;

namespace Logic.Domain.Level5Management.Archives;

class PcmParser(IPcmReader reader) : IPcmParser
{
    public PcmFile[] Parse(Stream input)
    {
        PcmContainer container = reader.Read(input);

        return Parse(container);
    }

    public PcmFile[] Parse(PcmContainer container)
    {
        var result = new PcmFile[container.Entries.Length];

        for (var i = 0; i < container.Entries.Length; i++)
        {
            result[i] = new PcmFile
            {
                FileName = container.Entries[i].fileName,
                FileData = container.Entries[i].fileData
            };
        }

        return result;
    }
}