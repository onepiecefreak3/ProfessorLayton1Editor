using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace UI.Layton1Tool.Messages;

public record SelectedFileChangedMessage(Layton1NdsFile File, object? Content);