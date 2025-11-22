using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Resources;
using Kaligraphy.Contract.Layout;
using Kaligraphy.Contract.Parsing;
using Kaligraphy.Contract.Rendering;
using Kaligraphy.DataClasses.Layout;
using Kaligraphy.DataClasses.Rendering;
using Kaligraphy.Enums.Layout;
using Kaligraphy.Layout;
using Kaligraphy.Parsing;
using Kaligraphy.Rendering;
using Logic.Business.Layton1ToolManagement.Contract;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.Contract.Enums.Texts;
using Logic.Business.Layton1ToolManagement.Contract.Files;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Forms.InternalContract;
using UI.Layton1Tool.Forms.Text;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Messages.Enums;

namespace UI.Layton1Tool.Forms.Views;

internal partial class PuzzleTitleView
{
    private readonly Layton1NdsInfo _ndsInfo;

    private readonly IEventBroker _eventBroker;
    private readonly IFontProvider _fontProvider;
    private readonly ILayton1NdsFileManager _fileManager;
    private readonly ILayton1PathProvider _pathProvider;

    private Layton1PuzzleId? _puzzleId;
    private TextLanguage? _language;

    private Image<Rgba32>? _topBg;
    private Image<Rgba32>? _bottomBg;
    private IGlyphProvider? _font;
    private IGlyphProvider? _furiganaFont;

    private Image<Rgba32>? _bg;

    private string? _title;

    public PuzzleTitleView(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, IFontProvider fontProvider, ILayton1NdsFileManager fileManager,
        ILayton1PathProvider pathProvider)
    {
        InitializeComponent();

        _ndsInfo = ndsInfo;

        _eventBroker = eventBroker;
        _fontProvider = fontProvider;
        _fileManager = fileManager;
        _pathProvider = pathProvider;

        eventBroker.Subscribe<FileContentModifiedMessage>(ProcessFileContentModified);
        eventBroker.Subscribe<FileAddedMessage>(ProcessFileAdded);
        eventBroker.Subscribe<FontModifiedMessage>(ProcessFontModified);
        eventBroker.Subscribe<SelectedPuzzleChangedMessage>(ProcessSelectedPuzzleChanged);
        eventBroker.Subscribe<SelectedPuzzleLanguageChangedMessage>(ProcessSelectedPuzzleLanguageChanged);
        eventBroker.Subscribe<SelectedPuzzleTitleTextModifiedMessage>(ProcessSelectedPuzzleTitleTextChanged);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<FileContentModifiedMessage>(ProcessFileContentModified);
        _eventBroker.Unsubscribe<FileAddedMessage>(ProcessFileAdded);
        _eventBroker.Unsubscribe<FontModifiedMessage>(ProcessFontModified);
        _eventBroker.Unsubscribe<SelectedPuzzleChangedMessage>(ProcessSelectedPuzzleChanged);
        _eventBroker.Unsubscribe<SelectedPuzzleLanguageChangedMessage>(ProcessSelectedPuzzleLanguageChanged);
        _eventBroker.Unsubscribe<SelectedPuzzleTitleTextModifiedMessage>(ProcessSelectedPuzzleTitleTextChanged);
    }

    private void ProcessFileContentModified(FileContentModifiedMessage message)
    {
        if (message.Source == this)
            return;

        if (_puzzleId is null || _language is null || _title is null)
            return;

        if (message.File.Rom != _ndsInfo.Rom)
            return;

        if (message.File.Path == GetTopBackgroundPath())
            _topBg = GetTopBackground();
        else if (message.File.Path == GetBottomBackgroundPath(_language.Value))
            _bottomBg = GetBottomBackground(_language.Value);
        else
            return;

        if (GenerateBackground(_language.Value))
            GeneratePreview(_title, _puzzleId);
    }

