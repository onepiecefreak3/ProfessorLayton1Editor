using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Domain.NintendoManagement.Contract.DataClasses.Font;

namespace UI.Layton1Tool.Messages;

public record SelectedFontChangedMessage(Layton1NdsRom Rom, NftrData Font);