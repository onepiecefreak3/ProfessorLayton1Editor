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

namespace UI.Layton1Tool.Forms.Rooms.Views;

internal partial class RoomView
{
    private readonly Layton1NdsInfo _ndsInfo;

    private readonly IEventBroker _eventBroker;
    private readonly ILayton1NdsFileManager _fileManager;
    private readonly ILayton1PcmFileManager _pcmManager;
    private readonly ILayton1PathProvider _pathProvider;
    private readonly IAnimationStateManager _animationManager;
    private readonly IFontProvider _fontProvider;

    private int? _roomId;
    private TextLanguage? _language;
    private RoomFlagStates? _states;
    private RoomRenderSettings? _settings;

    private int? _mapId;
    private Image<Rgba32>? _mapBg;
    private int? _mapTextId;
    private string? _mapText;
    private ImageResource? _fullMapBg;
    private string? _roomPath;
    private ImageResource? _roomBg;
    private (int, int, AnimationState)? _icon;
    private List<string> _eventObjectPaths = [];
    private List<(int, int, int, int, AnimationState)> _eventObjects = [];
    private List<string> _animationObjectPaths = [];
    private List<(int, int, AnimationState)> _animationObjects = [];
    private List<string> _arrowObjectPaths = [];
    private List<(int, int, int, AnimationState)> _arrowObjects = [];
    private readonly List<(int, int, int, int)> _textAreas = [];
    private readonly List<(int, int, int, int)> _hintAreas = [];

    private GdsScriptFile? _script;

    public RoomView(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ILayton1NdsFileManager fileManager, ILayton1PcmFileManager pcmManager,
        ILayton1PathProvider pathProvider, IAnimationStateManager animationManager, IFontProvider fontProvider)
    {
        _ndsInfo = ndsInfo;
        _eventBroker = eventBroker;
        _fileManager = fileManager;
        _pcmManager = pcmManager;
        _pathProvider = pathProvider;
        _animationManager = animationManager;
        _fontProvider = fontProvider;

        eventBroker.Subscribe<FileContentModifiedMessage>(ProcessFileContentModified);
        eventBroker.Subscribe<FileAddedMessage>(ProcessFileAdded);
        eventBroker.Subscribe<FontModifiedMessage>(ProcessFontModified);
        eventBroker.Subscribe<SelectedRoomLanguageChangedMessage>(ProcessSelectedRoomLanguageChanged);
        eventBroker.Subscribe<SelectedRoomParamsModifiedMessage>(ProcessSelectedRoomParamsModified);
        eventBroker.Subscribe<SelectedRoomFlagsUpdatedMessage>(ProcessSelectedRoomFlagsUpdated);
        eventBroker.Subscribe<RoomScriptUpdatedMessage>(ProcessRoomScriptUpdated);
        eventBroker.Subscribe<SelectedRoomFlagsModifiedMessage>(ProcessSelectedRoomFlagsModified);
        eventBroker.Subscribe<SelectedRoomRenderSettingsModifiedMessage>(ProcessSelectedRoomRenderSettingsModified);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<FileContentModifiedMessage>(ProcessFileContentModified);
        _eventBroker.Unsubscribe<FileAddedMessage>(ProcessFileAdded);
        _eventBroker.Unsubscribe<SelectedRoomLanguageChangedMessage>(ProcessSelectedRoomLanguageChanged);
        _eventBroker.Unsubscribe<SelectedRoomParamsModifiedMessage>(ProcessSelectedRoomParamsModified);
        _eventBroker.Unsubscribe<SelectedRoomFlagsUpdatedMessage>(ProcessSelectedRoomFlagsUpdated);
        _eventBroker.Unsubscribe<RoomScriptUpdatedMessage>(ProcessRoomScriptUpdated);
        _eventBroker.Unsubscribe<SelectedRoomFlagsModifiedMessage>(ProcessSelectedRoomFlagsModified);
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
        if (_mapBg is null || _mapText is null)
            return;

        if (message.File.Rom != _ndsInfo.Rom)
            return;

        if (message.Type is not FontType.Event)
            return;

        RenderMapBackground(_mapBg, _mapText);
    }

