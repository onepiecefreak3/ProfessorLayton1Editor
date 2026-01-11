using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Resources;
using Kaligraphy.Contract.DataClasses.Parsing;
using Kaligraphy.Contract.Layout;
using Kaligraphy.Contract.Parsing;
using Kaligraphy.Contract.Rendering;
using Kaligraphy.DataClasses.Layout;
using Kaligraphy.DataClasses.Parsing;
using Kaligraphy.DataClasses.Rendering;
using Kaligraphy.Enums.Layout;
using Kaligraphy.Layout;
using Kaligraphy.Rendering;
using Logic.Business.Layton1ToolManagement.Contract;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.Contract.Enums.Texts;
using Logic.Business.Layton1ToolManagement.Contract.Files;
using Logic.Business.Layton1ToolManagement.Contract.Scripts;
using Logic.Domain.Level5Management.Contract.DataClasses.Animations;
using Logic.Domain.Level5Management.Contract.DataClasses.Archives;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using UI.Layton1Tool.Components.Contract;
using UI.Layton1Tool.Components.Contract.DataClasses;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Forms.InternalContract;
using UI.Layton1Tool.Forms.Text;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Messages.DataClasses;
using UI.Layton1Tool.Messages.Enums;

namespace UI.Layton1Tool.Forms.Events.Views;

internal partial class EventView
{
    private readonly Layton1NdsInfo _ndsInfo;

    private readonly IEventBroker _eventBroker;
    private readonly ILayton1NdsFileManager _fileManager;
    private readonly ILayton1PcmFileManager _pcmManager;
    private readonly ILayton1PathProvider _pathProvider;
    private readonly ILayton1ScriptReducer _scriptReducer;
    private readonly IAnimationStateManager _animationManager;
    private readonly IFontProvider _fontProvider;

    private int? _eventId;
    private TextElement? _selectedText;
    private int? _selectedTextIndex;
    private TextLanguage? _language;
    private GameState? _states;

    private string? _bottomPath;
    private ImageResource? _bottomBg;
    private string? _topPath;
    private ImageResource? _topBg;
    private Color? _bottomColor;
    private TextElement[] _texts;
    private ImageResource? _textImage;
    private AnimationSequences? _eventCursorAnimations;
    private AnimationSequences? _eventWindowAnimations;
    private AnimationState? _eventCursorAnimation;
    private AnimationState? _eventWindowAnimation;
    private List<string> _personObjectPaths = [];
    private List<(int, int, AnimationState)> _personObjects = [];

    private GdsScriptFile? _script;

    public EventView(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ILayton1NdsFileManager fileManager, ILayton1PcmFileManager pcmManager,
        ILayton1PathProvider pathProvider, ILayton1ScriptReducer scriptReducer, IAnimationStateManager animationManager, IFontProvider fontProvider)
    {
        _ndsInfo = ndsInfo;
        _eventBroker = eventBroker;
        _fileManager = fileManager;
        _pcmManager = pcmManager;
        _pathProvider = pathProvider;
        _scriptReducer = scriptReducer;
        _animationManager = animationManager;
        _fontProvider = fontProvider;

        eventBroker.Subscribe<FileContentModifiedMessage>(ProcessFileContentModified);
        eventBroker.Subscribe<FileAddedMessage>(ProcessFileAdded);
        eventBroker.Subscribe<FontModifiedMessage>(ProcessFontModified);
        eventBroker.Subscribe<SelectedEventLanguageChangedMessage>(ProcessSelectedEventLanguageChanged);
        eventBroker.Subscribe<SelectedEventModifiedMessage>(ProcessSelectedEventModified);
        eventBroker.Subscribe<SelectedEventFlagsUpdatedMessage>(ProcessSelectedEventFlagsUpdated);
        eventBroker.Subscribe<EventScriptUpdatedMessage>(ProcessEventScriptUpdated);
        eventBroker.Subscribe<SelectedEventFlagsModifiedMessage>(ProcessSelectedEventFlagsModified);
        eventBroker.Subscribe<SelectedEventViewTextChangedMessage>(ProcessSelectedEventViewTextChanged);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<FileContentModifiedMessage>(ProcessFileContentModified);
        _eventBroker.Unsubscribe<FileAddedMessage>(ProcessFileAdded);
        _eventBroker.Unsubscribe<FontModifiedMessage>(ProcessFontModified);
        _eventBroker.Unsubscribe<SelectedEventLanguageChangedMessage>(ProcessSelectedEventLanguageChanged);
        _eventBroker.Unsubscribe<SelectedEventModifiedMessage>(ProcessSelectedEventModified);
        _eventBroker.Unsubscribe<SelectedEventFlagsUpdatedMessage>(ProcessSelectedEventFlagsUpdated);
        _eventBroker.Unsubscribe<EventScriptUpdatedMessage>(ProcessEventScriptUpdated);
        _eventBroker.Unsubscribe<SelectedEventFlagsModifiedMessage>(ProcessSelectedEventFlagsModified);
        _eventBroker.Unsubscribe<SelectedEventViewTextChangedMessage>(ProcessSelectedEventViewTextChanged);
    }

