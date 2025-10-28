using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Modals;
using Konnect.Contract.DataClasses.Plugin.File.Font;
using Logic.Domain.NintendoManagement.Contract.DataClasses.Font;
using UI.Layton1Tool.Components.Contract;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Dialogs;

partial class FontRemappingDialog
{
    private readonly IReadOnlyList<CharacterInfo> _remapCharacters;

    private readonly IEventBroker _eventBroker;
    private readonly IComponentFactory _components;

    private readonly List<Component> _selectedGlyphs = [];
    private readonly List<CharacterInfo> _selectedInfos = [];
    private readonly Dictionary<Component, CharacterInfo> _componentLookup = [];

    public FontRemappingDialog(NftrData fontData, IReadOnlyList<CharacterInfo> remapCharacters, IEventBroker eventBroker,
        IComponentFactory components, ILocalizationProvider localizations)
    {
        InitializeComponent(localizations);

        _remapCharacters = remapCharacters;

        _eventBroker = eventBroker;
        _components = components;

        _remapButton!.Clicked += _remapButton_Clicked;

        _eventBroker.Subscribe<SelectedGlyphElementChangedMessage>(ProcessSelectedGlyphElementChanged);

        SetGlyphs(fontData.Characters);
    }

    public override void Destroy()
    {
        foreach (Component item in _glyphsLayout.Items)
            item.Destroy();

        _eventBroker.Unsubscribe<SelectedGlyphElementChangedMessage>(ProcessSelectedGlyphElementChanged);
    }

    private void _remapButton_Clicked(object? sender, EventArgs e)
    {
        if (_selectedInfos.Count <= 0)
            return;

        for (var i = 0; i < _selectedInfos.Count; i++)
        {
            CharacterInfo selectedGlyph = _selectedInfos[i];
            CharacterInfo remapCharacter = _remapCharacters[i];

            remapCharacter.Glyph = selectedGlyph.Glyph;
            remapCharacter.GlyphPosition = selectedGlyph.GlyphPosition;
            remapCharacter.BoundingBox = selectedGlyph.BoundingBox;
            remapCharacter.ContentChanged = true;
        }

        Close(DialogResult.Ok);
    }

    private void SetGlyphs(IReadOnlyList<CharacterInfo> characters)
    {
        _componentLookup.Clear();
        _glyphsLayout.Items.Clear();

        foreach (CharacterInfo character in characters)
        {
            if (_remapCharacters.Contains(character))
                continue;

            var element = _components.CreateGlyphElement(character);

            _glyphsLayout.Items.Add(element);
            _componentLookup[element] = character;
        }
    }

    private void SetGlyphRemapRange(Component element)
    {
        foreach (Component selectedGlyph in _selectedGlyphs)
            RaiseUpdateSelectedGlyphElement(selectedGlyph, false);

        _selectedInfos.Clear();
        _selectedGlyphs.Clear();

        int elementIndex = _glyphsLayout.Items.IndexOf(element);

        if (elementIndex < 0 || elementIndex >= _glyphsLayout.Items.Count)
        {
            _remapButton.Enabled = false;
            return;
        }

        for (int i = elementIndex; i < Math.Min(_glyphsLayout.Items.Count, elementIndex + _remapCharacters.Count); i++)
        {
            if (!_componentLookup.TryGetValue(_glyphsLayout.Items[i], out CharacterInfo? charInfo))
                continue;

            _selectedInfos.Add(charInfo);
            _selectedGlyphs.Add(_glyphsLayout.Items[i]);

            RaiseUpdateSelectedGlyphElement(_glyphsLayout.Items[i], true);
        }

        _remapButton.Enabled = true;
    }

    private void ProcessSelectedGlyphElementChanged(SelectedGlyphElementChangedMessage message)
    {
        if (!_glyphsLayout.Items.Contains(message.Source))
            return;

        SetGlyphRemapRange(message.Source);
    }

    private void RaiseUpdateSelectedGlyphElement(Component element, bool isSelected)
    {
        _eventBroker.Raise(new UpdateSelectedGlyphElementMessage(element, isSelected));
    }
}