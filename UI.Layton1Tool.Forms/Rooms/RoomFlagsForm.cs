using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Layouts;
using ImGui.Forms.Models;
using Logic.Business.Layton1ToolManagement.Contract.DataClasses;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Forms.Rooms;

internal partial class RoomFlagsForm
{
    private readonly Layton1NdsInfo _ndsInfo;

    private readonly IEventBroker _eventBroker;
    private readonly ILocalizationProvider _localizations;

    private int? _roomId;
    private GameState? _states;

    public RoomFlagsForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ILocalizationProvider localizations)
    {
        InitializeComponent(localizations);

        _ndsInfo = ndsInfo;
        _eventBroker = eventBroker;
        _localizations = localizations;

        eventBroker.Subscribe<RoomScriptUpdatedMessage>(ProcessRoomScriptUpdated);
        eventBroker.Subscribe<SelectedRoomFlagsUpdatedMessage>(ProcessSelectedRoomFlagsUpdated);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<RoomScriptUpdatedMessage>(ProcessRoomScriptUpdated);
    }

    private void ProcessRoomScriptUpdated(RoomScriptUpdatedMessage message)
    {
        if (_ndsInfo.Rom != message.Rom)
            return;

        UpdateStates(message.States);

        _roomId = message.Room;
        _states = message.States;
    }

    private void ProcessSelectedRoomFlagsUpdated(SelectedRoomFlagsUpdatedMessage message)
    {
        if (!_roomId.HasValue)
            return;

        if (_roomId.Value != message.Room)
            return;

        if (_ndsInfo.Rom != message.Rom)
            return;

        UpdateStates(message.States);

        _states = message.States;
    }

    private void RaiseSelectedRoomFlagsModified(int roomId)
    {
        _eventBroker.Raise(new SelectedRoomFlagsModifiedMessage(_ndsInfo.Rom, roomId));
    }

    private void UpdateStates(GameState states)
    {
        _puzzleChecks.Items.Clear();
        _byteFlags.Items.Clear();
        _bitFlags.Items.Clear();

        foreach (int puzzleId in states.Puzzles.Keys)
        {
            var seenBox = new CheckBox(_localizations.ScriptPuzzleSeenText) { Checked = states.Puzzles[puzzleId].Seen };
            seenBox.CheckChanged += (_, _) => TogglePuzzleSeen(puzzleId, seenBox.Checked);

            var solvedBox = new CheckBox(_localizations.ScriptPuzzleSolvedText) { Checked = states.Puzzles[puzzleId].Solved };
            solvedBox.CheckChanged += (_, _) => TogglePuzzleSolved(puzzleId, solvedBox.Checked);

            var finalSolvedBox = new CheckBox(_localizations.ScriptPuzzleFinalSolvedText) { Checked = states.Puzzles[puzzleId].FinalSolved };
            finalSolvedBox.CheckChanged += (_, _) => TogglePuzzleFinalSolved(puzzleId, finalSolvedBox.Checked);

            _puzzleChecks.Items.Add(new StackLayout
            {
                Alignment = Alignment.Vertical,
                Size = Size.Content,
                ItemSpacing = 5,
                Items =
                {
                    new Label(_localizations.ScriptPuzzleCaption(puzzleId)),
                    new StackLayout
                    {
                        Alignment = Alignment.Horizontal,
                        Size = Size.Content,
                        ItemSpacing = 5,
                        Items =
                        {
                            seenBox,
                            solvedBox,
                            finalSolvedBox
                        }
                    }
                }
            });
        }

        foreach (int byteFlag in states.ByteFlags.Keys)
        {
            var flagBox = new CheckBox(_localizations.ScriptFlagText(byteFlag)) { Checked = states.ByteFlags[byteFlag] };
            flagBox.CheckChanged += (_, _) => ToggleByteFlag(byteFlag, flagBox.Checked);

            _byteFlags.Items.Add(flagBox);
        }

        foreach (int bitFlag in states.BitFlags.Keys)
        {
            var flagBox = new CheckBox(_localizations.ScriptFlagText(bitFlag)) { Checked = states.BitFlags[bitFlag] };
            flagBox.CheckChanged += (_, _) => ToggleBitFlag(bitFlag, flagBox.Checked);

            _bitFlags.Items.Add(flagBox);
        }

        _scriptReturnBox.CheckChanged -= ScriptReturnBox_CheckChanged;
        _scriptReturnBox.Checked = states.IsScriptReturn;
        _scriptReturnBox.CheckChanged += ScriptReturnBox_CheckChanged;

        _scriptSolvedBox.CheckChanged -= ScriptSolvedBox_CheckChanged;
        _scriptSolvedBox.Checked = states.IsScriptSolved;
        _scriptSolvedBox.CheckChanged += ScriptSolvedBox_CheckChanged;

        _scriptReturnBox.Enabled = true;
        _scriptSolvedBox.Enabled = true;

        _solvedCountText.TextChanged -= SolvedCount_TextChanged;
        _solvedCountText.Text = $"{states.State}";
        _solvedCountText.TextChanged += SolvedCount_TextChanged;

        _stateText.TextChanged -= State_TextChanged;
        _stateText.Text = $"{states.DialogIndex}";
        _stateText.TextChanged += State_TextChanged;

        _dialogIndexText.TextChanged -= DialogIndex_TextChanged;
        _dialogIndexText.Text = $"{states.DialogIndex}";
        _dialogIndexText.TextChanged += DialogIndex_TextChanged;

        _solvedCountText.IsReadOnly = false;
        _stateText.IsReadOnly = false;
        _dialogIndexText.IsReadOnly = false;
    }

    private void ScriptReturnBox_CheckChanged(object? sender, EventArgs e)
    {
        if (_states is null)
            return;

        if (!_roomId.HasValue)
            return;

        _states.IsScriptReturn = _scriptReturnBox.Checked;

        RaiseSelectedRoomFlagsModified(_roomId.Value);
    }

    private void ScriptSolvedBox_CheckChanged(object? sender, EventArgs e)
    {
        if (_states is null)
            return;

        if (!_roomId.HasValue)
            return;

        _states.IsScriptSolved = _scriptSolvedBox.Checked;

        RaiseSelectedRoomFlagsModified(_roomId.Value);
    }

    private void SolvedCount_TextChanged(object? sender, EventArgs e)
    {
        if (_states is null)
            return;

        if (!_roomId.HasValue)
            return;

        if (!int.TryParse(_solvedCountText.Text, out int state))
            return;

        _states.SolvedCount = state;

        RaiseSelectedRoomFlagsModified(_roomId.Value);
    }

    private void State_TextChanged(object? sender, EventArgs e)
    {
        if (_states is null)
            return;

        if (!_roomId.HasValue)
            return;

        if (!int.TryParse(_stateText.Text, out int state))
            return;

        _states.State = state;

        RaiseSelectedRoomFlagsModified(_roomId.Value);
    }

    private void DialogIndex_TextChanged(object? sender, EventArgs e)
    {
        if (_states is null)
            return;

        if (!_roomId.HasValue)
            return;

        if (!int.TryParse(_dialogIndexText.Text, out int count))
            return;

        _states.DialogIndex = count;

        RaiseSelectedRoomFlagsModified(_roomId.Value);
    }

    private void TogglePuzzleSeen(int puzzleId, bool toggle)
    {
        if (_states is null)
            return;

        if (!_roomId.HasValue)
            return;

        if (_states.Puzzles.TryGetValue(puzzleId, out var flags))
            _states.Puzzles[puzzleId] = (toggle, flags.Solved, flags.FinalSolved);

        RaiseSelectedRoomFlagsModified(_roomId.Value);
    }

    private void TogglePuzzleSolved(int puzzleId, bool toggle)
    {
        if (_states is null)
            return;

        if (!_roomId.HasValue)
            return;

        if (_states.Puzzles.TryGetValue(puzzleId, out var flags))
            _states.Puzzles[puzzleId] = (flags.Seen, toggle, flags.FinalSolved);

        RaiseSelectedRoomFlagsModified(_roomId.Value);
    }

    private void TogglePuzzleFinalSolved(int puzzleId, bool toggle)
    {
        if (_states is null)
            return;

        if (!_roomId.HasValue)
            return;

        if (_states.Puzzles.TryGetValue(puzzleId, out var flags))
            _states.Puzzles[puzzleId] = (flags.Seen, flags.Solved, toggle);

        RaiseSelectedRoomFlagsModified(_roomId.Value);
    }

    private void ToggleByteFlag(int flag, bool toggle)
    {
        if (_states is null)
            return;

        if (!_roomId.HasValue)
            return;

        _states.ByteFlags[flag] = toggle;

        RaiseSelectedRoomFlagsModified(_roomId.Value);
    }

    private void ToggleBitFlag(int flag, bool toggle)
    {
        if (_states is null)
            return;

        if (!_roomId.HasValue)
            return;

        _states.BitFlags[flag] = toggle;

        RaiseSelectedRoomFlagsModified(_roomId.Value);
    }
}