    private void ProcessFileAdded(FileAddedMessage message)
    {
        if (message.Source == this)
            return;

        if (_puzzleId is null || _language is null || _title is null)
            return;

        if (message.File.Rom != _ndsInfo.Rom)
            return;

        if (message.File.Path == GetTopBackgroundPath())
            _topBg = GetTopBackground();
        else if (message.File.Path == GetBottomBackgroundPath(_language.Value))
            _bottomBg = GetBottomBackground(_language.Value);
        else
            return;

        if (GenerateBackground(_language.Value))
            GeneratePreview(_title, _puzzleId);
    }

    private void ProcessFontModified(FontModifiedMessage message)
    {
        if (_puzzleId is null || _language is null || _title is null)
            return;

        if (message.File.Rom != _ndsInfo.Rom)
            return;

        switch (message.Type)
        {
            case FontType.Event:
                _font = message.Font;
                break;

            case FontType.Furigana:
                _furiganaFont = message.Font;
                break;

            default:
                return;
        }

        if (SetupBackground(_language.Value))
            GeneratePreview(_title, _puzzleId);
    }

    private void ProcessSelectedPuzzleChanged(SelectedPuzzleChangedMessage message)
    {
        if (message.Rom != _ndsInfo.Rom)
            return;

        if (_title is not null)
            if (GenerateBackground(message.Language))
                GeneratePreview(_title, message.Puzzle);

        _puzzleId = message.Puzzle;
        _language = message.Language;
    }

    private void ProcessSelectedPuzzleLanguageChanged(SelectedPuzzleLanguageChangedMessage message)
    {
        if (_puzzleId is null)
            return;

        if (message.Rom != _ndsInfo.Rom)
            return;

        if (_title is not null)
            if (GenerateBackground(message.Language))
                GeneratePreview(_title, _puzzleId);

        _language = message.Language;
    }

    private void ProcessSelectedPuzzleTitleTextChanged(SelectedPuzzleTitleTextModifiedMessage message)
    {
        if (_puzzleId is null || _language is null)
            return;

        if (message.Rom != _ndsInfo.Rom)
            return;

        if (message.Puzzle.InternalId != _puzzleId.InternalId)
            return;

        if (SetupBackground(_language.Value))
            GeneratePreview(message.Title, _puzzleId);

        _title = message.Title;
    }

    private void GeneratePreview(string title, Layton1PuzzleId puzzleId)
    {
        if (_bg is null)
            return;

        if (!SetupFontResources())
            return;

        Image<Rgba32> image = _bg.Clone();

        RenderTitleText(image, puzzleId, title);

        _titleImageBox.Image = ImageResource.FromImage(image);
    }

    private void RenderTitleText(Image<Rgba32> image, Layton1PuzzleId puzzleId, string title)
    {
        if (_font is null)
            return;

        ICharacterDeserializer deserializer;
        ITextLayouter layouter;
        ITextRenderer renderer;
        Point pos;

        switch (_ndsInfo.Rom.Version)
        {
            case GameVersion.Europe:
            case GameVersion.EuropeDemo:
                deserializer = new DiacriticCharacterDeserializer();
                layouter = new TextLayouter(new LayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Center }, _font);
                renderer = new TextRenderer(new RenderOptions { TextColor = Color.Black }, _font);
                pos = new Point(2, 121);
                break;

            case GameVersion.Usa:
            case GameVersion.UsaDemo:
                deserializer = new CharacterDeserializer();
                layouter = new TextLayouter(new LayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Center }, _font);
                renderer = new TextRenderer(new RenderOptions { TextColor = Color.Black }, _font);
                pos = new Point(0, 121);
                break;

