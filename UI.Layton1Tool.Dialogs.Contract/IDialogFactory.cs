using ImGui.Forms.Modals;
using Konnect.Contract.DataClasses.Plugin.File.Font;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Domain.NintendoManagement.Contract.DataClasses.Font;
using UI.Layton1Tool.Dialogs.Contract.DataClasses;
using UI.Layton1Tool.Dialogs.Contract.Enums;

namespace UI.Layton1Tool.Dialogs.Contract;

public interface IDialogFactory
{
    Modal CreateValidationDialog(Layton1NdsRom ndsRom);
    Modal CreateSearchDialog(Layton1NdsRom ndsRom);
    Modal CreatePreviewSettingsDialog(FontPreviewSettings settings);
    Modal CreateFontGenerationDialog(NftrData fontData, FontGenerationType type, string? selectedCharacters = null);
    Modal CreateFontRemappingDialog(NftrData fontData, IReadOnlyList<CharacterInfo> remapCharacters);
}