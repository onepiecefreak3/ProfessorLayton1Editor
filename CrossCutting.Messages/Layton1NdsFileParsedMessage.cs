using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums;

namespace CrossCutting.Messages;

public record Layton1NdsFileParsedMessage(Layton1NdsFile File, object Data, FileType Type);