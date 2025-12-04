using System.Buffers.Binary;
using System.Text;
using System.Text.RegularExpressions;
using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Controls.Base;
using ImGui.Forms.Modals;
using ImGui.Forms.Modals.IO.Windows;
using ImGui.Forms.Resources;
using Kaligraphy.Contract.DataClasses.Layout;
using Kaligraphy.Contract.DataClasses.Parsing;
using Kaligraphy.DataClasses.Layout;
using Kaligraphy.DataClasses.Rendering;
using Kaligraphy.Layout;
using Konnect.Contract.DataClasses.Plugin.File.Font;
using Konnect.Plugin.File.Font;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Files;
using Logic.Domain.NintendoManagement.Contract.DataClasses.Font;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UI.Layton1Tool.Components.Contract;
using UI.Layton1Tool.Dialogs.Contract;
using UI.Layton1Tool.Dialogs.Contract.DataClasses;
using UI.Layton1Tool.Dialogs.Contract.Enums;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Forms.InternalContract;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Forms.Files;

partial class FontForm : Component
{
    private readonly Layton1NdsInfo _ndsInfo;
    private readonly IUnicodeCharacterParser _unicodeParser;
    private readonly IEventBroker _eventBroker;
    private readonly ILayton1NdsFileManager _fileManager;
    private readonly IComponentFactory _components;
    private readonly IDialogFactory _dialogs;
    private readonly ILocalizationProvider _localizations;
    private readonly ISettingsProvider _settings;

    private readonly FontPreviewSettings _previewSettings = new();

    private Image<Rgba32>? _generatedPreview;

    private NftrData? _fontData;
    private Layton1NdsFile? _selectedFile;

    public FontForm(Layton1NdsInfo ndsInfo, IUnicodeCharacterParser unicodeParser, IEventBroker eventBroker, ILayton1NdsFileManager fileManager,
        IComponentFactory components, IDialogFactory dialogs, ILocalizationProvider localizations, ISettingsProvider settings, IImageProvider images)
    {
        InitializeComponent(_previewSettings, components, dialogs, localizations, images);

        _ndsInfo = ndsInfo;
        _unicodeParser = unicodeParser;
        _eventBroker = eventBroker;
        _fileManager = fileManager;
        _components = components;
        _dialogs = dialogs;
        _localizations = localizations;
        _settings = settings;

        _searchCharBox!.TextChanged += _searchCharBox_TextChanged;
        _generateBtn!.Clicked += _generateBtn_Clicked;
        _editBtn!.Clicked += _editBtn_Clicked;
        _removeBtn!.Clicked += _removeBtn_Clicked;
        _remapBtn!.Clicked += _remapBtn_Clicked;

        _previewTextEditor!.TextChanged += _previewTextEditor_TextChanged;

        _exportBtn!.Clicked += _exportBtn_Clicked;
        _settingsBtn!.Clicked += _settingsBtn_Clicked;

        eventBroker.Subscribe<FileContentModifiedMessage>(ProcessFileContentModified);
        eventBroker.Subscribe<SelectedFileChangedMessage>(ProcessSelectedFontChanged);
        eventBroker.Subscribe<SelectedGlyphElementChangedMessage>(ProcessSelectedGlyphElementChanged);

        RaiseUpdateCharacterInfoZoom(20f);
        _previewTextEditor.SetText(localizations.FontPreviewPlaceholder);

        ResetState();
    }

    public override void Destroy()
    {
        foreach (Component item in _glyphsLayout.Items)
            item.Destroy();

        _glyphBox.Destroy();
        _previewSettingsDialog.Destroy();

        _eventBroker.Unsubscribe<FileContentModifiedMessage>(ProcessFileContentModified);
        _eventBroker.Unsubscribe<SelectedFileChangedMessage>(ProcessSelectedFontChanged);
        _eventBroker.Unsubscribe<SelectedGlyphElementChangedMessage>(ProcessSelectedGlyphElementChanged);
    }

    private void ProcessFileContentModified(FileContentModifiedMessage message)
    {
        if (message.Source == this)
            return;

        if (message.File != _selectedFile)
            return;

        UpdateFont(message.File, message.Content);
    }

    private void ProcessSelectedFontChanged(SelectedFileChangedMessage message)
    {
        if (message.Target != this)
            return;

        UpdateFont(message.File, message.Content);
    }

