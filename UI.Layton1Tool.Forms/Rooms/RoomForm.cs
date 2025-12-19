using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Controls.Tree;
using Logic.Business.Layton1ToolManagement.Contract;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.Contract.Enums.Texts;
using Logic.Business.Layton1ToolManagement.Contract.Files;
using Logic.Business.Layton1ToolManagement.Contract.Scripts;
using Logic.Domain.CodeAnalysisManagement.Contract.DataClasses.Layton1;
using Logic.Domain.CodeAnalysisManagement.Contract.Level5;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Forms.Rooms;

internal partial class RoomForm
{
    private readonly Layton1NdsInfo _ndsInfo;

    private readonly IEventBroker _eventBroker;
    private readonly ILayton1NdsFileManager _fileManager;
    private readonly ILayton1PathProvider _pathProvider;
    private readonly IColorProvider _colors;

    private readonly HashSet<int> _changedRooms = [];

    private List<int>? _roomIds;
    private int? _selectedRoomId;
    private GameState? _roomStates;

    public RoomForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ILayton1NdsFileManager fileManager, ILayton1PathProvider pathProvider,
        IFormFactory forms, IImageProvider images, ILocalizationProvider localizations, IColorProvider colors)
    {
        InitializeComponent(ndsInfo, forms, images, localizations);

        _ndsInfo = ndsInfo;
        _eventBroker = eventBroker;
        _fileManager = fileManager;
        _pathProvider = pathProvider;
        _colors = colors;

        _saveButton!.Clicked += _saveButton_Clicked;
        _saveAsButton!.Clicked += _saveAsButton_Clicked;
        _addButton!.Clicked += _addButton_Clicked;
        _roomTree!.SelectedNodeChanged += _roomTree_SelectedNodeChanged;
        _languageCombo!.SelectedItemChanged += _languageCombo_SelectedItemChanged;

        eventBroker.Subscribe<FileContentModifiedMessage>(ProcessFileContentModified);
        eventBroker.Subscribe<SelectedRoomParamsModifiedMessage>(ProcessSelectedRoomParamsModified);
        eventBroker.Subscribe<NdsFileSavedMessage>(ProcessNdsFileSaved);

        InitializeRoomList();
        UpdateSaveButtons();
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<FileContentModifiedMessage>(ProcessFileContentModified);
        _eventBroker.Unsubscribe<SelectedRoomParamsModifiedMessage>(ProcessSelectedRoomParamsModified);
        _eventBroker.Unsubscribe<NdsFileSavedMessage>(ProcessNdsFileSaved);
    }

    private async void _saveAsButton_Clicked(object? sender, EventArgs e)
    {
        await Save(true);
    }

    private async void _saveButton_Clicked(object? sender, EventArgs e)
    {
        await Save(false);
    }

    private void _addButton_Clicked(object? sender, EventArgs e)
    {
        if (_roomIds is null)
            return;

        int newRoomId = _roomIds.Max() + 1;

        string paramScriptPath = GetRoomParamsScriptPath(newRoomId);
        string inScriptPath = _pathProvider.GetFullDirectory($"script/rooms/room{newRoomId}_in.gds", _ndsInfo.Rom.Version);
        string outScriptPath = _pathProvider.GetFullDirectory($"script/rooms/room{newRoomId}_out.gds", _ndsInfo.Rom.Version);

        GdsScriptFile script = CreateEmptyScript();

        if (!_fileManager.TryGet(_ndsInfo.Rom, paramScriptPath, out _))
        {
            Layton1NdsFile newFile = _fileManager.Add(_ndsInfo.Rom, paramScriptPath, script, FileType.Gds, CompressionType.None);
            RaiseFileAdded(newFile);
        }

        if (!_fileManager.TryGet(_ndsInfo.Rom, inScriptPath, out _))
        {
            Layton1NdsFile newFile = _fileManager.Add(_ndsInfo.Rom, inScriptPath, script, FileType.Gds, CompressionType.None);
            RaiseFileAdded(newFile);
        }

        if (!_fileManager.TryGet(_ndsInfo.Rom, outScriptPath, out _))
        {
            Layton1NdsFile newFile = _fileManager.Add(_ndsInfo.Rom, outScriptPath, script, FileType.Gds, CompressionType.None);
            RaiseFileAdded(newFile);
        }

        _roomIds.Add(newRoomId);
        _changedRooms.Add(newRoomId);

        _roomTree.Nodes.Add(new TreeNode<int>
        {
            Text = $"Room {newRoomId}",
            Data = newRoomId
        });

        UpdateRoomList();
    }

    private void _roomTree_SelectedNodeChanged(object? sender, EventArgs e)
    {
        if (_roomTree.SelectedNode is null)
            return;

        _selectedRoomId = _roomTree.SelectedNode.Data;

        UpdateRoomScript(_roomTree.SelectedNode.Data);
    }

    private void _languageCombo_SelectedItemChanged(object? sender, EventArgs e)
    {
        RaiseSelectedRoomLanguageChanged(_languageCombo.SelectedItem.Content);
    }

    private void ProcessFileContentModified(FileContentModifiedMessage message)
    {
        if (message.Source == this)
            return;

        if (!_selectedRoomId.HasValue)
            return;

        if (message.File.Rom != _ndsInfo.Rom)
            return;

        if (message.File.Path == GetRoomParamsScriptPath(_selectedRoomId.Value))
        {
            _changedRooms.Add(_selectedRoomId.Value);

            UpdateRoomScript(_selectedRoomId.Value);
        }
    }

    private void ProcessSelectedRoomParamsModified(SelectedRoomParamsModifiedMessage message)
    {
        if (!_selectedRoomId.HasValue || _roomStates is null)
            return;

        if (message.Rom != _ndsInfo.Rom)
            return;

        string paramsScriptPath = GetRoomParamsScriptPath(_selectedRoomId.Value);
        if (_fileManager.TryGet(_ndsInfo.Rom, paramsScriptPath, out Layton1NdsFile? paramsScript))
        {
            _fileManager.Compose(paramsScript, message.Script, FileType.Gds);
            RaiseFileContentModified(paramsScript, message.Script);
        }

        _changedRooms.Add(message.Room);

        _roomStates = UpdateFlagStates(message.Script, _roomStates);
        RaiseSelectedRoomFlagsUpdated(_selectedRoomId.Value, _roomStates);

        UpdateSaveButtons();
        UpdateRoomList();
    }

    private void ProcessNdsFileSaved(NdsFileSavedMessage message)
    {
        if (!_selectedRoomId.HasValue)
            return;

        if (message.Rom != _ndsInfo.Rom)
            return;

        _changedRooms.Clear();

        UpdateRoomScript(_selectedRoomId.Value);
        UpdateSaveButtons();
        UpdateRoomList();
    }

    private async Task RaiseNdsFileSaveRequested(bool saveAs)
    {
        await _eventBroker.RaiseAsync(new NdsFileSaveRequestedMessage(_ndsInfo.Path, saveAs));
    }

    private void RaiseSelectedRoomLanguageChanged(TextLanguage language)
    {
        _eventBroker.Raise(new SelectedRoomLanguageChangedMessage(_ndsInfo.Rom, language));
    }

    private void RaiseRoomScriptUpdated(int room, GdsScriptFile script, GameState states)
    {
        _eventBroker.Raise(new RoomScriptUpdatedMessage(_roomParamForm, _ndsInfo.Rom, room, _languageCombo.SelectedItem.Content, script, states));
    }

    private void RaiseSelectedRoomFlagsUpdated(int room, GameState states)
    {
        _eventBroker.Raise(new SelectedRoomFlagsUpdatedMessage(_ndsInfo.Rom, room, states));
    }

    private void RaiseFileContentModified(Layton1NdsFile file, object? content)
    {
        _eventBroker.Raise(new FileContentModifiedMessage(this, file, content));
    }

    private void RaiseFileAdded(Layton1NdsFile file)
    {
        _eventBroker.Raise(new FileAddedMessage(this, file));
    }

    private async Task Save(bool saveAs)
    {
        if (_ndsInfo.Rom.Files.All(f => !f.IsChanged))
            return;

        await RaiseNdsFileSaveRequested(saveAs);
    }

    private void InitializeRoomList()
    {
        string roomDirectory = _pathProvider.GetFullDirectory("script/rooms/", _ndsInfo.Rom.Version);

        if (!_fileManager.TryGetDirectory(_ndsInfo.Rom, roomDirectory, out Layton1NdsFile[]? roomScripts))
            return;

        var roomIds = new HashSet<int>();

        foreach (Layton1NdsFile roomScript in roomScripts)
        {
            string fileName = Path.GetRelativePath(roomDirectory, roomScript.Path);
            if (!fileName.StartsWith("room", StringComparison.InvariantCulture))
                continue;

            int endIndex = fileName.IndexOf('_', 4);
            if (endIndex < 0)
                continue;

            string roomIdText = fileName[4..endIndex];
            if (!int.TryParse(roomIdText, out int roomId))
                continue;

            roomIds.Add(roomId);
        }

        _roomIds = roomIds.Order().ToList();

        foreach (int roomId in _roomIds)
        {
            string inScriptPath = _pathProvider.GetFullDirectory($"script/rooms/room{roomId}_in.gds", _ndsInfo.Rom.Version);
            _ = _fileManager.TryGet(_ndsInfo.Rom, inScriptPath, out Layton1NdsFile? inScript);

            string outScriptPath = _pathProvider.GetFullDirectory($"script/rooms/room{roomId}_out.gds", _ndsInfo.Rom.Version);
            _ = _fileManager.TryGet(_ndsInfo.Rom, outScriptPath, out Layton1NdsFile? outScript);

            string paramsScriptPath = GetRoomParamsScriptPath(roomId);
            _ = _fileManager.TryGet(_ndsInfo.Rom, paramsScriptPath, out Layton1NdsFile? paramsScript);

            if (inScript is null && outScript is null && paramsScript is null)
                continue;

            _roomTree.Nodes.Add(new TreeNode<int>
            {
                Text = $"Room {roomId}",
                Data = roomId
            });
        }
    }

    private GameState CreateFlagStates(GdsScriptFile script)
    {
        var states = new GameState
        {
            State = 1,
            DialogIndex = 1,
            SolvedCount = 0
        };

        foreach (GdsScriptInstruction instruction in script.Instructions)
        {
            if (instruction.Type is not 0)
                continue;

            if (instruction.Arguments.Length <= 1)
                continue;

            switch (instruction.Arguments[0].Value)
            {
                case 72:
                    if (instruction.Arguments[1].Value is not int flag1)
                        break;

                    states.Puzzles[flag1] = (false, false, false);
                    break;

                case 84:
                    if (instruction.Arguments[1].Value is not int flag4)
                        break;

                    states.Puzzles[flag4] = (false, false, false);
                    break;

                case 119:
                    if (instruction.Arguments[1].Value is not int solved)
                        break;

                    states.SolvedCount = solved;
                    break;

                case 88:
                    if (instruction.Arguments[1].Value is not int flag2)
                        break;

                    states.ByteFlags[flag2] = false;
                    break;

                case 141:
                    if (instruction.Arguments[1].Value is not int flag3)
                        break;

                    states.BitFlags[flag3] = false;
                    break;

                case 99:
                    if (instruction.Arguments[1].Value is not int state1)
                        break;

                    states.State = state1;
                    break;

                case 218:
                    if (instruction.Arguments[1].Value is not int state2)
                        break;

                    states.DialogIndex = state2;
                    break;
            }
        }

        return states;
    }

    private GameState UpdateFlagStates(GdsScriptFile script, GameState oldStates)
    {
        var states = new GameState
        {
            IsScriptReturn = oldStates.IsScriptReturn,
            IsScriptSolved = oldStates.IsScriptSolved,
            ReceivedUserInput = oldStates.ReceivedUserInput,
            State = oldStates.State,
            DialogIndex = oldStates.DialogIndex,
            SolvedCount = oldStates.SolvedCount
        };

        foreach (GdsScriptInstruction instruction in script.Instructions)
        {
            if (instruction.Type is not 0)
                continue;

            if (instruction.Arguments.Length <= 1)
                continue;

            switch (instruction.Arguments[0].Value)
            {
                case 72:
                    if (instruction.Arguments[1].Value is not int flag4)
                        break;

                    if (oldStates.Puzzles.TryGetValue(flag4, out (bool Seen, bool Solved, bool FinalSolved) oldFlag4))
                        states.Puzzles[flag4] = oldFlag4;
                    else
                        states.Puzzles[flag4] = (false, false, false);
                    break;

                case 84:
                    if (instruction.Arguments[1].Value is not int flag5)
                        break;

                    if (oldStates.Puzzles.TryGetValue(flag5, out (bool Seen, bool Solved, bool FinalSolved) oldFlag5))
                        states.Puzzles[flag5] = oldFlag5;
                    else
                        states.Puzzles[flag5] = (false, false, false);
                    break;

                case 88:
                    if (instruction.Arguments[1].Value is not int flag2)
                        break;

                    if (oldStates.ByteFlags.TryGetValue(flag2, out bool oldFlag2))
                        states.ByteFlags[flag2] = oldFlag2;
                    else
                        states.ByteFlags[flag2] = false;
                    break;

                case 141:
                    if (instruction.Arguments[1].Value is not int flag3)
                        break;

                    if (oldStates.BitFlags.TryGetValue(flag3, out bool oldFlag3))
                        states.BitFlags[flag3] = oldFlag3;
                    else
                        states.BitFlags[flag3] = false;
                    break;
            }
        }

        return states;
    }

    private void UpdateRoomScript(int roomId)
    {
        string paramsScriptPath = GetRoomParamsScriptPath(roomId);
        if (!_fileManager.TryGet(_ndsInfo.Rom, paramsScriptPath, out Layton1NdsFile? paramsScriptFile))
            return;

        if (_fileManager.Parse(paramsScriptFile, FileType.Gds) is not GdsScriptFile paramsScript)
            return;

        _roomStates = CreateFlagStates(paramsScript);

        RaiseRoomScriptUpdated(roomId, paramsScript, _roomStates);
    }

    private void UpdateRoomList()
    {
        foreach (TreeNode<int> node in _roomTree.Nodes)
            node.TextColor = _changedRooms.Contains(node.Data) ? _colors.Changed : _colors.Default;
    }

    private void UpdateSaveButtons()
    {
        _saveButton.Enabled = _saveAsButton.Enabled = _ndsInfo.Rom.Files.Any(f => f.IsChanged);
    }

    private static GdsScriptFile CreateEmptyScript()
    {
        return new GdsScriptFile
        {
            Instructions = [new GdsScriptInstruction { Type = 12 }]
        };
    }

    private string GetRoomParamsScriptPath(int roomId)
    {
        string filePath = _ndsInfo.Rom.Version is not GameVersion.Usa
            ? _pathProvider.GetFullDirectory($"script/rooms/room{roomId}_param.gds", _ndsInfo.Rom.Version)
            : _pathProvider.GetFullDirectory("script/rooms/", _ndsInfo.Rom.Version, TextLanguage.English) + $"room{roomId}_param.gds";

        return filePath;
    }
}