using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Controls.Tree;
using Logic.Business.Layton1ToolManagement.Contract;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using Logic.Business.Layton1ToolManagement.Contract.Enums;
using Logic.Business.Layton1ToolManagement.Contract.Enums.Texts;
using Logic.Business.Layton1ToolManagement.Contract.Files;
using Logic.Domain.Level5Management.Contract.DataClasses.Script.Gds;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Forms.Events;

internal partial class EventForm
{
    private readonly Layton1NdsInfo _ndsInfo;

    private readonly IEventBroker _eventBroker;
    private readonly ILayton1NdsFileManager _fileManager;
    private readonly ILayton1PathProvider _pathProvider;
    private readonly IColorProvider _colors;

    private readonly HashSet<int> _changedEvents = [];

    private List<int>? _eventIds;
    private int? _selectedEventId;
    private GameState? _eventStates;

    public EventForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ILayton1NdsFileManager fileManager, ILayton1PathProvider pathProvider,
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
        _eventTree!.SelectedNodeChanged += EventTreeSelectedNodeChanged;
        _languageCombo!.SelectedItemChanged += _languageCombo_SelectedItemChanged;

        eventBroker.Subscribe<FileContentModifiedMessage>(ProcessFileContentModified);
        eventBroker.Subscribe<SelectedEventModifiedMessage>(ProcessSelectedEventModified);
        eventBroker.Subscribe<NdsFileSavedMessage>(ProcessNdsFileSaved);

        InitializeEventList();
        UpdateSaveButtons();
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<FileContentModifiedMessage>(ProcessFileContentModified);
        _eventBroker.Unsubscribe<SelectedEventModifiedMessage>(ProcessSelectedEventModified);
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
        if (_eventIds is null)
            return;

        int newEventId = _eventIds.Max() + 1;

        string scriptPath = GetEventScriptPath(newEventId);

        GdsScriptFile script = CreateEmptyScript();

        if (!_fileManager.TryGet(_ndsInfo.Rom, scriptPath, out _))
        {
            Layton1NdsFile newFile = _fileManager.Add(_ndsInfo.Rom, scriptPath, script, FileType.Gds, CompressionType.None);
            RaiseFileAdded(newFile);
        }

        _eventIds.Add(newEventId);
        _changedEvents.Add(newEventId);

        _eventTree.Nodes.Add(new TreeNode<int>
        {
            Text = $"Event {newEventId}",
            Data = newEventId
        });

