using CrossCutting.Core.Contract.EventBrokerage;
using ImGui.Forms.Controls;
using ImGui.Forms.Controls.Text;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Messages.DataClasses;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Forms.Rooms;

internal partial class RoomFlagsForm
{
    private readonly Layton1NdsInfo _ndsInfo;

    private readonly IEventBroker _eventBroker;
    private readonly ILocalizationProvider _localizations;

    private int? _roomId;
    private RoomFlagStates? _states;

    public RoomFlagsForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ILocalizationProvider localizations)
    {
        InitializeComponent();

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

    private void UpdateStates(RoomFlagStates states)
    {
        _flag1List.Items.Clear();
        _flag2List.Items.Clear();

        foreach (int flag1 in states.Flags1.Keys)
        {
            var flagBox = new CheckBox(_localizations.RoomFlagCaption(flag1)) { Checked = states.Flags1[flag1] };
            flagBox.CheckChanged += (_, _) => ToggleFlag1(flag1, flagBox.Checked);

            _flag1List.Items.Add(flagBox);
        }

        foreach (int flag2 in states.Flags2.Keys)
        {
            var flagBox = new CheckBox(_localizations.RoomFlagCaption(flag2)) { Checked = states.Flags2[flag2] };
            flagBox.CheckChanged += (_, _) => ToggleFlag2(flag2, flagBox.Checked);

            _flag2List.Items.Add(flagBox);
        }

        _stateText.TextChanged -= StateBox_TextChanged;
        _stateText.Text = $"{states.State}";
        _stateText.TextChanged += StateBox_TextChanged;

        _stateText.IsReadOnly = false;
    }

    private void StateBox_TextChanged(object? sender, EventArgs e)
    {
        if (sender is not TextBox stateBox)
            return;

        if (!int.TryParse(stateBox.Text, out int state))
            return;

        ToggleState(state);
    }

    private void ToggleState(int state)
    {
        if (_states is null)
            return;

        if (!_roomId.HasValue)
            return;

        _states.State = state;

        RaiseSelectedRoomFlagsModified(_roomId.Value);
    }

    private void ToggleFlag1(int flag, bool toggle)
    {
        if (_states is null)
            return;

        if (!_roomId.HasValue)
            return;

        _states.Flags1[flag] = toggle;

        RaiseSelectedRoomFlagsModified(_roomId.Value);
    }

    private void ToggleFlag2(int flag, bool toggle)
    {
        if (_states is null)
            return;

        if (!_roomId.HasValue)
            return;

        _states.Flags2[flag] = toggle;

        RaiseSelectedRoomFlagsModified(_roomId.Value);
    }
}