    private void UpdateResources(Layton1NdsFile file, object? content)
    {
        if (!_roomId.HasValue || !_mapId.HasValue || !_language.HasValue)
            return;

        if (file.Path == GetMapTextPath(_language.Value))
        {
            if (_mapTextId.HasValue && _mapBg is not null)
            {
                _mapText = GetPcmText(content, $"map{_mapTextId}.txt");

                if (_mapText is not null)
                    RenderMapBackground(_mapBg, _mapText);
            }
        }
        else if (file.Path == GetMapBackgroundPath(_mapId.Value) && content is Image<Rgba32> newImage)
        {
            _mapBg = newImage;

            if (_mapText is not null)
                RenderMapBackground(newImage, _mapText);
        }
        else if (file.Path == _roomPath && content is Image<Rgba32> newImage1)
        {
            _roomBg?.Destroy();
            _roomBg = ImageResource.FromImage(newImage1);
        }
        else if (file.Path == GetRoomBackgroundPath(_roomId.Value) && content is Image<Rgba32> newImage2)
        {
            _roomBg?.Destroy();
            _roomBg = ImageResource.FromImage(newImage2);
        }
        else if (content is AnimationSequences newAnimations)
        {
            int eventIndex = _eventObjectPaths.FindIndex(p => p == file.Path);
            if (eventIndex > 0)
            {
                foreach (ImageResource image in _eventObjects[eventIndex].Item5.Images)
                    image.Destroy();

                _eventObjects[eventIndex] = _eventObjects[eventIndex] with { Item5 = CreateAnimationState(newAnimations) };
                return;
            }

            int animationIndex = _animationObjectPaths.FindIndex(p => p == file.Path);
            if (animationIndex > 0)
            {
                foreach (ImageResource image in _animationObjects[animationIndex].Item3.Images)
                    image.Destroy();

                _animationObjects[animationIndex] = _animationObjects[animationIndex] with { Item3 = CreateAnimationState(newAnimations) };
                return;
            }

            int arrowIndex = _arrowObjectPaths.FindIndex(p => p == file.Path);
            if (arrowIndex > 0)
            {
                foreach (ImageResource image in _arrowObjects[arrowIndex].Item4.Images)
                    image.Destroy();

                _arrowObjects[arrowIndex] = _arrowObjects[arrowIndex] with { Item4 = CreateAnimationState(newAnimations) };
            }
        }
    }

    private void ProcessSelectedRoomLanguageChanged(SelectedRoomLanguageChangedMessage message)
    {
        if (!_roomId.HasValue)
            return;

        if (message.Rom != _ndsInfo.Rom)
            return;

        if (_script is not null && _states is not null)
            UpdateResources(_script, _roomId.Value, message.Language, _states);

        _language = message.Language;
    }

    private void ProcessSelectedRoomParamsModified(SelectedRoomParamsModifiedMessage message)
    {
        if (message.Rom != _ndsInfo.Rom)
            return;

        if (_roomId.HasValue && _roomId != message.Room)
            return;

        if (_language.HasValue && _states is not null)
            UpdateResources(message.Script, message.Room, _language.Value, _states);

        _script = message.Script;
    }

    private void ProcessSelectedRoomFlagsUpdated(SelectedRoomFlagsUpdatedMessage message)
    {
        if (message.Rom != _ndsInfo.Rom)
            return;

        if (_roomId.HasValue && _roomId != message.Room)
            return;

        if (_language.HasValue && _script is not null)
            UpdateResources(_script, message.Room, _language.Value, message.States);

        _states = message.States;
    }

    private void ProcessRoomScriptUpdated(RoomScriptUpdatedMessage message)
    {
        if (message.Rom != _ndsInfo.Rom)
            return;

        UpdateResources(message.Script, message.Room, message.Language, message.States);

        _roomId = message.Room;
        _language = message.Language;
        _states = message.States;
        _script = message.Script;
    }

