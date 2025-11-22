using System.Numerics;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Controls.Text;
using ImGui.Forms.Controls.Text.Editor;
using ImGui.Forms.Modals;
using ImGui.Forms.Models;
using ImGui.Forms.Models.IO;
using Konnect.Contract.DataClasses.Plugin.File.Font;
using UI.Layton1Tool.Components.Contract;
using UI.Layton1Tool.Dialogs.Contract;
using UI.Layton1Tool.Dialogs.Contract.DataClasses;
using UI.Layton1Tool.Resources.Contract;
using Veldrid;
using Rectangle = Veldrid.Rectangle;
using Size = ImGui.Forms.Models.Size;

namespace UI.Layton1Tool.Forms.Files;

partial class FontForm
{
    private static readonly KeyCommand SelectMultipleGlyphsCommand = new(ModifierKeys.Control, MouseButton.Left);
    private static readonly KeyCommand SelectGlyphRangeCommand = new(ModifierKeys.Shift, MouseButton.Left);

    private readonly Dictionary<CharacterInfo, Component> _infoLookup = new();
    private readonly Dictionary<char, Component> _charLookup = new();
    private readonly Dictionary<Component, CharacterInfo> _componentLookup = new();

    private readonly HashSet<CharacterInfo> _selectedCharacters = [];

    private StackLayout _mainLayout;
    private Modal _previewSettingsDialog;

    private Button _generateBtn;
    private ImageButton _editBtn;
    private ImageButton _removeBtn;
    private ImageButton _remapBtn;

    private TextEditor _previewTextEditor;
    private ZoomablePictureBox _textPreview;

    private Component _glyphBox;

    private ImageButton _exportBtn;
    private ImageButton _settingsBtn;

    private StackLayout _glyphLayout;
    private TextBox _searchCharBox;
    private UniformZLayout _glyphsLayout;

    private Component? _lastSelectedElement;
    private Component? _selectedElement;

    private void InitializeComponent(FontPreviewSettings previewSettings, IComponentFactory components, IDialogFactory dialogs, ILocalizationProvider localizations, IImageProvider images)
    {
        _previewSettingsDialog = dialogs.CreatePreviewSettingsDialog(previewSettings);

        _glyphBox = components.CreateZoomableCharacterInfo();
        _glyphBox.ShowBorder = true;

        _editBtn = new ImageButton
        {
            Image = images.FontEdit,
            Tooltip = localizations.FontGenerateEditCaption,
            ImageSize = new Vector2(16)
        };
        _removeBtn = new ImageButton
        {
            Image = images.FontRemove,
            Tooltip = localizations.FontGenerateRemoveCaption,
            ImageSize = new Vector2(16)
        };
        _remapBtn = new ImageButton
        {
            Image = images.FontRemap,
            Tooltip = localizations.FontGenerateRemappingCaption,
            ImageSize = new Vector2(16)
        };

        _searchCharBox = new TextBox
        {
            Width = SizeValue.Absolute(150),
            Placeholder = localizations.FontSearchPlaceholder
        };

        _glyphsLayout = new UniformZLayout(new Vector2(36, 61))
        {
            ItemSpacing = new Vector2(4, 4),
            Size = Size.Parent
        };
        _glyphLayout = new StackLayout
        {
            ItemSpacing = 4,
            Size = Size.Parent,
            Alignment = Alignment.Vertical,
            Items =
            {
                new StackLayout
                {
                    ItemSpacing = 4,
                    Size = Size.WidthAlign,
                    Alignment = Alignment.Horizontal,
                    Items =
                    {
                        _editBtn,
                        _removeBtn,
                        _remapBtn,
                        new StackItem(_searchCharBox) { Size = Size.WidthAlign, HorizontalAlignment = HorizontalAlignment.Right },
                    }
                },
                _glyphsLayout
            }
        };

        _exportBtn = new ImageButton
        {
            Image = images.ImageExport,
            Tooltip = localizations.FontPreviewExport,
            ImageSize = new Vector2(16)
        };
        _settingsBtn = new ImageButton
        {
            Image = images.Settings,
            Tooltip = localizations.FontPreviewSettings,
            ImageSize = new Vector2(16)
        };

        _generateBtn = new Button { Text = localizations.FontGenerateCaption, Width = SizeValue.Absolute(100) };

        _previewTextEditor = new TextEditor();
        _textPreview = new ZoomablePictureBox
        {
            ShowBorder = true
        };

        var toolbarLayout = new StackLayout
        {
            Alignment = Alignment.Horizontal,
            ItemSpacing = 4,
            Size = Size.WidthAlign,
            Items =
            {
                new StackItem(_generateBtn) { Size = Size.WidthAlign, HorizontalAlignment = HorizontalAlignment.Right }
            }
        };

        var textPreviewSettingsLayout = new StackLayout
        {
            Alignment = Alignment.Horizontal,
            Size = Size.WidthAlign,
            ItemSpacing = 4,
            Items =
            {
                new StackItem(_exportBtn) { Size = Size.WidthAlign, HorizontalAlignment = HorizontalAlignment.Right },
                _settingsBtn
            }
        };
        var textPreviewLayout = new StackLayout
        {
            Alignment = Alignment.Horizontal,
            Size = new Size(SizeValue.Parent, .25f),
            ItemSpacing = 4,
            Items =
            {
                _previewTextEditor,
                _textPreview
            }
        };

        var fontDataLayout = new StackLayout
        {
            Alignment = Alignment.Vertical,
            ItemSpacing = 4,
            Items =
            {
                toolbarLayout,
                new StackItem(_glyphBox) { Size = new Size(SizeValue.Parent, .75f) },
                textPreviewSettingsLayout,
                textPreviewLayout
            }
        };

        _mainLayout = new StackLayout
        {
            Alignment = Alignment.Horizontal,
            ItemSpacing = 4,
            Items =
            {
                fontDataLayout,
                _glyphLayout
            }
        };
    }

