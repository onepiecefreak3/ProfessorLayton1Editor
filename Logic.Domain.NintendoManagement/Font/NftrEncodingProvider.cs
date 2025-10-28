using Kanvas.Contract.Encoding;
using Kanvas.Encoding;
using Konnect.Plugin.File.Image;

namespace Logic.Domain.NintendoManagement.Font;

class NftrEncodingProvider
{
    private readonly Dictionary<int, IColorEncoding> _nftrFormat = new()
    {
        [1] = new La(0, 1),
        [2] = new La(0, 2)
    };

    public EncodingDefinition GetEncodingDefinitions()
    {
        var encodingDefinition = new EncodingDefinition();
        encodingDefinition.AddColorEncodings(_nftrFormat);

        return encodingDefinition;
    }
}