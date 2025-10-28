using Konnect.Contract.DataClasses.Plugin.File.Font;

namespace Logic.Domain.NintendoManagement.Contract.DataClasses.Font;

public class NftrData
{
    public List<CharacterInfo> Characters { get; set; }
    public NftrMetaData MetaData { get; set; }
    public NftrImageData ImageData { get; set; }
}