    public override Size GetSize() => Size.Parent;

    protected override void UpdateInternal(Rectangle contentRect)
    {
        _mainLayout.Update(contentRect);
    }

    private void SetGlyphs(IReadOnlyList<CharacterInfo> characters)
    {
        _lastSelectedElement = null;
        _selectedElement = null;
        _selectedCharacters.Clear();

        foreach (Component item in _glyphsLayout.Items)
            item.Destroy();

        _glyphsLayout.Items.Clear();
        _infoLookup.Clear();
        _charLookup.Clear();
        _componentLookup.Clear();

        foreach (CharacterInfo character in characters)
        {
            Component element = _components.CreateGlyphElement(character);

            _glyphsLayout.Items.Add(element);

            _infoLookup[character] = element;
            _charLookup[character.CodePoint] = element;
            _componentLookup[element] = character;
        }
    }

    private void UpdateGlyphs(IReadOnlyList<CharacterInfo> characters)
    {
        foreach (Component item in _glyphsLayout.Items)
            item.Destroy();

        _glyphsLayout.Items.Clear();
        _infoLookup.Clear();
        _charLookup.Clear();
        _componentLookup.Clear();

        foreach (CharacterInfo character in characters)
        {
            Component element = _components.CreateGlyphElement(character);
            RaiseUpdateSelectedGlyphElement(element, _selectedCharacters.Contains(character));

            _glyphsLayout.Items.Add(element);

            _infoLookup[character] = element;
            _charLookup[character.CodePoint] = element;
            _componentLookup[element] = character;
        }
    }

    private void SetSelectedGlyph(CharacterInfo charInfo)
    {
        if (!_infoLookup.TryGetValue(charInfo, out Component? element))
            return;

        SetSelectedGlyph(element, charInfo);
    }

    private void SetSelectedGlyph(Component element)
    {
        if (!_componentLookup.TryGetValue(element, out CharacterInfo? charInfo))
            return;

        SetSelectedGlyph(element, charInfo);
    }

    private void SetSelectedGlyph(Component element, CharacterInfo charInfo)
    {
        if (SelectMultipleGlyphsCommand.IsPressed())
        {
            _selectedCharacters.Add(charInfo);

            _lastSelectedElement = element;
        }
        else if (SelectGlyphRangeCommand.IsPressed())
        {
            if (_lastSelectedElement == null)
                _lastSelectedElement = element;
            else
            {
                var lastIndex = _glyphsLayout.Items.IndexOf(_lastSelectedElement);
                var currentIndex = _glyphsLayout.Items.IndexOf(element);

                foreach (CharacterInfo selectedCharacter in _selectedCharacters)
                {
                    if (!_infoLookup.TryGetValue(selectedCharacter, out Component? selectedGlyph))
                        continue;

                    RaiseUpdateSelectedGlyphElement(selectedGlyph, false);
                }

                _selectedCharacters.Clear();

                for (var i = Math.Min(lastIndex, currentIndex); i <= Math.Max(lastIndex, currentIndex); i++)
                {
                    var selectedGlyph = _glyphsLayout.Items[i];
                    if (!_componentLookup.TryGetValue(selectedGlyph, out CharacterInfo? selectedCharInfo))
                        continue;

                    _selectedCharacters.Add(selectedCharInfo);

                    RaiseUpdateSelectedGlyphElement(selectedGlyph, true);
                }
            }
        }
        else
        {
            foreach (CharacterInfo selectedCharacter in _selectedCharacters)
            {
                if (!_infoLookup.TryGetValue(selectedCharacter, out Component? selectedGlyph))
                    continue;

                RaiseUpdateSelectedGlyphElement(selectedGlyph, false);
            }

            _selectedCharacters.Clear();
            _selectedCharacters.Add(charInfo);

            _lastSelectedElement = element;
        }

        _selectedElement = element;

        RaiseUpdateSelectedGlyphElement(element, true);
        RaiseSelectedCharacterInfoChanged(charInfo);
    }

    protected override void SetTabInactiveCore()
    {
        _glyphsLayout.SetTabInactive();
    }
}