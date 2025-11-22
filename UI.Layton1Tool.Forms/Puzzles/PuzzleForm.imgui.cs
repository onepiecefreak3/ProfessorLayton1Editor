using System.Numerics;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Tree;
using ImGui.Forms.Models;
using ImGui.Forms.Models.IO;
using ImGuiNET;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.Contract.Enums.Texts;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Resources.Contract;
using Veldrid;

namespace UI.Layton1Tool.Forms.Puzzles;

partial class PuzzleForm : Component
{
    private int _layoutIndex;
    private ArrowButton _prevButton;
    private ArrowButton _nextButton;

    private StackLayout _mainLayout;

    private ImageButton _saveButton;
    private ImageButton _saveAsButton;

    private ComboBox<TextLanguage> _languageCombo;

    private TreeView<Layton1PuzzleId> _puzzleTree;
    private Panel _contentPanel;

    private Component _puzzleTitle;
    private Component _puzzleText;
    private Component _puzzleHint;
    private Component _puzzleScript;

    public override Size GetSize() => Size.Parent;

    protected override void UpdateInternal(Rectangle contentRect)
    {
        _mainLayout.Update(contentRect);
    }

    private void InitializeComponent(Layton1NdsInfo ndsInfo, ILocalizationProvider localizations, IFormFactory formFactory, IImageProvider images)
    {
        _puzzleTitle = formFactory.CreatePuzzleTitleForm(ndsInfo);
        _puzzleText = formFactory.CreatePuzzleTextForm(ndsInfo);
        _puzzleHint = formFactory.CreatePuzzleHintForm(ndsInfo);
        _puzzleScript = formFactory.CreatePuzzleScriptForm(ndsInfo);

        _puzzleTree = new TreeView<Layton1PuzzleId>();
        _contentPanel = new Panel();

        _prevButton = new ArrowButton(ImGuiDir.Left) { Enabled = false };
        _nextButton = new ArrowButton(ImGuiDir.Right);

        _languageCombo = new ComboBox<TextLanguage> { MaxShowItems = 5 };

        _saveButton = new ImageButton(images.Save)
        {
            ImageSize = new Vector2(16),
            Tooltip = localizations.MenuFileSaveCaption,
            KeyAction = new KeyCommand(ModifierKeys.Control, Key.S, localizations.MenuFileSaveShortcut)
        };
        _saveAsButton = new ImageButton(images.SaveAs)
        {
            ImageSize = new Vector2(16),
            Tooltip = localizations.MenuFileSaveAsCaption,
            KeyAction = new KeyCommand(Key.F12, localizations.MenuFileSaveAsShortcut)
        };

        _mainLayout = new StackLayout
        {
            Alignment = Alignment.Horizontal,
            Size = Size.Parent,
            ItemSpacing = 5,
            Items =
            {
                new StackLayout
                {
                    Alignment = Alignment.Vertical,
                    Size = new Size(SizeValue.Relative(.3f), SizeValue.Parent),
                    ItemSpacing = 5,
                    Items =
                    {
                        new StackLayout
                        {
                            Alignment = Alignment.Horizontal,
                            Size = Size.WidthAlign,
                            ItemSpacing = 5,
                            Items =
                            {
                                _saveButton,
                                _saveAsButton,
                                new StackItem(_languageCombo) { Size = Size.WidthAlign, HorizontalAlignment = HorizontalAlignment.Right }
                            }
                        },
                        _puzzleTree
                    }
                },
                new StackLayout
                {
                    Alignment = Alignment.Vertical,
                    Size = Size.Parent,
                    ItemSpacing = 5,
                    Items =
                    {
                        new StackLayout
                        {
                            Alignment = Alignment.Horizontal,
                            Size = Size.WidthAlign,
                            ItemSpacing = 5,
                            Items =
                            {
                                _prevButton,
                                _nextButton
                            }
                        },
                        _contentPanel
                    }
                }
            }
        };

        InitializeLanguages(_languageCombo, localizations, ndsInfo.Rom.Version);
    }

    protected override void SetTabInactiveCore()
    {
        _puzzleTree.SetTabInactive();
    }

    private void InitializeLanguages(ComboBox<TextLanguage> languageCombo, ILocalizationProvider localizations, GameVersion version)
    {
        switch (version)
        {
            case GameVersion.Korea:
                languageCombo.Items.Add(new DropDownItem<TextLanguage>(TextLanguage.Korean, localizations.PuzzleLanguageKoreanText));
                break;

            case GameVersion.Usa:
            case GameVersion.UsaDemo:
                languageCombo.Items.Add(new DropDownItem<TextLanguage>(TextLanguage.English, localizations.PuzzleLanguageEnglishText));
                break;

            case GameVersion.Japan:
            case GameVersion.JapanFriendly:
                languageCombo.Items.Add(new DropDownItem<TextLanguage>(TextLanguage.Japanese, localizations.PuzzleLanguageJapaneseText));
                break;

            case GameVersion.Europe:
            case GameVersion.EuropeDemo:
                languageCombo.Items.Add(new DropDownItem<TextLanguage>(TextLanguage.English, localizations.PuzzleLanguageEnglishText));
                languageCombo.Items.Add(new DropDownItem<TextLanguage>(TextLanguage.German, localizations.PuzzleLanguageGermanText));
                languageCombo.Items.Add(new DropDownItem<TextLanguage>(TextLanguage.Spanish, localizations.PuzzleLanguageSpanishText));
                languageCombo.Items.Add(new DropDownItem<TextLanguage>(TextLanguage.French, localizations.PuzzleLanguageFrenchText));
                languageCombo.Items.Add(new DropDownItem<TextLanguage>(TextLanguage.Italian, localizations.PuzzleLanguageItalianText));
                break;
        }

        languageCombo.SelectedItem = languageCombo.Items[0];
    }

    private void UpdateButtons()
    {
        _prevButton.Enabled = _layoutIndex > 0;
        _nextButton.Enabled = _layoutIndex < 3;
    }

    private void UpdateLayout()
    {
        switch (_layoutIndex)
        {
            case 0:
                _contentPanel.Content = _puzzleTitle;
                break;

            case 1:
                _contentPanel.Content = _puzzleText;
                break;

            case 2:
                _contentPanel.Content = _puzzleHint;
                break;

            case 3:
                _contentPanel.Content = _puzzleScript;
                break;
        }
    }
}