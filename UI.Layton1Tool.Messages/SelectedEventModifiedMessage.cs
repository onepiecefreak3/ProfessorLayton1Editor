using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;

namespace UI.Layton1Tool.Messages;

public record SelectedEventModifiedMessage(Layton1NdsRom Rom, int Event, GdsScriptFile Script);