using System.Drawing;
using System.Numerics;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Text;
using ImGui.Forms.Controls.Text.Editor;
using ImGui.Forms.Modals;
using ImGui.Forms.Models;
using UI.Layton1Tool.Components.Contract;
using UI.Layton1Tool.Dialogs.Contract.Enums;
using UI.Layton1Tool.Resources.Contract;
using Size = ImGui.Forms.Models.Size;

namespace UI.Layton1Tool.Dialogs;

partial class FontGenerationDialog : Modal
{
    private Component _glyphBox;
    private TextBox _paddingLeftBox;
    private TextBox _paddingRightBox;
    private ComboBox<FontFamily> _fontFamilyBox;
    private CheckBox _boldCheckBox;
    private CheckBox _italicCheckBox;
    private TextBox _fontSizeBox;
    private TextBox _baselineBox;
    private TextBox _glyphHeightBox;
    private TextBox _spaceWidthBox;
    private TextEditor _characterEditor;
    private CheckBox _replaceCharactersCheck;

    private Button _loadBtn;
    private Button _saveBtn;
    private Button _executeBtn;

    private void InitializeComponent(FontGenerationType type, string? selectedCharacters, IComponentFactory components,
        ILocalizationProvider localizations, ISettingsProvider settings)
    {
        _glyphBox = components.CreateZoomablePaddedGlyph();
        _glyphBox.ShowBorder = true;

        _paddingLeftBox = new TextBox { AllowedCharacters = CharacterRestriction.Decimal };
        _paddingRightBox = new TextBox { AllowedCharacters = CharacterRestriction.Decimal };
        _fontFamilyBox = new ComboBox<FontFamily> { MaxShowItems = 5 };
        _boldCheckBox = new CheckBox(localizations.DialogFontGenerateStyleBold);
        _italicCheckBox = new CheckBox(localizations.DialogFontGenerateStyleItalic);
        _fontSizeBox = new TextBox { Text = $"{DefaultFontSize_}", AllowedCharacters = CharacterRestriction.Decimal };
        _baselineBox = new TextBox { Text = $"{DefaultBaseline_}", AllowedCharacters = CharacterRestriction.Decimal };
        _glyphHeightBox = new TextBox { Text = $"{DefaultGlyphHeight_}", AllowedCharacters = CharacterRestriction.Decimal };
        _spaceWidthBox = new TextBox { Text = $"{DefaultSpaceWidth_}", AllowedCharacters = CharacterRestriction.Decimal };
        _replaceCharactersCheck = new CheckBox(localizations.DialogFontGenerateCharactersReplace) { Checked = settings.ReplaceFontCharacters };

        _characterEditor = new TextEditor { IsShowingLineNumbers = false };
        _characterEditor.SetText(string.IsNullOrEmpty(selectedCharacters) ? localizations.FontGenerateDefaultCharacters : selectedCharacters);

        _loadBtn = new Button(localizations.DialogFontGenerateLoad) { Width = 75 };
        _saveBtn = new Button(localizations.DialogFontGenerateSave) { Width = 75 };
        _executeBtn = new Button(type == FontGenerationType.Create
            ? localizations.DialogFontGenerateGenerate
            : localizations.DialogFontEditEdit)
        {
            Width = 75,
            Enabled = type != FontGenerationType.Create
        };

        Size = new Size(SizeValue.Relative(.7f), SizeValue.Relative(.8f));
        Caption = type == FontGenerationType.Create ? localizations.DialogFontGenerateCaption : localizations.DialogFontEditCaption;
        Content = new StackLayout
        {
            Alignment = Alignment.Horizontal,
            ItemSpacing = 4,
            Items =
            {
                new StackLayout
                {
                    Alignment = Alignment.Vertical,
                    ItemSpacing = 4,
                    Items =
                    {
                        _glyphBox,
                        new TableLayout
                        {
                            Size = Size.WidthAlign,
                            Spacing = new Vector2(4),
                            Rows =
                            {
                                new TableRow
                                {
                                    Cells =
                                    {
                                        new Label(localizations.DialogFontGeneratePaddingLeft),
                                        new Label(localizations.DialogFontGeneratePaddingRight)
                                    }
                                },
                                new TableRow
                                {
                                    Cells =
                                    {
                                        _paddingLeftBox,
                                        _paddingRightBox
                                    }
                                }
                            }
                        }
                    }
                },
                new StackLayout
                {
                    Alignment = Alignment.Vertical,
                    ItemSpacing = 4,
                    Items =
                    {
                        new TableLayout
                        {
                            Spacing = new Vector2(4),
                            Rows =
                            {
                                new TableRow
                                {
                                    Cells =
                                    {
                                        new Label(localizations.DialogFontGenerateFamily),
                                        _fontFamilyBox
                                    }
                                },
                                new TableRow
                                {
                                    Cells =
                                    {
                                        new Label(localizations.DialogFontGenerateStyle),
                                        new StackLayout
                                        {
                                            Alignment = Alignment.Horizontal,
                                            Size = Size.WidthAlign,
                                            ItemSpacing = 4,
                                            Items =
                                            {
                                                new StackItem(_boldCheckBox){Size = Size.WidthAlign},
                                                new StackItem(_italicCheckBox){Size = Size.WidthAlign}
                                            }
                                        }
                                    }
                                },
                                new TableRow
                                {
                                    Cells =
                                    {
                                        new Label(localizations.DialogFontGenerateSize),
                                        _fontSizeBox
                                    }
                                },
                                new TableRow
                                {
                                    Cells =
                                    {
                                        new Label(localizations.DialogFontGenerateBaseline),
                                        _baselineBox
                                    }
                                },
                                new TableRow
                                {
                                    Cells =
                                    {
                                        new Label(localizations.DialogFontGenerateGlyphHeight),
                                        _glyphHeightBox
                                    }
                                },
                                new TableRow
                                {
                                    Cells =
                                    {
                                        new Label(localizations.DialogFontGenerateSpaceWidth),
                                        _spaceWidthBox
                                    }
                                },
                                new TableRow
                                {
                                    Cells =
                                    {
                                        new Label(localizations.DialogFontGenerateCharacters),
                                        _characterEditor
                                    }
                                },
                                new TableRow
                                {
                                    Cells =
                                    {
                                        new Label(string.Empty),
                                        new StackLayout
                                        {
                                            Alignment = Alignment.Horizontal,
                                            Size = Size.WidthAlign,
                                            ItemSpacing = 4,
                                            Items =
                                            {
                                                new StackItem(_replaceCharactersCheck){Size = Size.WidthAlign}
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new StackLayout
                        {
                            Alignment = Alignment.Horizontal,
                            Size = Size.WidthAlign,
                            ItemSpacing = 4,
                            Items =
                            {
                                _loadBtn,
                                _saveBtn,
                                new StackItem(_executeBtn) { Size = Size.WidthAlign, HorizontalAlignment = HorizontalAlignment.Right }
                            }
                        }
                    }
                }
            }
        };
    }
}