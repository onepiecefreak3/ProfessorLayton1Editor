using ImGui.Forms.Localization;
using Logic.Business.Layton1ToolManagement.Contract.Enums;

namespace UI.Layton1Tool.Resources.Contract;

public interface ILocalizationProvider
{
    LocalizedString ApplicationTitle { get; }

    LocalizedString MenuFileCaption { get; }
    LocalizedString MenuFileOpenCaption { get; }
    LocalizedString MenuFileOpenShortcut { get; }
    LocalizedString MenuFileSaveCaption { get; }
    LocalizedString MenuFileSaveShortcut { get; }
    LocalizedString MenuFileSaveAsCaption { get; }
    LocalizedString MenuFileSaveAsShortcut { get; }
    LocalizedString MenuFileValidateCaption { get; }
    LocalizedString MenuFileSearchCaption { get; }
    LocalizedString MenuFileExtractCaption { get; }

    LocalizedString FileNdsHistoryPreviousShortcut { get; }
    LocalizedString FileNdsHistoryNextShortcut { get; }

    LocalizedString FileNdsSearchPlaceholder { get; }
    LocalizedString FileNdsSearchClear { get; }

    LocalizedString ScriptInstructionCaption { get; }

    LocalizedString DialogFileNdsOpenCaption { get; }
    LocalizedString DialogFileNdsOpenFilter { get; }
    LocalizedString DialogFileNdsSaveCaption { get; }
    LocalizedString DialogFileNdsSaveFilter { get; }
    LocalizedString DialogFileExtractCaption { get; }
    LocalizedString DialogDirectoryExtractCaption { get; }
    LocalizedString DialogDirectoryExtractProgress(float completion);
    LocalizedString DialogValidationCaption { get; }
    LocalizedString DialogValidationPath { get; }
    LocalizedString DialogValidationErrorCaption { get; }
    LocalizedString DialogValidationErrorText(Layton1Error error);
    LocalizedString DialogValidationProgress(float completion);
    LocalizedString DialogValidationCancelCaption { get; }
    LocalizedString DialogSearchCaption { get; }
    LocalizedString DialogSearchPath { get; }
    LocalizedString DialogSearchSubPath { get; }
    LocalizedString DialogSearchPlaceholderCaption { get; }
    LocalizedString DialogSearchExecuteCaption { get; }
    LocalizedString DialogSearchProgress(float completion);
    LocalizedString DialogSearchCancelCaption { get; }
    LocalizedString DialogUnsavedChangesCaption { get; }
    LocalizedString DialogUnsavedChangesText { get; }

    LocalizedString StatusNdsSelectError { get; }
    LocalizedString StatusNdsOpenError { get; }
    LocalizedString StatusNdsSaveError { get; }
    LocalizedString StatusFileSelectError { get; }
    LocalizedString StatusDirectorySelectError { get; }
    LocalizedString StatusFileOpenError { get; }
    LocalizedString StatusAnimationLoadError { get; }
}