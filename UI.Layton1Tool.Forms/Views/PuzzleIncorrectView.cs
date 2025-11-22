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

internal partial class PuzzleIncorrectView
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

    private string? _incorrect;

    public PuzzleIncorrectView(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, IFontProvider fontProvider, ILayton1NdsFileManager fileManager,
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
        eventBroker.Subscribe<SelectedPuzzleIncorrectTextModifiedMessage>(ProcessSelectedPuzzleIncorrectTextModified);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<FileContentModifiedMessage>(ProcessFileContentModified);
        _eventBroker.Unsubscribe<FileAddedMessage>(ProcessFileAdded);
        _eventBroker.Unsubscribe<FontModifiedMessage>(ProcessFontModified);
        _eventBroker.Unsubscribe<SelectedPuzzleChangedMessage>(ProcessSelectedPuzzleChanged);
        _eventBroker.Unsubscribe<SelectedPuzzleLanguageChangedMessage>(ProcessSelectedPuzzleLanguageChanged);
        _eventBroker.Unsubscribe<SelectedPuzzleIncorrectTextModifiedMessage>(ProcessSelectedPuzzleIncorrectTextModified);
    }

    private void ProcessFileContentModified(FileContentModifiedMessage message)
    {
        if (message.Source == this)
            return;

        if (_language is null || _incorrect is null)
            return;

        if (message.File.Rom != _ndsInfo.Rom)
            return;

        if (message.File.Path == GetTopBackgroundPath(_language.Value))
            _topBg = GetTopBackground(_language.Value);
        else if (message.File.Path == GetBottomBackgroundPath())
            _bottomBg = GetBottomBackground();
        else
            return;

        if (GenerateBackground(_language.Value))
            GeneratePreview(_incorrect);
    }

    private void ProcessFileAdded(FileAddedMessage message)
    {
        if (message.Source == this)
            return;

        if (_language is null || _incorrect is null)
            return;

        if (message.File.Rom != _ndsInfo.Rom)
            return;

        if (message.File.Path == GetTopBackgroundPath(_language.Value))
            _topBg = GetTopBackground(_language.Value);
        else if (message.File.Path == GetBottomBackgroundPath())
            _bottomBg = GetBottomBackground();
        else
            return;

        if (GenerateBackground(_language.Value))
            GeneratePreview(_incorrect);
    }

    private void ProcessFontModified(FontModifiedMessage message)
    {
        if (_language is null || _incorrect is null)
            return;

        if (message.File.Rom != _ndsInfo.Rom)
            return;

        switch (message.Type)
        {
            case FontType.Question:
                _font = message.Font;
                break;

            case FontType.Furigana:
                _furiganaFont = message.Font;
                break;

            default:
                return;
        }

        if (SetupBackground(_language.Value))
            GeneratePreview(_incorrect);
    }

    private void ProcessSelectedPuzzleChanged(SelectedPuzzleChangedMessage message)
    {
        if (message.Rom != _ndsInfo.Rom)
            return;

        if (_incorrect is not null)
            if (GenerateBackground(message.Language))
                GeneratePreview(_incorrect);

        _puzzleId = message.Puzzle;
        _language = message.Language;
    }

    private void ProcessSelectedPuzzleLanguageChanged(SelectedPuzzleLanguageChangedMessage message)
    {
        if (_puzzleId is null)
            return;

        if (message.Rom != _ndsInfo.Rom)
            return;

        if (_incorrect is not null)
            if (GenerateBackground(message.Language))
                GeneratePreview(_incorrect);

        _language = message.Language;
    }

    private void ProcessSelectedPuzzleIncorrectTextModified(SelectedPuzzleIncorrectTextModifiedMessage message)
    {
        if (_puzzleId is null || _language is null)
            return;

        if (message.Rom != _ndsInfo.Rom)
            return;

        if (message.Puzzle.InternalId != _puzzleId.InternalId)
            return;

        if (SetupBackground(_language.Value))
            GeneratePreview(message.Incorrect);

        _incorrect = message.Incorrect;
    }

    private void GeneratePreview(string incorrect)
    {
        if (_bg is null)
            return;

        if (!SetupFontResources())
            return;

        Image<Rgba32> image = _bg.Clone();

        RenderIncorrectText(image, incorrect);

        _indexImageBox.Image = ImageResource.FromImage(image);
    }

    private void RenderIncorrectText(Image<Rgba32> image, string incorrect)
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
                layouter = new TextLayouter(new LayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Left, LineHeight = 12 }, _font);
                renderer = new TextRenderer(new RenderOptions { TextColor = Color.Black }, _font);
                pos = new Point(4, 215);
                break;

            case GameVersion.Usa:
            case GameVersion.UsaDemo:
                deserializer = new CharacterDeserializer();
                layouter = new TextLayouter(new LayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Left, LineHeight = 12 }, _font);
                renderer = new TextRenderer(new RenderOptions { TextColor = Color.Black }, _font);
                pos = new Point(4, 218);
                break;

            case GameVersion.Korea:
                deserializer = new CharacterDeserializer();
                layouter = new TextLayouter(new LayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Left, LineHeight = 13 }, _font);
                renderer = new TextRenderer(new RenderOptions { TextColor = Color.Black }, _font);
                pos = new Point(4, 213);
                break;

            case GameVersion.JapanFriendly:
                if (_furiganaFont is null)
                    return;

                deserializer = new FuriganaCharacterDeserializer(false);
                layouter = new FuriganaTextLayouter(new FuriganaLayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Left, FuriganaLineSpacing = -3, LineHeight = 17 }, _font, _furiganaFont);
                renderer = new FuriganaTextRenderer(new FuriganaRenderOptions { TextColor = Color.Black, FuriganaTextColor = Color.FromRgb(0x9e, 0x96, 0x30) }, _font, _furiganaFont);
                pos = new Point(10, 208);
                break;

            case GameVersion.Japan:
                deserializer = new CharacterDeserializer();
                layouter = new TextLayouter(new LayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Left, LineHeight = 15 }, _font);
                renderer = new TextRenderer(new RenderOptions { TextColor = Color.Black }, _font);
                pos = new Point(9, 220);
                break;

            default:
                throw new InvalidOperationException($"Unknown game version {_ndsInfo.Rom.Version}.");
        }

        var layout = layouter.Create(deserializer.Deserialize(incorrect), pos, new Size(256, 192));
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
        _bottomBg ??= GetBottomBackground();

        return _bottomBg is not null;
    }

    private bool SetupLocalizedImageResources(TextLanguage language)
    {
        if (_language is not null)
        {
            if (_language.Value != language)
            {
                _topBg = GetTopBackground(language);
            }
            else
            {
                _topBg ??= GetTopBackground(language);
            }
        }
        else
        {
            _topBg ??= GetTopBackground(language);
        }

        return _topBg is not null;
    }

    private bool SetupFontResources()
    {
        _font = _fontProvider.GetQuestionFont(_ndsInfo.Rom);
        _furiganaFont = _fontProvider.GetFuriganaFont(_ndsInfo.Rom);

        bool isLoaded = _font is not null;

        if (_ndsInfo.Rom.Version is GameVersion.JapanFriendly)
            isLoaded &= _furiganaFont is not null;

        return isLoaded;
    }

    private Image<Rgba32>? GetTopBackground(TextLanguage language)
    {
        string filePath = GetTopBackgroundPath(language);

        if (!_fileManager.TryGet(_ndsInfo.Rom, filePath, out Layton1NdsFile? file))
            return null;

        return (Image<Rgba32>?)_fileManager.Parse(file, FileType.Bgx);
    }

    private Image<Rgba32>? GetBottomBackground()
    {
        string filePath = GetBottomBackgroundPath();

        if (!_fileManager.TryGet(_ndsInfo.Rom, filePath, out Layton1NdsFile? file))
            return null;

        return (Image<Rgba32>?)_fileManager.Parse(file, FileType.Bgx);
    }

    private string GetTopBackgroundPath(TextLanguage language)
    {
        string filePath = _ndsInfo.Rom.Version is GameVersion.Japan or GameVersion.Usa or GameVersion.UsaDemo
            ? _pathProvider.GetFullDirectory("bg/", _ndsInfo.Rom.Version)
            : _pathProvider.GetFullDirectory("bg/", _ndsInfo.Rom.Version, language);

        return filePath + "judge_r114_bg.arc";
    }

    private string GetBottomBackgroundPath()
    {
        return _pathProvider.GetFullDirectory("bg/qend_awrong.arc", _ndsInfo.Rom.Version);
    }
}