            case GameVersion.Korea:
                deserializer = new CharacterDeserializer();
                layouter = new TextLayouter(new LayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Center }, _font);
                renderer = new TextRenderer(new RenderOptions { TextColor = Color.Black }, _font);
                pos = new Point(2, 119);
                break;

            case GameVersion.JapanFriendly:
                if (_furiganaFont is null)
                    return;

                string numberText = $"{puzzleId.Number:000}".Aggregate("", (o, c) => o + (char)(c + 0xFEE0));

                deserializer = new FuriganaCharacterDeserializer(false);
                layouter = new FuriganaTextLayouter(new FuriganaLayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Center, FuriganaLineSpacing = -2 }, _font, _furiganaFont);
                renderer = new FuriganaTextRenderer(new FuriganaRenderOptions { TextColor = Color.Black, FuriganaTextColor = Color.FromRgb(0xad, 0x7b, 0x39) }, _font, _furiganaFont);
                pos = new Point(2, 121);
                title = $"ナゾ{numberText}  {title}";
                break;

            case GameVersion.Japan:
                string numberText1 = $"{puzzleId.Number:000}".Aggregate("", (o, c) => o + (char)(c + 0xFEE0));

                deserializer = new CharacterDeserializer();
                layouter = new TextLayouter(new LayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Center }, _font);
                renderer = new TextRenderer(new RenderOptions { TextColor = Color.Black }, _font);
                pos = new Point(2, 121);
                title = $"ナゾ{numberText1}  {title}";
                break;

            default:
                throw new InvalidOperationException($"Unknown game version {_ndsInfo.Rom.Version}.");
        }

        var layout = layouter.Create(deserializer.Deserialize(title), pos, new Size(256, 192));
        renderer.Render(image, layout);
    }

    private bool SetupBackground(TextLanguage language)
    {
        if (_bg is not null)
            return true;

        return GenerateBackground(language);
    }

    private bool GenerateBackground(TextLanguage language)
    {
        if (!SetupImageResources(language))
            return false;

        var image = new Image<Rgba32>(256, 192 * 2);

        image.Mutate(c => c.DrawImage(_topBg!, Point.Empty, 1f));
        image.Mutate(c => c.DrawImage(_bottomBg!, new Point(0, 192), 1f));

        _bg = image;

        return true;
    }

    private bool SetupImageResources(TextLanguage language)
    {
        return SetupStaticImageResources() &&
               SetupLocalizedImageResources(language);
    }

    private bool SetupStaticImageResources()
    {
        _topBg ??= GetTopBackground();

        return _topBg is not null;
    }

    private bool SetupLocalizedImageResources(TextLanguage language)
    {
        if (_language is not null)
        {
            if (_language.Value != language)
                _bottomBg = GetBottomBackground(language);
            else
                _bottomBg ??= GetBottomBackground(language);
        }
        else
        {
            _bottomBg ??= GetBottomBackground(language);
        }

        return _bottomBg is not null;
    }

    private bool SetupFontResources()
    {
        _font = _fontProvider.GetEventFont(_ndsInfo.Rom);
        _furiganaFont = _fontProvider.GetFuriganaFont(_ndsInfo.Rom);

        bool isLoaded = _font is not null;

        if (_ndsInfo.Rom.Version is GameVersion.JapanFriendly)
            isLoaded &= _furiganaFont is not null;

        return isLoaded;
    }

    private Image<Rgba32>? GetTopBackground()
    {
        string filePath = GetTopBackgroundPath();

        if (!_fileManager.TryGet(_ndsInfo.Rom, filePath, out Layton1NdsFile? file))
            return null;

        return (Image<Rgba32>?)_fileManager.Parse(file, FileType.Bgx);
    }

    private Image<Rgba32>? GetBottomBackground(TextLanguage language)
    {
        string filePath = GetBottomBackgroundPath(language);

        if (!_fileManager.TryGet(_ndsInfo.Rom, filePath, out Layton1NdsFile? file))
            return null;

        return (Image<Rgba32>?)_fileManager.Parse(file, FileType.Bgx);
    }

    private string GetTopBackgroundPath()
    {
        return _pathProvider.GetFullDirectory("bg/q_alt_sub_bg.arc", _ndsInfo.Rom.Version);
    }

    private string GetBottomBackgroundPath(TextLanguage language)
    {
        return _ndsInfo.Rom.Version is GameVersion.Japan or GameVersion.Usa or GameVersion.UsaDemo
            ? _pathProvider.GetFullDirectory("bg/", _ndsInfo.Rom.Version) + "picarat_bg.arc"
            : _pathProvider.GetFullDirectory("bg/", _ndsInfo.Rom.Version, language) + "picarat_bg.arc";
    }
}