    private void ProcessFileContentModified(FileContentModifiedMessage message)
    {
        if (message.Source == this)
            return;

        if (message.File.Rom != _ndsInfo.Rom)
            return;

        UpdateResources(message.File, message.Content);
    }

    private void ProcessFileAdded(FileAddedMessage message)
    {
        if (message.Source == this)
            return;

        if (message.File.Rom != _ndsInfo.Rom)
            return;

        object? content = _fileManager.Parse(message.File, out _);

        UpdateResources(message.File, content);
    }

    private void ProcessFontModified(FontModifiedMessage message)
    {
        if (message.File.Rom != _ndsInfo.Rom)
            return;

        if (message.Type is not FontType.Event)
            return;

        if (_selectedText is null || !_selectedTextIndex.HasValue)
            return;

        RenderText(_selectedText.Texts[_selectedTextIndex.Value]);
    }

    private void UpdateResources(Layton1NdsFile file, object? content)
    {
        if (!_eventId.HasValue || !_language.HasValue)
            return;

        if (file.Path == _bottomPath && content is Image<Rgba32> newImage1)
        {
            _bottomBg?.Destroy();
            _bottomBg = ImageResource.FromImage(newImage1);
        }
        else if (file.Path == _topPath && content is Image<Rgba32> newImage2)
        {
            _topBg?.Destroy();
            _topBg = ImageResource.FromImage(newImage2);
        }
        else if (_selectedText is not null && file.Path == GetEventWindowPath() && content is AnimationSequences newEventWindow)
        {
            _eventWindowAnimations = newEventWindow;
            _eventWindowAnimation = CreateAnimationState(newEventWindow, _selectedText.SpeakerWindow);
        }
        else if (file.Path == GetEventCursorPath() && content is AnimationSequences newEventCursor)
        {
            _eventCursorAnimations = newEventCursor;
            _eventCursorAnimation = CreateAnimationState(newEventCursor, "touch");
        }
        else if (_selectedText is not null && file.Path == GetBaseTextPath(_eventId.Value, _ndsInfo.Rom.Version, _language.Value))
        {
            string? text = GetEventText(_eventId.Value, _selectedText.TextId, _ndsInfo.Rom.Version, _language.Value);

            if (text is not null)
            {
                _selectedText.Texts = SplitText(text);

                RaiseSelectedEventViewTextsUpdated(_texts, true);
            }
        }
        else if (content is AnimationSequences newAnimations)
        {
            int animationIndex = _personObjectPaths.FindIndex(p => p == file.Path);
            if (animationIndex > 0)
            {
                foreach (ImageResource image in _personObjects[animationIndex].Item3.Images)
                    image.Destroy();

                _personObjects[animationIndex] = _personObjects[animationIndex] with { Item3 = CreateAnimationState(newAnimations) };
            }
        }
    }

    private void LoadTextWindow()
    {
        if (_selectedText is null)
            return;

        LoadEventWindows();
        if (_eventWindowAnimations is not null)
            _eventWindowAnimation = CreateAnimationState(_eventWindowAnimations, _selectedText.SpeakerWindow);

        LoadEventCursor();
        if (_eventCursorAnimations is not null)
            _eventCursorAnimation ??= CreateAnimationState(_eventCursorAnimations, "touch");
    }

