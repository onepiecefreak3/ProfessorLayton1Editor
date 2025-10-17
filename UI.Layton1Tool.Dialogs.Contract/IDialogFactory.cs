using ImGui.Forms.Modals;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;

namespace UI.Layton1Tool.Dialogs.Contract;

public interface IDialogFactory
{
    Modal CreateValidationDialog(Layton1NdsRom ndsRom);
    Modal CreateSearchDialog(Layton1NdsRom ndsRom);
}