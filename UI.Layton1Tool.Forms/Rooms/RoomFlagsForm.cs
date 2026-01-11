using CrossCutting.Core.Contract.EventBrokerage;
using UI.Layton1Tool.Forms.Contract.DataClasses;
using UI.Layton1Tool.Messages;
using UI.Layton1Tool.Resources.Contract;

namespace UI.Layton1Tool.Forms.Rooms;

internal class RoomFlagsForm : FlagsForm
{
    private readonly Layton1NdsInfo _ndsInfo;

    private readonly IEventBroker _eventBroker;

    private int? _roomId;

    public RoomFlagsForm(Layton1NdsInfo ndsInfo, IEventBroker eventBroker, ILocalizationProvider localizations) : base(localizations)
    {
        _ndsInfo = ndsInfo;
        _eventBroker = eventBroker;

        eventBroker.Subscribe<RoomScriptUpdatedMessage>(ProcessRoomScriptUpdated);
        eventBroker.Subscribe<SelectedRoomFlagsUpdatedMessage>(ProcessSelectedRoomFlagsUpdated);
    }

    public override void Destroy()
    {
        _eventBroker.Unsubscribe<RoomScriptUpdatedMessage>(ProcessRoomScriptUpdated);
        _eventBroker.Unsubscribe<SelectedRoomFlagsUpdatedMessage>(ProcessSelectedRoomFlagsUpdated);
    }

    private void ProcessRoomScriptUpdated(RoomScriptUpdatedMessage message)
    {
        if (_ndsInfo.Rom != message.Rom)
            return;

        UpdateStates(message.States);

        _roomId = message.Room;
        States = message.States;
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

        States = message.States;
    }

    private void RaiseSelectedRoomFlagsModified(int roomId)
    {
        _eventBroker.Raise(new SelectedRoomFlagsModifiedMessage(_ndsInfo.Rom, roomId));
    }

    protected override void OnFlagsChanged()
    {
        if (!_roomId.HasValue)
            return;

        RaiseSelectedRoomFlagsModified(_roomId.Value);
    }
}