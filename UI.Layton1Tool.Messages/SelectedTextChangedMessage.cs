using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace UI.Layton1Tool.Messages;

public record SelectedTextChangedMessage(Layton1NdsRom Rom, string Text);