    private void ProcessSelectedRoomFlagsModified(SelectedRoomFlagsModifiedMessage message)
    {
        if (_script is null || _states is null || !_roomId.HasValue || !_language.HasValue)
            return;

        if (message.Rom != _ndsInfo.Rom)
            return;

        if (_roomId != message.Room)
            return;

        UpdateResources(_script, _roomId.Value, _language.Value, _states);
    }

    private void ProcessSelectedRoomRenderSettingsModified(SelectedRoomRenderSettingsModifiedMessage message)
    {
        _settings = message.Settings;
    }

    private void UpdateResources(GdsScriptFile script, int roomId, TextLanguage language, RoomFlagStates states)
    {
        var hasMap = false;
        var hasRoomImage = false;

        _textAreas.Clear();
        _hintAreas.Clear();

        var eventObjectPaths = new List<string>();
        var eventObjects = new List<(int, int, int, int, AnimationState)>();
        var animationObjectPaths = new List<string>();
        var animationObjects = new List<(int, int, AnimationState)>();
        var arrowObjectPaths = new List<string>();
        var arrowObjects = new List<(int, int, int, AnimationState)>();

        _ = SetupRoomImageResource(roomId);

        var jumped = false;
        var negate = false;
        for (var i = 0; i < script.Instructions.Count; i++)
        {
            GdsScriptInstruction instruction = script.Instructions[i];

            var negateLocal = negate;
            negate = false;

            if (instruction.Type is not 0)
            {
                if (instruction.Type is 8)
                    negate = true;
            }
            else if (instruction.Arguments.Length > 1 && instruction.Arguments[0].Value is 23)
            {
                if (jumped)
                {
                    jumped = false;
                    continue;
                }

                jumped = false;

                if (instruction.Arguments[1].Value is not string label)
                    continue;

                int targetInstructionIndex = script.Instructions.FindIndex(x => x.Jump?.Label == label);
                if (targetInstructionIndex >= 0)
                    i = targetInstructionIndex - 1;
            }
            else if (instruction.Arguments.Length > 2 && instruction.Arguments[0].Value is 88 or 141 or 99)
            {
                jumped = false;

                string label;
                if (instruction.Arguments[0].Value is 88 or 141)
                {
                    IDictionary<int, bool> flags = instruction.Arguments[0].Value switch
                    {
                        88 => states.Flags1,
                        141 => states.Flags2,
                        _ => new Dictionary<int, bool>()
                    };

                    if (instruction.Arguments[1].Value is not int flag)
                        continue;

                    if (negateLocal ? flags.TryGetValue(flag, out bool toggle1) && !toggle1 : flags.TryGetValue(flag, out bool toggle) && toggle)
                        continue;

                    if (instruction.Arguments[2].Value is not string label1)
                        continue;

                    label = label1;
                }
                else
                {
                    if (instruction.Arguments[1].Value is not int state)
                        continue;

                    if (negateLocal ? states.State != state : states.State == state)
                        continue;

                    if (instruction.Arguments[2].Value is not string label1)
                        continue;

                    label = label1;
                }

                int targetInstructionIndex = script.Instructions.FindIndex(x => x.Jump?.Label == label);
                if (targetInstructionIndex >= 0)
                {
                    i = targetInstructionIndex - 1;
                    jumped = true;
                }
            }
            else if (instruction.Arguments.Length > 1 && instruction.Arguments[0].Value is 11)
            {
                if (instruction.Arguments[1].Value is string imagePath)
                {
                    imagePath = _pathProvider.GetFullDirectory($"bg/{imagePath}".Replace(".bgx", ".arc"),
                        _ndsInfo.Rom.Version, language);
                    ImageResource? roomBg = GetImage(imagePath);

                    if (roomBg is not null)
                    {
                        hasRoomImage = true;

                        _roomPath = imagePath;
                        _roomBg = roomBg;
                    }
                }
            }
            else if (instruction.Arguments.Length > 5 && instruction.Arguments[0].Value is 67)
            {
                if (instruction.Arguments[2].Value is int x && instruction.Arguments[3].Value is int y &&
                    instruction.Arguments[4].Value is int width && instruction.Arguments[5].Value is int height)
                    _textAreas.Add((x, y, width, height));
            }
            else if (instruction.Arguments.Length > 5 && instruction.Arguments[0].Value is 80)
            {
                if (instruction.Arguments[5].Value is not int objectId)
                    continue;

                string objectPath = GetEventObjectPath(objectId);
                int objectIndex = _eventObjectPaths.IndexOf(objectPath);

                if (instruction.Arguments[1].Value is int x && instruction.Arguments[2].Value is int y &&
                    instruction.Arguments[3].Value is int width && instruction.Arguments[4].Value is int height)
                {
                    if (objectIndex >= 0)
                    {
                        eventObjectPaths.Add(objectPath);
                        eventObjects.Add((x, y, width, height, _eventObjects[objectIndex].Item5));
                        continue;
                    }

                    AnimationSequences? animations = GetAnimation(objectPath);
                    if (animations is null)
                        continue;

                    if (animations.Sequences.Length <= 0)
                        continue;

                    eventObjectPaths.Add(objectPath);
                    eventObjects.Add((x, y, width, height, CreateAnimationState(animations)));
                }
            }
            else if (instruction.Arguments.Length > 3 && instruction.Arguments[0].Value is 92)
            {
                if (instruction.Arguments[3].Value is not string objectPath)
                    continue;

                objectPath = GetAnimationObjectPath(objectPath);
                int objectIndex = _animationObjectPaths.IndexOf(objectPath);

                if (instruction.Arguments[1].Value is int x && instruction.Arguments[2].Value is int y)
                {
                    if (objectIndex >= 0)
                    {
                        animationObjectPaths.Add(objectPath);
                        animationObjects.Add((x, y, _animationObjects[objectIndex].Item3));
                        continue;
                    }

                    AnimationSequences? animations = GetAnimation(objectPath);
                    if (animations is null)
                        continue;

                    if (animations.Sequences.Length <= 0)
                        continue;

                    animationObjectPaths.Add(objectPath);
                    animationObjects.Add((x, y, CreateAnimationState(animations)));
                }
            }
            else if (instruction.Arguments.Length > 8 && instruction.Arguments[0].Value is 34)
            {
                if (instruction.Arguments[1].Value is not int arrowId)
                    continue;

                string arrowPath = GetArrowObjectPath(arrowId);
                int objectIndex = _arrowObjectPaths.IndexOf(arrowPath);

                if (instruction.Arguments[2].Value is int x && instruction.Arguments[3].Value is int y)
                {
                    if (objectIndex >= 0)
                    {
                        arrowObjectPaths.Add(arrowPath);
                        arrowObjects.Add((x, y, arrowId, _arrowObjects[objectIndex].Item4));
                        continue;
                    }

                    AnimationSequences? animations = GetAnimation(arrowPath);
                    if (animations is null)
                        continue;

                    if (animations.Sequences.Length <= 0)
                        continue;

                    arrowObjectPaths.Add(arrowPath);
                    arrowObjects.Add((x, y, arrowId, CreateAnimationState(animations)));
                }
            }
            else if (instruction.Arguments.Length > 2 && instruction.Arguments[0].Value is 90)
            {
                if (instruction.Arguments[2].Value is int mapId)
                    hasMap |= SetupMapImageResource(mapId);

                if (hasMap)
                {
                    if (instruction.Arguments[1].Value is string mapText && _mapBg is not null)
                    {
                        _mapText = mapText;
                        RenderMapBackground(_mapBg, mapText);
                    }
                    else if (instruction.Arguments[1].Value is int mapTextId && _mapBg is not null)
                    {
                        _mapTextId = mapTextId;

                        string textPath = GetMapTextPath(language);
                        string? mapText1 = GetPcmText(textPath, $"map{mapTextId}.txt");
                        if (mapText1 is null)
                            continue;

                        _mapText = mapText1;
                        RenderMapBackground(_mapBg, mapText1);
                    }

                    if (instruction.Arguments[3].Value is int x && instruction.Arguments[4].Value is int y)
                    {
                        AnimationSequences? animations = GetAnimation(GetMapIconPath());
                        if (animations is null)
                            continue;

                        if (animations.Sequences.Length <= 0)
                            continue;

                        _icon = (x, y, CreateAnimationState(animations));
                    }
                }
            }
            else if (instruction.Arguments.Length > 5 && instruction.Arguments[0].Value is 104)
            {
                if (instruction.Arguments[2].Value is int x && instruction.Arguments[3].Value is int y &&
                    instruction.Arguments[4].Value is int width && instruction.Arguments[5].Value is int height)
                    _hintAreas.Add((x, y, width, height));
            }
        }

        if (!hasMap)
        {
            _icon = null;
            _mapId = null;
            _mapBg = null;
            _fullMapBg?.Destroy();
            _fullMapBg = null;
            _mapTextId = null;
            _mapText = null;
        }

        if (!hasRoomImage)
            _roomPath = null;

        _eventObjectPaths = eventObjectPaths;
        _eventObjects = eventObjects;
        _animationObjectPaths = animationObjectPaths;
        _animationObjects = animationObjects;
        _arrowObjectPaths = arrowObjectPaths;
        _arrowObjects = arrowObjects;
    }

