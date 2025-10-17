using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Domain.Level5Management.Contract.DataClasses.Archives;

namespace UI.Layton1Tool.Messages;

public record SelectedPcmChangedMessage(Layton1NdsRom Rom, PcmFile[] Files);