    private void UpdateFont(Layton1NdsFile file, object? content)
    {
        if (content is not NftrData font)
            return;

        if (file.Rom != _ndsInfo.Rom)
            return;

        _fontData = font;
        _selectedFile = file;

        ResetState();
    }

    private void ProcessSelectedGlyphElementChanged(SelectedGlyphElementChangedMessage message)
    {
        if (!_glyphsLayout.Items.Contains(message.Source))
            return;

        SetSelectedGlyph(message.Source);
    }

    private void RaiseUpdateCharacterInfoZoom(float zoom)
    {
        _eventBroker.Raise(new UpdateCharacterInfoZoomMessage(_glyphBox, zoom));
    }

    private void RaiseUpdateSelectedGlyphElement(Component element, bool isSelected)
    {
        _eventBroker.Raise(new UpdateSelectedGlyphElementMessage(element, isSelected));
    }

    private void RaiseSelectedCharacterInfoChanged(CharacterInfo charInfo)
    {
        _eventBroker.Raise(new SelectedCharacterInfoChangedMessage(_glyphBox, charInfo));
    }

    private void RaiseFileContentModified(Layton1NdsFile file, NftrData font)
    {
        _eventBroker.Raise(new FileContentModifiedMessage(this, file, font));
    }

    private async void _settingsBtn_Clicked(object? sender, EventArgs e)
    {
        await _previewSettingsDialog.ShowAsync();

        UpdateTextPreview();
    }

    private async void _exportBtn_Clicked(object? sender, EventArgs e)
    {
        if (_generatedPreview is null)
            return;

        // Select file to save at
        var sfd = new WindowsSaveFileDialog
        {
            Title = _localizations.ImageMenuExportPng,
            InitialDirectory = GetPreviewDirectory(),
            InitialFileName = "preview.png"
        };

        if (await sfd.ShowAsync() is not DialogResult.Ok)
            return;

        await _generatedPreview.SaveAsPngAsync(sfd.Files[0]);

        string? selectedDir = Path.GetDirectoryName(sfd.Files[0]);
        if (string.IsNullOrEmpty(selectedDir))
            return;

        _settings.PreviewDirectory = selectedDir;
    }

    #region Events

    private void _searchCharBox_TextChanged(object? sender, EventArgs e)
    {
        char? searchChar = GetCharacter(_searchCharBox.Text);
        if (!searchChar.HasValue)
            return;

        if (!_charLookup.TryGetValue(searchChar.Value, out Component? glyph))
            return;

        _glyphsLayout.ScrollToItem(glyph);
        SetSelectedGlyph(glyph);
    }

    private async void _generateBtn_Clicked(object? sender, EventArgs e)
    {
        if (_fontData is null || _selectedFile is null)
            return;

        var generationDialog = _dialogs.CreateFontGenerationDialog(_fontData, FontGenerationType.Create);

        DialogResult result = await generationDialog.ShowAsync();
        generationDialog.Destroy();

        if (result is not DialogResult.Ok)
            return;

        _fileManager.Compose(_selectedFile, _fontData);

        RaiseFileContentModified(_selectedFile, _fontData);

        ResetState();
    }

    private async void _editBtn_Clicked(object? sender, EventArgs e)
    {
        if (_fontData is null || _selectedFile is null)
            return;

        string selectedCharacters = string.Concat(_selectedCharacters.Select(c => c.CodePoint));

        var generationDialog = _dialogs.CreateFontGenerationDialog(_fontData, FontGenerationType.Edit, selectedCharacters);

        DialogResult result = await generationDialog.ShowAsync();
        generationDialog.Destroy();

        if (result is not DialogResult.Ok)
            return;

        _fileManager.Compose(_selectedFile, _fontData);

        RaiseFileContentModified(_selectedFile, _fontData);

        UpdateState();
    }

    private async void _removeBtn_Clicked(object? sender, EventArgs e)
    {
        if (_selectedCharacters.Count <= 0 || _fontData is null || _selectedFile is null)
            return;

        DialogResult result = await MessageBox.ShowYesNoAsync(_localizations.DialogFontRemoveCaption, _localizations.DialogFontRemoveText);
        if (result is not DialogResult.Yes)
            return;

        if (_selectedCharacters.Count >= _fontData.Characters.Count)
        {
            _fontData.Characters.Clear();

            _charLookup.Clear();
            _infoLookup.Clear();

            _selectedCharacters.Clear();

            _glyphsLayout.Items.Clear();
        }
        else
        {
            foreach (CharacterInfo character in _selectedCharacters)
            {
                if (!_fontData.Characters.Remove(character))
                    continue;

                if (_infoLookup.TryGetValue(character, out Component? element))
                    _glyphsLayout.Items.Remove(element);

                _charLookup.Remove(character.CodePoint);
                _infoLookup.Remove(character);

                _selectedCharacters.Remove(character);
            }
        }

        _lastSelectedElement = null;

        _fileManager.Compose(_selectedFile, _fontData);

        RaiseFileContentModified(_selectedFile, _fontData);

        UpdateState();
    }

