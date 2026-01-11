using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Tree;
using ImGui.Forms.Models;
using ImGui.Forms.Models.IO;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.Contract.Enums.Texts;
using System.Numerics;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Resources.Contract;
using Veldrid;

namespace UI.Layton1Tool.Forms.Events;

internal partial class EventForm : Component
{
    private StackLayout _mainLayout;

    private ImageButton _saveButton;
    private ImageButton _saveAsButton;
    private ImageButton _addButton;

    private ComboBox<TextLanguage> _languageCombo;

    private TreeView<int> _eventTree;

    private Component _eventScriptForm;
    private Component _eventFlagsForm;
    private Component _eventTextForm;

    public override Size GetSize() => Size.Parent;

    protected override void UpdateInternal(Rectangle contentRect)
    {
        _mainLayout.Update(contentRect);
    }

    private void InitializeComponent(Layton1NdsInfo ndsInfo, IFormFactory forms, IImageProvider images, ILocalizationProvider localizations)
    {
        _eventScriptForm = forms.CreateEventScriptForm(ndsInfo);
        _eventFlagsForm = forms.CreateEventFlagsForm(ndsInfo);
        _eventTextForm = forms.CreateEventTextControl(ndsInfo);

        _eventTree = new TreeView<int>();

        _languageCombo = new ComboBox<TextLanguage> { MaxShowItems = 5 };

        _addButton = new ImageButton(images.Add)
        {
            ImageSize = new Vector2(16),
            Tooltip = localizations.EventAddCaption,
            KeyAction = new KeyCommand(ModifierKeys.Control | ModifierKeys.Shift, Key.A, localizations.EventAddShortcut)
        };
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
                                _addButton,
                                new StackItem(_languageCombo) { Size = Size.WidthAlign, HorizontalAlignment = HorizontalAlignment.Right }
                            }
                        },
                        _eventTree
                    }
                },
                new StackLayout
                {
                    Alignment = Alignment.Vertical,
                    Size = Size.Parent,
                    ItemSpacing = 5,
                    Items =
                    {
                        new StackItem(_eventFlagsForm){Size = new Size(SizeValue.Parent, .15f)},
                        new StackLayout
                        {
                            Alignment = Alignment.Horizontal,
                            Size = new Size(SizeValue.Parent, .85f),
                            ItemSpacing = 5,
                            Items =
                            {
                                _eventScriptForm,
                                _eventTextForm
                            }
                        }
                    }
                }
            }
        };

        InitializeLanguages(_languageCombo, localizations, ndsInfo.Rom.Version);
    }

    protected override void SetTabInactiveCore()
    {
        _eventTree.SetTabInactive();
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
}