using Kaligraphy.Contract.DataClasses.Parsing;

namespace UI.Layton1Tool.Messages.DataClasses;

public class TextElement
{
    public required int TextId { get; set; }
    public required string SpeakerWindow { get; set; }
    public required List<CharacterData>[] Texts { get; set; }
}