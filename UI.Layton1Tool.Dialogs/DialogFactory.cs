using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using ImGui.Forms.Modals;
using Konnect.Contract.DataClasses.Plugin.File.Font;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Domain.NintendoManagement.Contract.DataClasses.Font;
using UI.Layton1Tool.Dialogs.Contract;
using UI.Layton1Tool.Dialogs.Contract.DataClasses;
using UI.Layton1Tool.Dialogs.Contract.Enums;

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

    public Modal CreatePreviewSettingsDialog(FontPreviewSettings settings)
    {
        return kernel.Get<FontPreviewSettingsDialog>(new ConstructorParameter("settings", settings));
    }

    public Modal CreateFontGenerationDialog(NftrData fontData, FontGenerationType type, string? selectedCharacters)
    {
        return kernel.Get<FontGenerationDialog>(
            new ConstructorParameter("fontData", fontData),
            new ConstructorParameter("type", type),
            new ConstructorParameter("selectedCharacters", selectedCharacters));
    }

    public Modal CreateFontRemappingDialog(NftrData fontData, IReadOnlyList<CharacterInfo> remapCharacters)
    {
        return kernel.Get<FontRemappingDialog>(
            new ConstructorParameter("fontData", fontData),
            new ConstructorParameter("remapCharacters", remapCharacters));
    }
}