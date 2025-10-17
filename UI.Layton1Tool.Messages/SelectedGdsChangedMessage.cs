using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Level5;

namespace UI.Layton1Tool.Messages;

public record SelectedGdsChangedMessage(Layton1NdsRom Rom, CodeUnitSyntax Script);