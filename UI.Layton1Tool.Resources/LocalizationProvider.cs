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
    public LocalizedString MenuFileViewCaption => LocalizedString.FromId("Menu.File.View.Caption");
    public LocalizedString MenuFileViewFilesCaption => LocalizedString.FromId("Menu.File.View.Files.Caption");
    public LocalizedString MenuFileViewPuzzlesCaption => LocalizedString.FromId("Menu.File.View.Puzzles.Caption");
    public LocalizedString MenuFileValidateCaption => LocalizedString.FromId("Menu.File.Validate.Caption");
    public LocalizedString MenuFileSearchCaption => LocalizedString.FromId("Menu.File.Search.Caption");
    public LocalizedString MenuFileExtractCaption => LocalizedString.FromId("Menu.File.Extract.Caption");
    public LocalizedString MenuFileImportCaption => LocalizedString.FromId("Menu.File.Import.Caption");

    public LocalizedString FileNdsHistoryPreviousShortcut => LocalizedString.FromId("File.Nds.History.Previous.Shortcut");
    public LocalizedString FileNdsHistoryNextShortcut => LocalizedString.FromId("File.Nds.History.Next.Shortcut");

    public LocalizedString FileNdsSearchPlaceholder => LocalizedString.FromId("File.Nds.Search.Placeholder");
    public LocalizedString FileNdsSearchClear => LocalizedString.FromId("File.Nds.Search.Clear");

    public LocalizedString PuzzleLanguageEnglishText => LocalizedString.FromId("Puzzle.Language.English.Text");
    public LocalizedString PuzzleLanguageGermanText => LocalizedString.FromId("Puzzle.Language.German.Text");
    public LocalizedString PuzzleLanguageSpanishText => LocalizedString.FromId("Puzzle.Language.Spanish.Text");
    public LocalizedString PuzzleLanguageFrenchText => LocalizedString.FromId("Puzzle.Language.French.Text");
    public LocalizedString PuzzleLanguageItalianText => LocalizedString.FromId("Puzzle.Language.Italian.Text");
    public LocalizedString PuzzleLanguageKoreanText => LocalizedString.FromId("Puzzle.Language.Korean.Text");
    public LocalizedString PuzzleLanguageJapaneseText => LocalizedString.FromId("Puzzle.Language.Japanese.Text");

    public LocalizedString PuzzleInfoInternalIdText => LocalizedString.FromId("Puzzle.Info.InternalId.Text");
    public LocalizedString PuzzleInfoNumberText => LocalizedString.FromId("Puzzle.Info.Number.Text");
    public LocalizedString PuzzleInfoTitleText => LocalizedString.FromId("Puzzle.Info.Title.Text");
    public LocalizedString PuzzleInfoLocationText => LocalizedString.FromId("Puzzle.Info.Location.Text");
    public LocalizedString PuzzleInfoTypeText => LocalizedString.FromId("Puzzle.Info.Type.Text");
    public LocalizedString PuzzleInfoPicaratText => LocalizedString.FromId("Puzzle.Info.Picarat.Text");
    public LocalizedString PuzzleInfoDescriptionText => LocalizedString.FromId("Puzzle.Info.Description.Text");
    public LocalizedString PuzzleInfoCorrectText => LocalizedString.FromId("Puzzle.Info.Correct.Text");
    public LocalizedString PuzzleInfoIncorrectText => LocalizedString.FromId("Puzzle.Info.Incorrect.Text");
    public LocalizedString PuzzleInfoHintsText => LocalizedString.FromId("Puzzle.Info.Hints.Text");

    public LocalizedString DialogFileNdsOpenCaption => LocalizedString.FromId("Dialog.File.Nds.Open.Caption");
    public LocalizedString DialogFileNdsOpenFilter => LocalizedString.FromId("Dialog.File.Nds.Open.Filter");
    public LocalizedString DialogFileNdsSaveCaption => LocalizedString.FromId("Dialog.File.Nds.Save.Caption");
    public LocalizedString DialogFileNdsSaveFilter => LocalizedString.FromId("Dialog.File.Nds.Save.Filter");
    public LocalizedString DialogFileExtractCaption => LocalizedString.FromId("Dialog.File.Extract.Caption");
    public LocalizedString DialogFileImportCaption => LocalizedString.FromId("Dialog.File.Import.Caption");
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
    public LocalizedString DialogFontRemoveCaption => LocalizedString.FromId("Dialog.Font.Remove.Caption");
    public LocalizedString DialogFontRemoveText => LocalizedString.FromId("Dialog.Font.Remove.Text");
    public LocalizedString DialogFontRemappingCaption => LocalizedString.FromId("Dialog.Font.Remapping.Caption");
    public LocalizedString DialogFontRemappingText => LocalizedString.FromId("Dialog.Font.Remapping.Text");
    public LocalizedString DialogFontRemappingRemap => LocalizedString.FromId("Dialog.Font.Remapping.Remap");

    public LocalizedString StatusNdsSelectError => LocalizedString.FromId("Status.Nds.Select.Error");
    public LocalizedString StatusNdsOpenError => LocalizedString.FromId("Status.Nds.Open.Error");
    public LocalizedString StatusNdsSaveError => LocalizedString.FromId("Status.Nds.Save.Error");
    public LocalizedString StatusFileSelectError => LocalizedString.FromId("Status.File.Select.Error");
    public LocalizedString StatusDirectorySelectError => LocalizedString.FromId("Status.Directory.Select.Error");
    public LocalizedString StatusFileOpenError => LocalizedString.FromId("Status.File.Open.Error");
    public LocalizedString StatusAnimationLoadError => LocalizedString.FromId("Status.Animation.Load.Error");

    public LocalizedString ImageMenuExportPng => LocalizedString.FromId("Image.Menu.Export.Png");
    public LocalizedString ImageExportText => LocalizedString.FromId("Image.Export.Text");

    public LocalizedString AnimationSpeedInputCaption => LocalizedString.FromId("Animation.Speed.Input.Caption");

    public LocalizedString FontSearchPlaceholder => LocalizedString.FromId("Font.Search.Placeholder");
    public LocalizedString FontPreviewPlaceholder => LocalizedString.FromId("Font.Preview.Placeholder");
    public LocalizedString FontPreviewExport => LocalizedString.FromId("Font.Preview.Export");
    public LocalizedString FontPreviewSettings => LocalizedString.FromId("Font.Preview.Settings");
    public LocalizedString FontGenerateCaption => LocalizedString.FromId("Font.Generate.Caption");
    public LocalizedString FontGenerateEditCaption => LocalizedString.FromId("Font.Generate.Edit.Caption");
    public LocalizedString FontGenerateRemoveCaption => LocalizedString.FromId("Font.Generate.Remove.Caption");
    public LocalizedString FontGenerateRemappingCaption => LocalizedString.FromId("Font.Generate.Remapping.Caption");

    public LocalizedString FontPreviewSettingsCaption => LocalizedString.FromId("Font.Preview.Settings.Caption");
    public LocalizedString FontPreviewSettingsShowDebug => LocalizedString.FromId("Font.Preview.Settings.ShowDebug");
    public LocalizedString FontPreviewSettingsSpacing => LocalizedString.FromId("Font.Preview.Settings.Spacing");
    public LocalizedString FontPreviewSettingsLineHeight => LocalizedString.FromId("Font.Preview.Settings.LineHeight");
    public LocalizedString FontPreviewSettingsAlignment => LocalizedString.FromId("Font.Preview.Settings.Alignment");
    public LocalizedString FontPreviewSettingsAlignmentLeft => LocalizedString.FromId("Font.Preview.Settings.Alignment.Left");
    public LocalizedString FontPreviewSettingsAlignmentCenter => LocalizedString.FromId("Font.Preview.Settings.Alignment.Center");
    public LocalizedString FontPreviewSettingsAlignmentRight => LocalizedString.FromId("Font.Preview.Settings.Alignment.Right");

    public LocalizedString FontGenerateDefaultCharacters => LocalizedString.FromId("Font.Generate.DefaultCharacters");

    public LocalizedString DialogFontGenerateCaption => LocalizedString.FromId("Dialog.Font.Generate.Caption");
    public LocalizedString DialogFontEditCaption => LocalizedString.FromId("Dialog.Font.Edit.Caption");
    public LocalizedString DialogFontGenerateFamily => LocalizedString.FromId("Dialog.Font.Generate.Family");
    public LocalizedString DialogFontGenerateStyle => LocalizedString.FromId("Dialog.Font.Generate.Style");
    public LocalizedString DialogFontGenerateSize => LocalizedString.FromId("Dialog.Font.Generate.Size");
    public LocalizedString DialogFontGenerateBaseline => LocalizedString.FromId("Dialog.Font.Generate.Baseline");
    public LocalizedString DialogFontGenerateGlyphHeight => LocalizedString.FromId("Dialog.Font.Generate.GlyphHeight");
    public LocalizedString DialogFontGenerateSpaceWidth => LocalizedString.FromId("Dialog.Font.Generate.SpaceWidth");
    public LocalizedString DialogFontGenerateCharacters => LocalizedString.FromId("Dialog.Font.Generate.Characters");
    public LocalizedString DialogFontGenerateCharactersReplace => LocalizedString.FromId("Dialog.Font.Generate.Characters.Replace");
    public LocalizedString DialogFontGenerateStyleBold => LocalizedString.FromId("Dialog.Font.Generate.Style.Bold");
    public LocalizedString DialogFontGenerateStyleItalic => LocalizedString.FromId("Dialog.Font.Generate.Style.Italic");
    public LocalizedString DialogFontGenerateLoad => LocalizedString.FromId("Dialog.Font.Generate.Load");
    public LocalizedString DialogFontGenerateLoadCaption => LocalizedString.FromId("Dialog.Font.Generate.Load.Caption");
    public LocalizedString DialogFontGenerateSave => LocalizedString.FromId("Dialog.Font.Generate.Save");
    public LocalizedString DialogFontGenerateSaveCaption => LocalizedString.FromId("Dialog.Font.Generate.Save.Caption");
    public LocalizedString DialogFontGenerateProfile => LocalizedString.FromId("Dialog.Font.Generate.Profile");
    public LocalizedString DialogFontGenerateGenerate => LocalizedString.FromId("Dialog.Font.Generate.Generate");
    public LocalizedString DialogFontEditEdit => LocalizedString.FromId("Dialog.Font.Edit.Edit");
    public LocalizedString DialogFontGeneratePaddingLeft => LocalizedString.FromId("Dialog.Font.Generate.Padding.Left");
    public LocalizedString DialogFontGeneratePaddingRight => LocalizedString.FromId("Dialog.Font.Generate.Padding.Right");
}