    private async void _remapBtn_Clicked(object? sender, EventArgs e)
    {
        if (_fontData is null || _selectedFile is null)
            return;

        var selectedCharacters = _selectedCharacters.OrderBy(c => c.CodePoint).ToArray();

        var dialog = _dialogs.CreateFontRemappingDialog(_fontData, selectedCharacters);

        var result = await dialog.ShowAsync();
        dialog.Destroy();

        if (result is not DialogResult.Ok)
            return;

        _fileManager.Compose(_selectedFile, _fontData);

        RaiseFileContentModified(_selectedFile, _fontData);

        UpdateState();
    }

    private void _previewTextEditor_TextChanged(object? sender, string e)
    {
        UpdateTextPreview();
    }

    private void UpdateTextPreview()
    {
        _generatedPreview = GeneratePreview();

        _textPreview.SetImage((_generatedPreview is null ? null : ImageResource.FromImage(_generatedPreview))!);
    }

    private Image<Rgba32>? GeneratePreview()
    {
        if (_fontData is null)
            return null;

        string text = _previewTextEditor.GetText();

        IList<CharacterData> parsedText = _unicodeParser.Parse(Encoding.UTF8.GetBytes(text), Encoding.UTF8);

        var glyphProvider = new FontPluginGlyphProvider(_fontData.Characters);

        var layoutOptions = new LayoutOptions
        {
            TextSpacing = _previewSettings.Spacing,
            HorizontalAlignment = _previewSettings.HorizontalAlignment,
            LineHeight = _previewSettings.LineHeight
        };
        var layouter = new TextLayouter(layoutOptions, glyphProvider);
        IList<TextLayoutLineData> layoutLines = layouter.Create(parsedText);

        float imageWidth = layoutLines.Count <= 0 ? 0 : layoutLines.Max(l => l.BoundingBox.Width);
        float imageHeight = layoutLines.Count <= 0 ? 0 : layoutLines.Sum(l => l.BoundingBox.Height);
        if (imageWidth <= 0 || imageHeight <= 0)
            return null;

        var image = new Image<Rgba32>((int)imageWidth + 1, (int)imageHeight + 1);
        TextLayoutData layout = layouter.Create(layoutLines, Point.Empty, image.Size);

        var renderOptions = new RenderOptions
        {
            DrawBoundingBoxes = _previewSettings.ShowDebugBoxes
        };
        var renderer = new Kaligraphy.Rendering.TextRenderer(renderOptions, glyphProvider);
        renderer.Render(image, layout);

        return image;
    }

    #endregion

    #region Update methods

    private void ResetState()
    {
        if (_fontData is null)
            return;

        SetGlyphs(_fontData.Characters);

        if (_fontData.Characters.Count > 0)
            SetSelectedGlyph(_fontData.Characters[0]);

        UpdateTextPreview();
    }

    private void UpdateState()
    {
        if (_fontData is null)
            return;

        if (_selectedElement is not null && _componentLookup.TryGetValue(_selectedElement, out CharacterInfo? selectedCharInfo))
            RaiseSelectedCharacterInfoChanged(selectedCharInfo);

        UpdateGlyphs(_fontData.Characters);

        UpdateTextPreview();
    }

    #endregion

    #region Support

    private char? GetCharacter(string searchText)
    {
        var regex = new Regex(@"^\\u([a-fA-F0-9]{4})$");
        Match match = regex.Match(searchText);

        if (match.Groups.Count > 1)
            return (char)BinaryPrimitives.ReadInt16BigEndian(Convert.FromHexString(match.Groups[1].Value));

        if (searchText.Length is 1)
            return searchText[0];

        return null;
    }

    private string GetPreviewDirectory()
    {
        var settingsDir = _settings.PreviewDirectory;
        return string.IsNullOrEmpty(settingsDir) ? Path.GetFullPath(".") : settingsDir;
    }

    #endregion
}