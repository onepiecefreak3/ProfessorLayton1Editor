using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums.Texts;

namespace UI.Layton1Tool.Messages;

public record SelectedEventLanguageChangedMessage(Layton1NdsRom Rom, TextLanguage Language);