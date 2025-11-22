using Kaligraphy.Contract.Rendering;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using UI.Layton1Tool.Messages.Enums;

namespace UI.Layton1Tool.Messages;

public record FontModifiedMessage(Layton1NdsFile File, IGlyphProvider Font, FontType Type);