    private void ProcessSelectedEventLanguageChanged(SelectedEventLanguageChangedMessage message)
    {
        if (!_eventId.HasValue)
            return;

        if (message.Rom != _ndsInfo.Rom)
            return;

        if (_script is not null && _states is not null)
            UpdateResources(_script, _eventId.Value, message.Language, _states, true);

        _language = message.Language;
    }

    private void ProcessSelectedEventModified(SelectedEventModifiedMessage message)
    {
        if (message.Rom != _ndsInfo.Rom)
            return;

        if (_eventId.HasValue && _eventId != message.Event)
            return;

        if (_language.HasValue && _states is not null)
            UpdateResources(message.Script, message.Event, _language.Value, _states, true);

        _script = message.Script;
    }

    private void ProcessSelectedEventFlagsUpdated(SelectedEventFlagsUpdatedMessage message)
    {
        if (message.Rom != _ndsInfo.Rom)
            return;

        if (_eventId.HasValue && _eventId != message.Event)
            return;

        if (_language.HasValue && _script is not null)
            UpdateResources(_script, message.Event, _language.Value, message.States, true);

        _states = message.States;
    }

    private void ProcessEventScriptUpdated(EventScriptUpdatedMessage message)
    {
        if (message.Rom != _ndsInfo.Rom)
            return;

        UpdateResources(message.Script, message.Event, message.Language, message.States, false);

        _eventId = message.Event;
        _language = message.Language;
        _states = message.States;
        _script = message.Script;
    }

    private void ProcessSelectedEventFlagsModified(SelectedEventFlagsModifiedMessage message)
    {
        if (_script is null || _states is null || !_eventId.HasValue || !_language.HasValue)
            return;

        if (message.Rom != _ndsInfo.Rom)
            return;

        if (_eventId != message.Event)
            return;

        UpdateResources(_script, _eventId.Value, _language.Value, _states, false);
    }

    private void ProcessSelectedEventViewTextChanged(SelectedEventViewTextChangedMessage message)
    {
        if (message.Target != this)
            return;

        if (message.Rom != _ndsInfo.Rom)
            return;

        _selectedText = message.Text;
        _selectedTextIndex = message.TextIndex;

        RenderText(message.Text.Texts[message.TextIndex]);

        LoadTextWindow();
    }

    private void RaiseSelectedEventViewTextsUpdated(TextElement[] texts, bool keepIndex)
    {
        _eventBroker.Raise(new SelectedEventViewTextsUpdatedMessage(this, _ndsInfo.Rom, texts, keepIndex));
    }

