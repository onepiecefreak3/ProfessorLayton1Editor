using ImGui.Forms.Localization;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Resources;

internal class LocalizationProvider : ILocalizationProvider
{
    public LocalizedString ApplicationTitle => LocalizedString.FromId("Application.Title");

    public LocalizedString MenuFileCaption => LocalizedString.FromId("Menu.File.Caption");
    public LocalizedString MenuFileOpenCaption => LocalizedString.FromId("Menu.File.Open.Caption");
    public LocalizedString MenuFileOpenShortcut => LocalizedString.FromId("Menu.File.Open.Shortcut");
    public LocalizedString MenuFileSaveCaption => LocalizedString.FromId("Menu.File.Save.Caption");
    public LocalizedString MenuFileSaveShortcut => LocalizedString.FromId("Menu.File.Save.Shortcut");
    public LocalizedString MenuFileSaveAsCaption => LocalizedString.FromId("Menu.File.SaveAs.Caption");
    public LocalizedString MenuFileSaveAsShortcut => LocalizedString.FromId("Menu.File.SaveAs.Shortcut");
    public LocalizedString MenuFileValidateCaption => LocalizedString.FromId("Menu.File.Validate.Caption");
    public LocalizedString MenuFileSearchCaption => LocalizedString.FromId("Menu.File.Search.Caption");
    public LocalizedString MenuFileExtractCaption => LocalizedString.FromId("Menu.File.Extract.Caption");

    public LocalizedString FileNdsHistoryPreviousShortcut => LocalizedString.FromId("File.Nds.History.Previous.Shortcut");
    public LocalizedString FileNdsHistoryNextShortcut => LocalizedString.FromId("File.Nds.History.Next.Shortcut");

    public LocalizedString FileNdsSearchPlaceholder => LocalizedString.FromId("File.Nds.Search.Placeholder");
    public LocalizedString FileNdsSearchClear => LocalizedString.FromId("File.Nds.Search.Clear");

    public LocalizedString ScriptInstructionCaption => LocalizedString.FromId("Script.Instruction.Caption");

    public LocalizedString DialogFileNdsOpenCaption => LocalizedString.FromId("Dialog.File.Nds.Open.Caption");
    public LocalizedString DialogFileNdsOpenFilter => LocalizedString.FromId("Dialog.File.Nds.Open.Filter");
    public LocalizedString DialogFileNdsSaveCaption => LocalizedString.FromId("Dialog.File.Nds.Save.Caption");
    public LocalizedString DialogFileNdsSaveFilter => LocalizedString.FromId("Dialog.File.Nds.Save.Filter");
    public LocalizedString DialogFileExtractCaption => LocalizedString.FromId("Dialog.File.Extract.Caption");
    public LocalizedString DialogDirectoryExtractCaption => LocalizedString.FromId("Dialog.Directory.Extract.Caption");
    public LocalizedString DialogDirectoryExtractProgress(float completion) => LocalizedString.FromId("Dialog.Directory.Extract.Progress", () => completion);
    public LocalizedString DialogValidationCaption => LocalizedString.FromId("Dialog.Validation.Caption");
    public LocalizedString DialogValidationPath => LocalizedString.FromId("Dialog.Validation.Path");
    public LocalizedString DialogValidationErrorCaption => LocalizedString.FromId("Dialog.Validation.Error");
    public LocalizedString DialogValidationErrorText(Layton1Error error) => LocalizedString.FromId($"Dialog.Validation.Error.{error}");
    public LocalizedString DialogValidationProgress(float completion) => LocalizedString.FromId("Dialog.Validation.Progress", () => completion);
    public LocalizedString DialogValidationCancelCaption => LocalizedString.FromId("Dialog.Validation.Cancel.Caption");
    public LocalizedString DialogSearchCaption => LocalizedString.FromId("Dialog.Search.Caption");
    public LocalizedString DialogSearchPath => LocalizedString.FromId("Dialog.Search.Path");
    public LocalizedString DialogSearchSubPath => LocalizedString.FromId("Dialog.Search.SubPath");
    public LocalizedString DialogSearchPlaceholderCaption => LocalizedString.FromId("Dialog.Search.Placeholder.Caption");
    public LocalizedString DialogSearchExecuteCaption => LocalizedString.FromId("Dialog.Search.Execute.Caption");
    public LocalizedString DialogSearchProgress(float completion) => LocalizedString.FromId("Dialog.Search.Progress", () => completion);
    public LocalizedString DialogSearchCancelCaption => LocalizedString.FromId("Dialog.Search.Cancel.Caption");
    public LocalizedString DialogUnsavedChangesCaption => LocalizedString.FromId("Dialog.UnsavedChanges.Caption");
    public LocalizedString DialogUnsavedChangesText => LocalizedString.FromId("Dialog.UnsavedChanges.Text");

    public LocalizedString StatusNdsSelectError => LocalizedString.FromId("Status.Nds.Select.Error");
    public LocalizedString StatusNdsOpenError => LocalizedString.FromId("Status.Nds.Open.Error");
    public LocalizedString StatusNdsSaveError => LocalizedString.FromId("Status.Nds.Save.Error");
    public LocalizedString StatusFileSelectError => LocalizedString.FromId("Status.File.Select.Error");
    public LocalizedString StatusDirectorySelectError => LocalizedString.FromId("Status.Directory.Select.Error");
    public LocalizedString StatusFileOpenError => LocalizedString.FromId("Status.File.Open.Error");
    public LocalizedString StatusAnimationLoadError => LocalizedString.FromId("Status.Animation.Load.Error");
}