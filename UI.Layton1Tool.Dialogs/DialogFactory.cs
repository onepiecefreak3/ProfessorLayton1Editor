using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using ImGui.Forms.Modals;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using UI.Layton1Tool.Dialogs.Contract;

namespace UI.Layton1Tool.Dialogs;

class DialogFactory(ICoCoKernel kernel) : IDialogFactory
{
    public Modal CreateValidationDialog(Layton1NdsRom ndsRom)
    {
        return kernel.Get<ValidationDialog>(new ConstructorParameter("ndsRom", ndsRom));
    }

    public Modal CreateSearchDialog(Layton1NdsRom ndsRom)
    {
        return kernel.Get<SearchDialog>(new ConstructorParameter("ndsRom", ndsRom));
    }
}