using Kaligraphy.Contract.Rendering;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace UI.Layton1Tool.Forms.InternalContract;

public interface IFontProvider
{
    IGlyphProvider? GetQuestionFont(Layton1NdsRom rom);
    IGlyphProvider? GetEventFont(Layton1NdsRom rom);
    IGlyphProvider? GetFuriganaFont(Layton1NdsRom rom);

    void Free(Layton1NdsRom rom);
}