    private void UpdateResources(GdsScriptFile script, int eventId, TextLanguage language, GameState states, bool keepTextIndex)
    {
        var hasTopImage = false;
        var hasBottomImage = false;

        var texts = new List<TextElement>();
        var personObjectPaths = new List<string>();
        var personObjects = new List<(int, int, AnimationState)>();

        var reducedInstructions = _scriptReducer.Reduce(script, states);

        foreach (GdsScriptInstruction instruction in reducedInstructions)
        {
            if (instruction.Arguments.Length > 1 && instruction.Arguments[0].Value is 11)
            {
                if (instruction.Arguments[1].Value is string bottomPath)
                {
                    bottomPath = _pathProvider.GetFullDirectory($"bg/{bottomPath}".Replace(".bgx", ".arc"),
                        _ndsInfo.Rom.Version, language);
                    ImageResource? bottomBg = GetImage(bottomPath);

                    if (bottomBg is not null)
                    {
                        hasBottomImage = true;

                        _bottomPath = bottomPath;
                        _bottomBg = bottomBg;
                    }
                }
            }
            else if (instruction.Arguments.Length > 1 && instruction.Arguments[0].Value is 12)
            {
                if (instruction.Arguments[1].Value is string topPath)
                {
                    topPath = _pathProvider.GetFullDirectory($"bg/{topPath}".Replace(".bgx", ".arc"),
                        _ndsInfo.Rom.Version, language);
                    ImageResource? topBg = GetImage(topPath);

                    if (topBg is not null)
                    {
                        hasTopImage = true;

                        _topPath = topPath;
                        _topBg = topBg;
                    }
                }
            }
            else if (instruction.Arguments.Length > 4 && instruction.Arguments[0].Value is 123)
            {
                if (instruction.Arguments[1].Value is int red && instruction.Arguments[2].Value is int green &&
                    instruction.Arguments[3].Value is int blue && instruction.Arguments[4].Value is int alpha)
                    _bottomColor = new Rgba32((byte)red, (byte)green, (byte)blue, (byte)alpha);
            }
            else if (instruction.Arguments.Length > 2 && instruction.Arguments[0].Value is 156 or 157)
            {
                if (instruction.Arguments[1].Value is not int textId)
                    continue;

                string? text = GetEventText(eventId, textId, _ndsInfo.Rom.Version, language);

                if (text is null)
                    continue;

                texts.Add(new TextElement
                {
                    TextId = textId,
                    SpeakerWindow = instruction.Arguments[0].Value is 156 ? "gfx" : "gfx2",
                    Texts = SplitText(text)
                });
            }
            else if (instruction.Arguments.Length > 4 && instruction.Arguments[0].Value is 108)
            {
                if (instruction.Arguments[3].Value is not string personObjectPath)
                    continue;

                if (instruction.Arguments[4].Value is not string personObjectName)
                    continue;

                personObjectPath = GetAnimationObjectPath(personObjectPath);
                int personIndex = _personObjectPaths.IndexOf(personObjectPath);

                if (instruction.Arguments[1].Value is int x && instruction.Arguments[2].Value is int y)
                {
                    if (personIndex >= 0)
                    {
                        personObjectPaths.Add(personObjectPath);
                        personObjects.Add((x, y, _personObjects[personIndex].Item3));
                        continue;
                    }

                    AnimationSequences? animations = GetAnimation(personObjectPath);
                    if (animations is null)
                        continue;

                    if (animations.Sequences.Length <= 0)
                        continue;

                    personObjectPaths.Add(personObjectPath);
                    personObjects.Add((x, y, CreateAnimationState(animations, personObjectName)));
                }
            }
            else if (instruction.Arguments.Length > 2 && instruction.Arguments[0].Value is 110)
            {
                if (instruction.Arguments[1].Value is not int personIndex || personIndex >= _personObjects.Count)
                    continue;

                if (instruction.Arguments[2].Value is not string personObjectName)
                    continue;

                var personObject = _personObjects[personIndex];
                _personObjects[personIndex] = (personObject.Item1, personObject.Item2, CloneAnimationState(personObject.Item3, personObjectName));
            }
        }

        if (!hasBottomImage)
            _bottomPath = null;
        if (!hasTopImage)
            _topPath = null;

        _texts = [.. texts];
        _personObjectPaths = personObjectPaths;
        _personObjects = personObjects;

        RaiseSelectedEventViewTextsUpdated(_texts, keepTextIndex);

        LoadTextWindow();
    }

    private void RenderText(IList<CharacterData> text)
    {
        IGlyphProvider? font = _fontProvider.GetEventFont(_ndsInfo.Rom);
        if (font is null)
            return;

        ITextLayouter layouter;
        ITextRenderer renderer;
        Point pos;

        switch (_ndsInfo.Rom.Version)
        {
            case GameVersion.Europe:
            case GameVersion.EuropeDemo:
                layouter = new TextLayouter(new LayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Left, LineHeight = 16 }, font);
                renderer = new TextRenderer(new RenderOptions { TextColor = Color.Black }, font);
                pos = new Point(14, 141);
                break;

            case GameVersion.Usa:
            case GameVersion.UsaDemo:
                layouter = new TextLayouter(new LayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Left, LineHeight = 16 }, font);
                renderer = new TextRenderer(new RenderOptions { TextColor = Color.Black }, font);
                pos = new Point(15, 141);
                break;