        UpdateEventList();
    }

    private void EventTreeSelectedNodeChanged(object? sender, EventArgs e)
    {
        if (_eventTree.SelectedNode is null)
            return;

        _selectedEventId = _eventTree.SelectedNode.Data;

        UpdateEventScript(_eventTree.SelectedNode.Data);
    }

    private void _languageCombo_SelectedItemChanged(object? sender, EventArgs e)
    {
        RaiseSelectedEventLanguageChanged(_languageCombo.SelectedItem.Content);
    }

    private void ProcessFileContentModified(FileContentModifiedMessage message)
    {
        if (message.Source == this)
            return;

        if (!_selectedEventId.HasValue)
            return;

        if (message.File.Rom != _ndsInfo.Rom)
            return;

        if (message.File.Path == GetEventScriptPath(_selectedEventId.Value))
        {
            _changedEvents.Add(_selectedEventId.Value);

            UpdateEventScript(_selectedEventId.Value);
        }
    }

    private void ProcessSelectedEventModified(SelectedEventModifiedMessage message)
    {
        if (!_selectedEventId.HasValue || _eventStates is null)
            return;

        if (message.Rom != _ndsInfo.Rom)
            return;

        string paramsScriptPath = GetEventScriptPath(_selectedEventId.Value);
        if (_fileManager.TryGet(_ndsInfo.Rom, paramsScriptPath, out Layton1NdsFile? paramsScript))
        {
            _fileManager.Compose(paramsScript, message.Script, FileType.Gds);
            RaiseFileContentModified(paramsScript, message.Script);
        }

        _changedEvents.Add(message.Event);

        _eventStates = UpdateFlagStates(message.Script, _eventStates);
        RaiseSelectedEventFlagsUpdated(_selectedEventId.Value, _eventStates);

        UpdateSaveButtons();
        UpdateEventList();
    }

    private void ProcessNdsFileSaved(NdsFileSavedMessage message)
    {
        if (!_selectedEventId.HasValue)
            return;

        if (message.Rom != _ndsInfo.Rom)
            return;

        _changedEvents.Clear();

        UpdateEventScript(_selectedEventId.Value);
        UpdateSaveButtons();
        UpdateEventList();
    }

    private async Task RaiseNdsFileSaveRequested(bool saveAs)
    {
        await _eventBroker.RaiseAsync(new NdsFileSaveRequestedMessage(_ndsInfo.Path, saveAs));
    }

    private void RaiseSelectedEventLanguageChanged(TextLanguage language)
    {
        _eventBroker.Raise(new SelectedEventLanguageChangedMessage(_ndsInfo.Rom, language));
    }

    private void RaiseEventScriptUpdated(int eventId, GdsScriptFile script, GameState states)
    {
        _eventBroker.Raise(new EventScriptUpdatedMessage(_eventScriptForm, _ndsInfo.Rom, eventId, _languageCombo.SelectedItem.Content, script, states));
    }

    private void RaiseSelectedEventFlagsUpdated(int eventId, GameState states)
    {
        _eventBroker.Raise(new SelectedEventFlagsUpdatedMessage(_ndsInfo.Rom, eventId, states));
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

    private void InitializeEventList()
    {
        string eventDirectory = _pathProvider.GetFullDirectory("script/event/", _ndsInfo.Rom.Version);

        if (!_fileManager.TryGetDirectory(_ndsInfo.Rom, eventDirectory, out Layton1NdsFile[]? eventScripts))
            return;

        var eventIds = new HashSet<int>();

        foreach (Layton1NdsFile eventScript in eventScripts)
        {
            string fileName = Path.GetRelativePath(eventDirectory, eventScript.Path);
            if (!fileName.StartsWith('e'))
                continue;

            int endIndex = fileName.IndexOf('.', 1);
            if (endIndex < 0)
                continue;

            string eventIdText = fileName[1..endIndex];
            if (!int.TryParse(eventIdText, out int eventId))
                continue;

            eventIds.Add(eventId);
        }

        _eventIds = eventIds.Order().ToList();

        foreach (int eventId in _eventIds)
        {
            string scriptPath = GetEventScriptPath(eventId);
            _ = _fileManager.TryGet(_ndsInfo.Rom, scriptPath, out Layton1NdsFile? script);

            if (script is null)
                continue;

            _eventTree.Nodes.Add(new TreeNode<int>
            {
                Text = $"Event {eventId}",
                Data = eventId
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

    private void UpdateEventScript(int eventId)
    {
        string paramsScriptPath = GetEventScriptPath(eventId);
        if (!_fileManager.TryGet(_ndsInfo.Rom, paramsScriptPath, out Layton1NdsFile? paramsScriptFile))
            return;

        if (_fileManager.Parse(paramsScriptFile, FileType.Gds) is not GdsScriptFile paramsScript)
            return;

        _eventStates = CreateFlagStates(paramsScript);

        RaiseEventScriptUpdated(eventId, paramsScript, _eventStates);
    }

    private void UpdateEventList()
    {
        foreach (TreeNode<int> node in _eventTree.Nodes)
            node.TextColor = _changedEvents.Contains(node.Data) ? _colors.Changed : _colors.Default;
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

    private string GetEventScriptPath(int eventId)
    {
        return _pathProvider.GetFullDirectory($"script/event/e{eventId}.gds", _ndsInfo.Rom.Version);
    }
}