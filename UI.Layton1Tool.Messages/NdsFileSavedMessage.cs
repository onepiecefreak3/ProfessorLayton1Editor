using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace UI.Layton1Tool.Messages;

public record NdsFileSavedMessage(Layton1NdsRom OriginalRom, string RomPath, Layton1NdsRom Rom);