            case GameVersion.Korea:
                layouter = new TextLayouter(new LayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Left, LineHeight = 13 }, font);
                renderer = new TextRenderer(new RenderOptions { TextColor = Color.Black }, font);
                pos = new Point(14, 141);
                break;

            case GameVersion.JapanFriendly:
                IGlyphProvider? furiganaFont = _fontProvider.GetFuriganaFont(_ndsInfo.Rom);
                if (furiganaFont is null)
                    return;

                layouter = new FuriganaTextLayouter(new FuriganaLayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Left, LineHeight = 19, FuriganaLineSpacing = -2 }, font, furiganaFont);
                renderer = new FuriganaTextRenderer(new FuriganaRenderOptions { TextColor = Color.Black, FuriganaTextColor = Color.FromRgb(0xae, 0x79, 0x38) }, font, furiganaFont);
                pos = new Point(14, 132);
                break;

            case GameVersion.Japan:
                layouter = new TextLayouter(new LayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Left, LineHeight = 16 }, font);
                renderer = new TextRenderer(new RenderOptions { TextColor = Color.Black }, font);
                pos = new Point(17, 141);
                break;

            default:
                throw new InvalidOperationException($"Unknown game version {_ndsInfo.Rom.Version}.");
        }

        var textImage = new Image<Rgba32>(256, 192);

        var layout = layouter.Create(text, pos, new Size(256, 192));
        renderer.Render(textImage, layout);

        _textImage = ImageResource.FromImage(textImage);
    }

    private List<CharacterData>[] SplitText(string text)
    {
        var deserializer = GetCharacterDeserializer();
        var characters = deserializer.Deserialize(text);

        var parts = new List<List<CharacterData>> { new() };

        var removedLinebreak = false;
        for (var i = 0; i < characters.Count; i++)
        {
            if (parts[^1].Count <= 0 && !removedLinebreak && characters[i] is LineBreakCharacterData)
            {
                removedLinebreak = true;
                i++;
            }

            if (characters[i] is BreakCharacterData)
            {
                removedLinebreak = false;

                parts.Add([]);
                continue;
            }

            parts[^1].Add(characters[i]);
        }

        return [.. parts];
    }

    private ICharacterDeserializer GetCharacterDeserializer()
    {
        switch (_ndsInfo.Rom.Version)
        {
            case GameVersion.Europe:
            case GameVersion.EuropeDemo:
                return new DiacriticCharacterDeserializer();

            case GameVersion.JapanFriendly:
                return new FuriganaCharacterDeserializer(false);

            case GameVersion.Usa:
            case GameVersion.UsaDemo:
            case GameVersion.Korea:
            case GameVersion.Japan:
                return new BaseCharacterDeserializer();

            default:
                throw new InvalidOperationException($"Unknown game version {_ndsInfo.Rom.Version}.");
        }
    }

    private AnimationState CreateAnimationState(AnimationSequences animations, string? animationName = null)
    {
        var frames = new ImageResource[animations.Frames.Length];
        for (var j = 0; j < animations.Frames.Length; j++)
        {
            var options = new DrawingOptions { GraphicsOptions = new GraphicsOptions { AlphaCompositionMode = PixelAlphaCompositionMode.Src } };
            Image<Rgba32> frame = animations.Frames[j].Image.Clone(c => c
                .Fill(options, new RecolorBrush(Color.FromRgb(0x00, 0xff, 0x0), Color.Transparent, .0f))
                .Fill(options, new RecolorBrush(Color.FromRgb(0x00, 0xf7, 0x0), Color.Transparent, .0f)));

            frames[j] = ImageResource.FromImage(frame);
        }

        return new AnimationState
        {
            Animations = animations,
            Images = frames,
            FrameSpeed = 1f,
            ActiveAnimation = animationName is null ? animations.Sequences[0] : animations.Sequences.FirstOrDefault(s => s.Name == animationName)
        };
    }

    private AnimationState CloneAnimationState(AnimationState state, string? animationName = null)
    {
        return new AnimationState
        {
            Animations = state.Animations,
            Images = state.Images,
            FrameSpeed = 1f,
            ActiveAnimation = animationName is null ? state.Animations.Sequences[0] : state.Animations.Sequences.FirstOrDefault(s => s.Name == animationName)
        };
    }

    private AnimationSequences? GetAnimation(string animationPath)
    {
        if (!_fileManager.TryGet(_ndsInfo.Rom, animationPath, out Layton1NdsFile? file))
            return null;

        return (AnimationSequences?)_fileManager.Parse(file, out _);
    }

    private ImageResource? GetImage(string imagePath)
    {
        if (!_fileManager.TryGet(_ndsInfo.Rom, imagePath, out Layton1NdsFile? file))
            return null;

        var image = (Image<Rgba32>?)_fileManager.Parse(file, FileType.Bgx);

        return image is null ? null : ImageResource.FromImage(image);
    }

    private void LoadEventWindows()
    {
        string eventWindowPath = GetEventWindowPath();
        _eventWindowAnimations ??= GetAnimation(eventWindowPath);
    }

    private void LoadEventCursor()
    {
        string eventCursorPath = GetEventCursorPath();
        _eventCursorAnimations ??= GetAnimation(eventCursorPath);
    }

    private string GetAnimationObjectPath(string objectPath)
    {
        string filePath = _pathProvider.GetFullDirectory($"ani/{objectPath}".Replace(".spr", ".arc"), _ndsInfo.Rom.Version);

        return filePath;
    }

    private string GetEventWindowPath()
    {
        string filePath = _pathProvider.GetFullDirectory("ani/event_window_1.arc", _ndsInfo.Rom.Version);

        return filePath;
    }

    private string GetEventCursorPath()
    {
        string filePath = _pathProvider.GetFullDirectory("ani/cursor_wait.arc", _ndsInfo.Rom.Version);

        return filePath;
    }

    private string GetBaseTextPath(int eventId, GameVersion version, TextLanguage language)
    {
        switch (version)
        {
            case GameVersion.Usa:
            case GameVersion.UsaDemo:
            case GameVersion.Japan:
                return _pathProvider.GetFullDirectory("etext/", version, language);

            case GameVersion.Europe:
            case GameVersion.EuropeDemo:
            case GameVersion.Korea:
            case GameVersion.JapanFriendly:
                string group = eventId < 100 ? "000" : eventId < 200 ? "100" : "200";
                return _pathProvider.GetFullDirectory("etext/", version, language) + $"e{group}.pcm";

            default:
                throw new InvalidOperationException($"Unknown game version {version}.");
        }
    }

    private string? GetEventText(int eventId, int textId, GameVersion version, TextLanguage language)
    {
        string textPath = GetBaseTextPath(eventId, version, language);
        switch (_ndsInfo.Rom.Version)
        {
            case GameVersion.Usa:
            case GameVersion.UsaDemo:
            case GameVersion.Japan:
                return GetText(textPath + $"e{eventId}_t{textId}.txt");

            case GameVersion.Europe:
            case GameVersion.EuropeDemo:
            case GameVersion.Korea:
            case GameVersion.JapanFriendly:
                return GetPcmText(textPath, $"e{eventId}_t{textId}.txt");

            default:
                throw new InvalidOperationException($"Unknown game version {_ndsInfo.Rom.Version}.");
        }
    }

    private string? GetText(string filePath)
    {
        if (!_fileManager.TryGet(_ndsInfo.Rom, filePath, out Layton1NdsFile? ndsFile))
            return null;

        return _fileManager.Parse(ndsFile, FileType.Text) as string;
    }

    private string? GetPcmText(string filePath, string entryName)
    {
        if (!_fileManager.TryGet(_ndsInfo.Rom, filePath, out Layton1NdsFile? ndsFile))
            return null;

        object? content = _fileManager.Parse(ndsFile, FileType.Pcm);

        return GetPcmText(content, entryName);
    }

    private string? GetPcmText(object? content, string entryName)
    {
        if (content is not List<PcmFile> pcmFiles)
            return null;

        PcmFile? pcmFile = pcmFiles.FirstOrDefault(f => f.Name == entryName);

        if (pcmFile is null)
            return null;

        return _pcmManager.Parse(pcmFile, FileType.Text, _ndsInfo.Rom.Version) as string;
    }
}