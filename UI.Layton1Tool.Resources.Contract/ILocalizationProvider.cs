﻿using ImGui.Forms.Localization;
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
    LocalizedString DialogFontRemoveCaption { get; }
    LocalizedString DialogFontRemoveText { get; }
    LocalizedString DialogFontRemappingCaption { get; }
    LocalizedString DialogFontRemappingText { get; }
    LocalizedString DialogFontRemappingRemap { get; }

    LocalizedString StatusNdsSelectError { get; }
    LocalizedString StatusNdsOpenError { get; }
    LocalizedString StatusNdsSaveError { get; }
    LocalizedString StatusFileSelectError { get; }
    LocalizedString StatusDirectorySelectError { get; }
    LocalizedString StatusFileOpenError { get; }
    LocalizedString StatusAnimationLoadError { get; }

    LocalizedString ImageMenuExportPng { get; }
    LocalizedString ImageExportText { get; }

    LocalizedString AnimationSpeedInputCaption { get; }

    LocalizedString FontSearchPlaceholder { get; }
    LocalizedString FontPreviewPlaceholder { get; }
    LocalizedString FontPreviewExport { get; }
    LocalizedString FontPreviewSettings { get; }
    LocalizedString FontGenerateCaption { get; }
    LocalizedString FontGenerateEditCaption { get; }
    LocalizedString FontGenerateRemoveCaption { get; }
    LocalizedString FontGenerateRemappingCaption { get; }

    LocalizedString FontPreviewSettingsCaption { get; }
    LocalizedString FontPreviewSettingsShowDebug { get; }
    LocalizedString FontPreviewSettingsSpacing { get; }
    LocalizedString FontPreviewSettingsLineHeight { get; }
    LocalizedString FontPreviewSettingsAlignment { get; }
    LocalizedString FontPreviewSettingsAlignmentLeft { get; }
    LocalizedString FontPreviewSettingsAlignmentCenter { get; }
    LocalizedString FontPreviewSettingsAlignmentRight { get; }

    LocalizedString FontGenerateDefaultCharacters { get; }

    LocalizedString DialogFontGenerateCaption { get; }
    LocalizedString DialogFontEditCaption { get; }
    LocalizedString DialogFontGenerateFamily { get; }
    LocalizedString DialogFontGenerateStyle { get; }
    LocalizedString DialogFontGenerateSize { get; }
    LocalizedString DialogFontGenerateBaseline { get; }
    LocalizedString DialogFontGenerateGlyphHeight { get; }
    LocalizedString DialogFontGenerateSpaceWidth { get; }
    LocalizedString DialogFontGenerateCharacters { get; }
    LocalizedString DialogFontGenerateCharactersReplace { get; }
    LocalizedString DialogFontGenerateStyleBold { get; }
    LocalizedString DialogFontGenerateStyleItalic { get; }
    LocalizedString DialogFontGenerateLoad { get; }
    LocalizedString DialogFontGenerateLoadCaption { get; }
    LocalizedString DialogFontGenerateSave { get; }
    LocalizedString DialogFontGenerateSaveCaption { get; }
    LocalizedString DialogFontGenerateProfile { get; }
    LocalizedString DialogFontGenerateGenerate { get; }
    LocalizedString DialogFontEditEdit { get; }
    LocalizedString DialogFontGeneratePaddingLeft { get; }
    LocalizedString DialogFontGeneratePaddingRight { get; }
}