    private void RenderMapBackground(Image<Rgba32> mapBg, string mapText)
    {
        IGlyphProvider? font = _fontProvider.GetEventFont(_ndsInfo.Rom);
        if (font is null)
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
                layouter = new TextLayouter(new LayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Center }, font);
                renderer = new TextRenderer(new RenderOptions { TextColor = Color.Black }, font);
                pos = new Point(79, 7);
                break;

            case GameVersion.Usa:
            case GameVersion.UsaDemo:
                deserializer = new CharacterDeserializer();
                layouter = new TextLayouter(new LayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Center }, font);
                renderer = new TextRenderer(new RenderOptions { TextColor = Color.Black }, font);
                pos = new Point(83, 7);
                break;

            case GameVersion.Korea:
                deserializer = new CharacterDeserializer();
                layouter = new TextLayouter(new LayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Center }, font);
                renderer = new TextRenderer(new RenderOptions { TextColor = Color.Black }, font);
                pos = new Point(79, 5);
                break;

            case GameVersion.JapanFriendly:
                deserializer = new CharacterDeserializer();
                layouter = new TextLayouter(new LayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Center }, font);
                renderer = new TextRenderer(new RenderOptions { TextColor = Color.Black }, font);
                pos = new Point(79, 7);
                break;

            case GameVersion.Japan:
                deserializer = new CharacterDeserializer();
                layouter = new TextLayouter(new LayoutOptions { HorizontalAlignment = HorizontalTextAlignment.Center }, font);
                renderer = new TextRenderer(new RenderOptions { TextColor = Color.Black }, font);
                pos = new Point(77, 7);
                break;

            default:
                throw new InvalidOperationException($"Unknown game version {_ndsInfo.Rom.Version}.");
        }

        var fullMapImage = mapBg.Clone();

        var layout = layouter.Create(deserializer.Deserialize(mapText), pos, new Size(256, 192));
        renderer.Render(fullMapImage, layout);

        fullMapImage.SaveAsPng(@"E:\reverse_engineering\professor_layton_1\map.png");

        _fullMapBg = ImageResource.FromImage(fullMapImage);
    }

    private AnimationState CreateAnimationState(AnimationSequences animations)
    {
        var frames = new ImageResource[animations.Frames.Length];
        for (var j = 0; j < animations.Frames.Length; j++)
        {
            var options = new DrawingOptions { GraphicsOptions = new GraphicsOptions { AlphaCompositionMode = PixelAlphaCompositionMode.Src } };
            Image<Rgba32> frame = animations.Frames[j].Image.Clone(c => c.Fill(options, new RecolorBrush(Color.FromRgb(0x00, 0xff, 0x0), Color.Transparent, .0f)));

            frames[j] = ImageResource.FromImage(frame);
        }

        return new AnimationState
        {
            Animations = animations,
            Images = frames,
            FrameSpeed = 1f,
            ActiveAnimation = animations.Sequences[0]
        };
    }

    private bool SetupRoomImageResource(int roomId)
    {
        if (_roomId.HasValue)
        {
            if (roomId != _roomId.Value)
                _roomBg = GetRoomBackground(roomId);
            else
                _roomBg ??= GetRoomBackground(roomId);
        }
        else
        {
            _roomBg ??= GetRoomBackground(roomId);
        }

        return _roomBg is not null;
    }

    private bool SetupMapImageResource(int mapId)
    {
        if (_mapId.HasValue)
        {
            if (mapId != _mapId.Value)
                _mapBg = GetMapBackground(mapId);
            else
                _mapBg ??= GetMapBackground(mapId);
        }
        else
        {
            _mapBg ??= GetMapBackground(mapId);
        }

        _mapId = mapId;

        return _mapBg is not null;
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

    private ImageResource? GetRoomBackground(int roomId)
    {
        string filePath = GetRoomBackgroundPath(roomId);

        if (!_fileManager.TryGet(_ndsInfo.Rom, filePath, out Layton1NdsFile? file))
            return null;

        var image = (Image<Rgba32>?)_fileManager.Parse(file, FileType.Bgx);

        return image is null ? null : ImageResource.FromImage(image);
    }

    private Image<Rgba32>? GetMapBackground(int mapId)
    {
        string filePath = GetMapBackgroundPath(mapId);

        if (!_fileManager.TryGet(_ndsInfo.Rom, filePath, out Layton1NdsFile? file))
            return null;

        return (Image<Rgba32>?)_fileManager.Parse(file, FileType.Bgx);
    }

    private string GetArrowObjectPath(int arrowId)
    {
        if (arrowId is 0)
            return _pathProvider.GetFullDirectory("ani/exit_door.arc", _ndsInfo.Rom.Version);

        string filePath = _pathProvider.GetFullDirectory($"ani/exit_{arrowId}.arc", _ndsInfo.Rom.Version);

        return filePath;
    }

    private string GetMapIconPath()
    {
        string filePath = _pathProvider.GetFullDirectory("ani/mapicon.arj", _ndsInfo.Rom.Version);

        return filePath;
    }

    private string GetAnimationObjectPath(string objectPath)
    {
        string filePath = _pathProvider.GetFullDirectory($"ani/{objectPath}".Replace(".spr", ".arc"), _ndsInfo.Rom.Version);

        return filePath;
    }

    private string GetEventObjectPath(int objectId)
    {
        string filePath = _pathProvider.GetFullDirectory($"ani/obj_{objectId}.arc", _ndsInfo.Rom.Version);

        return filePath;
    }

    private string GetRoomBackgroundPath(int roomId)
    {
        string filePath = _pathProvider.GetFullDirectory($"bg/room_{roomId}_bg.arc", _ndsInfo.Rom.Version);

        return filePath;
    }

    private string GetMapBackgroundPath(int mapId)
    {
        string filePath = _pathProvider.GetFullDirectory($"bg/map_{mapId}.arc", _ndsInfo.Rom.Version);

        return filePath;
    }

    private string GetMapTextPath(TextLanguage language)
    {
        string filePath = _pathProvider.GetFullDirectory("storytext/", _ndsInfo.Rom.Version, language) + "storytext.pcm";

        return filePath;
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