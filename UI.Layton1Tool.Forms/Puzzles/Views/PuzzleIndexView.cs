using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Resources;
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

internal partial class PuzzleIndexView
{
    private readonly Layton1NdsInfo _ndsInfo;

    private readonly IEventBroker _eventBroker;
    private readonly IFontProvider _fontProvider;
    private readonly ILayton1NdsFileManager _fileManager;
    private readonly ILayton1PathProvider _pathProvider;

    private Layton1PuzzleId? _puzzleId;
    private TextLanguage? _language;

    private Image<Rgba32>? _topBg;
    private Image<Rgba32>? _topIcon;
    private Image<Rgba32>? _bottomBg;
    private Image<Rgba32>? _bottomListButtons;
    private IGlyphProvider? _questionFont;
    private IGlyphProvider? _eventFont;

    private Image<Rgba32>? _bg;

    private string? _title;
    private string? _type;
    private string? _location;

    public PuzzleIndexView(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, IFontProvider fontProvider, ILayton1NdsFileManager fileManager,
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
        eventBroker.Subscribe<SelectedPuzzleIndexTextsModifiedMessage>(ProcessSelectedPuzzleIndexTextsChanged);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<FileContentModifiedMessage>(ProcessFileContentModified);
        _eventBroker.Unsubscribe<FileAddedMessage>(ProcessFileAdded);
        _eventBroker.Unsubscribe<FontModifiedMessage>(ProcessFontModified);
        _eventBroker.Unsubscribe<SelectedPuzzleChangedMessage>(ProcessSelectedPuzzleChanged);
        _eventBroker.Unsubscribe<SelectedPuzzleLanguageChangedMessage>(ProcessSelectedPuzzleLanguageChanged);
        _eventBroker.Unsubscribe<SelectedPuzzleIndexTextsModifiedMessage>(ProcessSelectedPuzzleIndexTextsChanged);
    }

    private void ProcessFileContentModified(FileContentModifiedMessage message)
    {
        if (message.Source == this)
            return;

        if (_puzzleId is null || _language is null || _title is null || _type is null || _location is null)
            return;

        if (message.File.Rom != _ndsInfo.Rom)
            return;

        if (message.File.Path == GetTopBackgroundPath(_language.Value))
            _topBg = GetTopBackground(_language.Value);
        else if (message.File.Path == GetTopIconPath(_puzzleId) ||
                 message.File.Path == GetTopIconDefaultPath())
            _topIcon = GetTopIcon(_puzzleId);
        else if (message.File.Path == GetBottomBackgroundPath())
            _bottomBg = GetBottomBackground();
        else if (message.File.Path == GetBottomListButtonsPath(_language.Value))
            _bottomListButtons = GetBottomListButtons(_language.Value);
        else
            return;

        if (GenerateBackground(_puzzleId, _language.Value))
            GeneratePreview(_title, _type, _location);
    }

    private void ProcessFileAdded(FileAddedMessage message)
    {
        if (message.Source == this)
            return;

        if (_puzzleId is null || _language is null || _title is null || _type is null || _location is null)
            return;

        if (message.File.Rom != _ndsInfo.Rom)
            return;

        if (message.File.Path == GetTopBackgroundPath(_language.Value))
            _topBg = GetTopBackground(_language.Value);
        else if (message.File.Path == GetTopIconPath(_puzzleId) ||
                 message.File.Path == GetTopIconDefaultPath())
            _topIcon = GetTopIcon(_puzzleId);
        else if (message.File.Path == GetBottomBackgroundPath())
            _bottomBg = GetBottomBackground();
        else if (message.File.Path == GetBottomListButtonsPath(_language.Value))
            _bottomListButtons = GetBottomListButtons(_language.Value);
        else
            return;

        if (GenerateBackground(_puzzleId, _language.Value))
            GeneratePreview(_title, _type, _location);
    }

    private void ProcessFontModified(FontModifiedMessage message)
    {
        if (_puzzleId is null || _language is null || _title is null || _type is null || _location is null)
            return;

        if (message.File.Rom != _ndsInfo.Rom)
            return;

        switch (message.Type)
        {
            case FontType.Question:
                _questionFont = message.Font;
                break;

            case FontType.Event:
                _eventFont = message.Font;
                break;

            default:
                return;
        }

        if (SetupBackground(_puzzleId, _language.Value))
            GeneratePreview(_title, _type, _location);
    }

