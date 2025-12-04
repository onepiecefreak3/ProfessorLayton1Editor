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
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Forms.InternalContract;
using UI.Layton1Tool.Forms.Text;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Messages.Enums;

namespace UI.Layton1Tool.Forms.Puzzles.Views;

internal partial class PuzzleDescriptionView
{
    private readonly Layton1NdsInfo _ndsInfo;

    private readonly IEventBroker _eventBroker;
    private readonly IFontProvider _fontProvider;
    private readonly ILayton1NdsFileManager _fileManager;
    private readonly ILayton1PathProvider _pathProvider;

    private Layton1PuzzleId? _puzzleId;
    private TextLanguage? _language;

    private Image<Rgba32>? _topBg;
    private IGlyphProvider? _font;
    private IGlyphProvider? _furiganaFont;

    private Image<Rgba32>? _bg;

    private string? _description;

    public PuzzleDescriptionView(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, IFontProvider fontProvider, ILayton1NdsFileManager fileManager,
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
        eventBroker.Subscribe<SelectedPuzzleDescriptionTextModifiedMessage>(ProcessSelectedPuzzleDescriptionTextModified);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<FileContentModifiedMessage>(ProcessFileContentModified);
        _eventBroker.Unsubscribe<FileAddedMessage>(ProcessFileAdded);
        _eventBroker.Unsubscribe<FontModifiedMessage>(ProcessFontModified);
        _eventBroker.Unsubscribe<SelectedPuzzleChangedMessage>(ProcessSelectedPuzzleChanged);
        _eventBroker.Unsubscribe<SelectedPuzzleLanguageChangedMessage>(ProcessSelectedPuzzleLanguageChanged);
        _eventBroker.Unsubscribe<SelectedPuzzleDescriptionTextModifiedMessage>(ProcessSelectedPuzzleDescriptionTextModified);
    }

    private void ProcessFileContentModified(FileContentModifiedMessage message)
    {
        if (message.Source == this)
            return;

        if (_language is null || _description is null)
            return;

        if (message.File.Rom != _ndsInfo.Rom)
            return;

        if (message.File.Path == GetTopBackgroundPath(_language.Value))
            _topBg = GetTopBackground(_language.Value);
        else
            return;

        if (GenerateBackground(_language.Value))
            GeneratePreview(_description);
    }

    private void ProcessFileAdded(FileAddedMessage message)
    {
        if (message.Source == this)
            return;

        if (_language is null || _description is null)
            return;

        if (message.File.Rom != _ndsInfo.Rom)
            return;

        if (message.File.Path == GetTopBackgroundPath(_language.Value))
            _topBg = GetTopBackground(_language.Value);
        else
            return;

        if (GenerateBackground(_language.Value))
            GeneratePreview(_description);
    }

    private void ProcessFontModified(FontModifiedMessage message)
    {
        if (_language is null || _description is null)
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
            GeneratePreview(_description);
    }

    private void ProcessSelectedPuzzleChanged(SelectedPuzzleChangedMessage message)
    {
        if (message.Rom != _ndsInfo.Rom)
            return;

        if (_description is not null)
            if (GenerateBackground(message.Language))
                GeneratePreview(_description);

        _puzzleId = message.Puzzle;
        _language = message.Language;
    }

    private void ProcessSelectedPuzzleLanguageChanged(SelectedPuzzleLanguageChangedMessage message)
    {
        if (message.Rom != _ndsInfo.Rom)
            return;

        if (_description is not null)
            if (GenerateBackground(message.Language))
                GeneratePreview(_description);

        _language = message.Language;
    }

    private void ProcessSelectedPuzzleDescriptionTextModified(SelectedPuzzleDescriptionTextModifiedMessage message)
    {
        if (_puzzleId is null || _language is null)
            return;

        if (message.Rom != _ndsInfo.Rom)
            return;

        if (message.Puzzle.InternalId != _puzzleId.InternalId)
            return;

        if (SetupBackground(_language.Value))
            GeneratePreview(message.Description);

        _description = message.Description;
    }

    private void GeneratePreview(string description)
    {
        if (_bg is null)
            return;

        if (!SetupFontResources())
            return;

        Image<Rgba32> image = _bg.Clone();

        RenderDescriptionText(image, description);

        _indexImageBox.SetImage(ImageResource.FromImage(image));
    }

    private void RenderDescriptionText(Image<Rgba32> image, string description)
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
                pos = new Point(4, 23);
                break;

            case GameVersion.Usa:
            case GameVersion.UsaDemo:
                deserializer = new CharacterDeserializer();
                layouter = new TextLayouter(new LayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Left, LineHeight = 12 }, _font);
                renderer = new TextRenderer(new RenderOptions { TextColor = Color.Black }, _font);
                pos = new Point(4, 26);
                break;

            case GameVersion.Korea:
                deserializer = new CharacterDeserializer();
                layouter = new TextLayouter(new LayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Left, LineHeight = 13 }, _font);
                renderer = new TextRenderer(new RenderOptions { TextColor = Color.Black }, _font);
                pos = new Point(4, 21);
                break;

            case GameVersion.JapanFriendly:
                if (_furiganaFont is null)
                    return;

                deserializer = new FuriganaCharacterDeserializer(false);
                layouter = new FuriganaTextLayouter(new FuriganaLayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Left, FuriganaLineSpacing = -3, LineHeight = 17 }, _font, _furiganaFont);
                renderer = new FuriganaTextRenderer(new FuriganaRenderOptions { TextColor = Color.Black, FuriganaTextColor = Color.FromRgb(0x9c, 0x94, 0x31) }, _font, _furiganaFont);
                pos = new Point(10, 16);
                break;

            case GameVersion.Japan:
                deserializer = new CharacterDeserializer();
                layouter = new TextLayouter(new LayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Left, LineHeight = 15 }, _font);
                renderer = new TextRenderer(new RenderOptions { TextColor = Color.Black }, _font);
                pos = new Point(9, 28);
                break;

            default:
                throw new InvalidOperationException($"Unknown game version {_ndsInfo.Rom.Version}.");
        }

        var layout = layouter.Create(deserializer.Deserialize(description), pos, new Size(256, 192));
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
        image.Mutate(c => c.Fill(Color.Black, new Rectangle(0, 192, 256, 192)));

        _bg = image;

        return true;
    }

    private bool SetupImageResources(TextLanguage language)
    {
        return SetupLocalizedImageResources(language);
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

    private string GetTopBackgroundPath(TextLanguage language)
    {
        string filePath = _ndsInfo.Rom.Version is GameVersion.Japan or GameVersion.Usa or GameVersion.UsaDemo
            ? _pathProvider.GetFullDirectory("bg/", _ndsInfo.Rom.Version)
            : _pathProvider.GetFullDirectory("bg/", _ndsInfo.Rom.Version, language);

        return filePath + "q_bg.arc";
    }
}