    private void ProcessSelectedPuzzleChanged(SelectedPuzzleChangedMessage message)
    {
        if (message.Rom != _ndsInfo.Rom)
            return;

        if (_title is not null && _type is not null && _location is not null)
            if (GenerateBackground(message.Puzzle, message.Language))
                GeneratePreview(_title, _type, _location);

        _puzzleId = message.Puzzle;
        _language = message.Language;
    }

    private void ProcessSelectedPuzzleLanguageChanged(SelectedPuzzleLanguageChangedMessage message)
    {
        if (_puzzleId is null)
            return;

        if (message.Rom != _ndsInfo.Rom)
            return;

        if (_title is not null && _type is not null && _location is not null)
            if (GenerateBackground(_puzzleId, message.Language))
                GeneratePreview(_title, _type, _location);

        _language = message.Language;
    }

    private void ProcessSelectedPuzzleIndexTextsChanged(SelectedPuzzleIndexTextsModifiedMessage message)
    {
        if (_puzzleId is null || _language is null)
            return;

        if (message.Rom != _ndsInfo.Rom)
            return;

        if (message.Puzzle.InternalId != _puzzleId.InternalId)
            return;

        if (SetupBackground(_puzzleId, _language.Value))
            GeneratePreview(message.Title, message.Type, message.Location);

        _title = message.Title;
        _type = message.Type;
        _location = message.Location;
    }

    private void GeneratePreview(string title, string type, string location)
    {
        if (_bg is null)
            return;

        if (!SetupFontResources())
            return;

        Image<Rgba32> image = _bg.Clone();

        RenderTopTitleText(image, title);
        RenderTypeText(image, type);
        RenderLocationText(image, location);
        RenderBottomTitleText(image, title);

        _indexImageBox.SetImage(ImageResource.FromImage(image));
    }

    private void RenderTopTitleText(Image<Rgba32> image, string title)
    {
        if (_questionFont is null)
            return;

        ICharacterDeserializer deserializer;
        Point pos;
        int spacing;

        switch (_ndsInfo.Rom.Version)
        {
            case GameVersion.Europe:
            case GameVersion.EuropeDemo:
                deserializer = new DiacriticCharacterDeserializer();
                pos = new Point(0, 26);
                spacing = 1;
                break;

            case GameVersion.Usa:
            case GameVersion.UsaDemo:
                deserializer = new CharacterDeserializer();
                pos = new Point(0, 26);
                spacing = 2;
                break;

            case GameVersion.Korea:
                deserializer = new CharacterDeserializer();
                pos = new Point(0, 22);
                spacing = 2;
                break;

            case GameVersion.JapanFriendly:
                deserializer = new FuriganaCharacterDeserializer(true);
                pos = new Point(2, 13);
                spacing = 1;
                break;

            case GameVersion.Japan:
                deserializer = new CharacterDeserializer();
                pos = new Point(2, 13);
                spacing = 1;
                break;

            default:
                throw new InvalidOperationException($"Unknown game version {_ndsInfo.Rom.Version}.");
        }

        var layouter = new TextLayouter(new LayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Center, TextSpacing = spacing }, _questionFont);
        var renderer = new TextRenderer(new RenderOptions { TextColor = Color.Black }, _questionFont);

        var layout = layouter.Create(deserializer.Deserialize(title), pos, new Size(256, 192));
        renderer.Render(image, layout);
    }

    private void RenderBottomTitleText(Image<Rgba32> image, string title)
    {
        if (_questionFont is null)
            return;

        ICharacterDeserializer deserializer;
        Point pos;

        switch (_ndsInfo.Rom.Version)
        {
            case GameVersion.Europe:
            case GameVersion.EuropeDemo:
                deserializer = new DiacriticCharacterDeserializer();
                pos = new Point(101, 253);
                break;

            case GameVersion.Usa:
            case GameVersion.UsaDemo:
                deserializer = new CharacterDeserializer();
                pos = new Point(101, 253);
                break;

            case GameVersion.Korea:
                deserializer = new CharacterDeserializer();
                pos = new Point(101, 249);
                break;

            case GameVersion.JapanFriendly:
                deserializer = new FuriganaCharacterDeserializer(true);
                pos = new Point(101, 250);
                break;

            case GameVersion.Japan:
                deserializer = new CharacterDeserializer();
                pos = new Point(101, 250);
                break;

            default:
                throw new InvalidOperationException($"Unknown game version {_ndsInfo.Rom.Version}.");
        }

        var layouter = new TextLayouter(new LayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Left }, _questionFont);
        var renderer = new TextRenderer(new RenderOptions { TextColor = Color.Black }, _questionFont);

        var layout = layouter.Create(deserializer.Deserialize(title), pos, new Size(256, 192));
        renderer.Render(image, layout);
    }

    private void RenderTypeText(Image<Rgba32> image, string type)
    {
        if (_eventFont is null)
            return;

        ICharacterDeserializer deserializer;
        Point pos;

        switch (_ndsInfo.Rom.Version)
        {
            case GameVersion.Europe:
            case GameVersion.EuropeDemo:
                deserializer = new DiacriticCharacterDeserializer();
                pos = new Point(59, 142);
                break;

            case GameVersion.Usa:
            case GameVersion.UsaDemo:
                deserializer = new CharacterDeserializer();
                pos = new Point(67, 142);
                break;

            case GameVersion.Korea:
                deserializer = new CharacterDeserializer();
                pos = new Point(67, 142);
                break;

            case GameVersion.JapanFriendly:
                deserializer = new FuriganaCharacterDeserializer(true);
                pos = new Point(61, 142);
                break;

            case GameVersion.Japan:
                deserializer = new CharacterDeserializer();
                pos = new Point(61, 142);
                break;

            default:
                throw new InvalidOperationException($"Unknown game version {_ndsInfo.Rom.Version}.");
        }

        var layouter = new TextLayouter(new LayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Center }, _eventFont);
        var renderer = new TextRenderer(new RenderOptions { TextColor = Color.Black }, _eventFont);

        var layout = layouter.Create(deserializer.Deserialize(type), pos, new Size(256, 192));
        renderer.Render(image, layout);
    }

    private void RenderLocationText(Image<Rgba32> image, string location)
    {
        if (_eventFont is null)
            return;

        ICharacterDeserializer deserializer;
        Point pos;

        switch (_ndsInfo.Rom.Version)
        {
            case GameVersion.Europe:
            case GameVersion.EuropeDemo:
                deserializer = new DiacriticCharacterDeserializer();
                pos = new Point(59, 163);
                break;

            case GameVersion.Usa:
            case GameVersion.UsaDemo:
                deserializer = new CharacterDeserializer();
                pos = new Point(73, 163);
                break;

            case GameVersion.Korea:
                deserializer = new CharacterDeserializer();
                pos = new Point(73, 163);
                break;

            case GameVersion.JapanFriendly:
                deserializer = new FuriganaCharacterDeserializer(true);
                pos = new Point(61, 163);
                break;

            case GameVersion.Japan:
                deserializer = new CharacterDeserializer();
                pos = new Point(61, 163);
                break;

            default:
                throw new InvalidOperationException($"Unknown game version {_ndsInfo.Rom.Version}.");
        }

        var layouter = new TextLayouter(new LayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Center }, _eventFont);
        var renderer = new TextRenderer(new RenderOptions { TextColor = Color.Black }, _eventFont);

        var layout = layouter.Create(deserializer.Deserialize(location), pos, new Size(256, 192));
        renderer.Render(image, layout);
    }

    private bool SetupBackground(Layton1PuzzleId puzzleId, TextLanguage language)
    {
        if (_bg is not null)
            return true;

        return GenerateBackground(puzzleId, language);
    }

    private bool GenerateBackground(Layton1PuzzleId puzzleId, TextLanguage language)
    {
        if (!SetupImageResources(puzzleId, language))
            return false;

        var image = new Image<Rgba32>(256, 192 * 2);

        image.Mutate(c => c.DrawImage(_topBg!, Point.Empty, 1f));
        image.Mutate(c => c.DrawImage(_topIcon!, Point.Empty, 1f));
        image.Mutate(c => c.DrawImage(_bottomBg!, new Point(0, 192), 1f));
        image.Mutate(c => c.DrawImage(_bottomListButtons!, new Point(0, 192), 1f));

        _bg = image;

        return true;
    }

    private bool SetupImageResources(Layton1PuzzleId puzzleId, TextLanguage language)
    {
        return SetupStaticImageResources() &&
               SetupLocalizedImageResources(language) &&
               SetupPuzzleImageResources(puzzleId);
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
                _bottomListButtons = GetBottomListButtons(language);
            }
            else
            {
                _topBg ??= GetTopBackground(language);
                _bottomListButtons ??= GetBottomListButtons(language);
            }
        }
        else
        {
            _topBg ??= GetTopBackground(language);
            _bottomListButtons ??= GetBottomListButtons(language);
        }

        return _topBg is not null && _bottomListButtons is not null;
    }

    private bool SetupPuzzleImageResources(Layton1PuzzleId puzzleId)
    {
        if (_puzzleId is not null)
        {
            if (puzzleId.InternalId != _puzzleId.InternalId)
                _topIcon = GetTopIcon(puzzleId);
            else
                _topIcon ??= GetTopIcon(puzzleId);
        }
        else
        {
            _topIcon ??= GetTopIcon(puzzleId);
        }

        return _topIcon is not null;
    }

    private bool SetupFontResources()
    {
        _questionFont = _fontProvider.GetQuestionFont(_ndsInfo.Rom);
        _eventFont = _fontProvider.GetEventFont(_ndsInfo.Rom);

        return _questionFont is not null && _eventFont is not null;
    }

    private Image<Rgba32>? GetTopBackground(TextLanguage language)
    {
        string filePath = GetTopBackgroundPath(language);

        if (!_fileManager.TryGet(_ndsInfo.Rom, filePath, out Layton1NdsFile? file))
            return null;

        return (Image<Rgba32>?)_fileManager.Parse(file, FileType.Bgx);
    }

    private Image<Rgba32>? GetTopIcon(Layton1PuzzleId puzzleId)
    {
        string filePath = GetTopIconPath(puzzleId);

        if (!_fileManager.TryGet(_ndsInfo.Rom, filePath, out Layton1NdsFile? file))
        {
            filePath = GetTopIconDefaultPath();

            if (!_fileManager.TryGet(_ndsInfo.Rom, filePath, out file))
                return null;
        }

        var image = (Image<Rgba32>?)_fileManager.Parse(file, FileType.Bgx);
        if (image is null)
            return null;

        var options = new DrawingOptions { GraphicsOptions = new GraphicsOptions { AlphaCompositionMode = PixelAlphaCompositionMode.Src } };
        image.Mutate(c => c.Fill(options, new RecolorBrush(Color.FromRgb(0xe7, 0x00, 0x7b), Color.Transparent, .0f)));

        return image;
    }

    private Image<Rgba32>? GetBottomBackground()
    {
        string filePath = GetBottomBackgroundPath();

        if (!_fileManager.TryGet(_ndsInfo.Rom, filePath, out Layton1NdsFile? file))
            return null;

        return (Image<Rgba32>?)_fileManager.Parse(file, FileType.Bgx);
    }

    private Image<Rgba32>? GetBottomListButtons(TextLanguage language)
    {
        string filePath = GetBottomListButtonsPath(language);

        if (!_fileManager.TryGet(_ndsInfo.Rom, filePath, out Layton1NdsFile? file))
            return null;

        var image = (Image<Rgba32>?)_fileManager.Parse(file, FileType.Bgx);
        if (image is null)
            return null;

        var options = new DrawingOptions { GraphicsOptions = new GraphicsOptions { AlphaCompositionMode = PixelAlphaCompositionMode.Src } };
        image.Mutate(c => c.Fill(options, new RecolorBrush(Color.FromRgb(0xe7, 0x00, 0x7b), Color.Transparent, .0f)));

        return image;
    }

    private string GetTopBackgroundPath(TextLanguage language)
    {
        string filePath = _ndsInfo.Rom.Version is GameVersion.Japan or GameVersion.Usa or GameVersion.UsaDemo
            ? _pathProvider.GetFullDirectory("bg/", _ndsInfo.Rom.Version)
            : _pathProvider.GetFullDirectory("bg/", _ndsInfo.Rom.Version, language);

        return filePath + "jiten_sub.arc";
    }

    private string GetTopIconPath(Layton1PuzzleId puzzleId)
    {
        return _pathProvider.GetFullDirectory($"bg/jiten_q{puzzleId.InternalId}.arc", _ndsInfo.Rom.Version);
    }

    private string GetTopIconDefaultPath()
    {
        return _pathProvider.GetFullDirectory("bg/jiten_q00.arc", _ndsInfo.Rom.Version);
    }

    private string GetBottomBackgroundPath()
    {
        return _pathProvider.GetFullDirectory("bg/jiten_bg.arc", _ndsInfo.Rom.Version);
    }

    private string GetBottomListButtonsPath(TextLanguage language)
    {
        string filePath = _ndsInfo.Rom.Version is GameVersion.Japan or GameVersion.Usa or GameVersion.UsaDemo
            ? _pathProvider.GetFullDirectory("bg/", _ndsInfo.Rom.Version)
            : _pathProvider.GetFullDirectory("bg/", _ndsInfo.Rom.Version, language);

        return filePath + "jiten_over.arc";
    }
}